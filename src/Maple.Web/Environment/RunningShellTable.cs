using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Maple.Web.Environment.Configuration;

namespace Maple.Web.Environment {
    /// <summary>
    /// 运行中的子站点配置信息存储器
    /// </summary>
    public interface IRunningShellTable {
        void Add(ShellSettings settings);
        void Remove(ShellSettings settings);
        void Update(ShellSettings settings);
        ShellSettings Match(HttpContextBase httpContext);
        ShellSettings Match(string host, string appRelativeCurrentExecutionFilePath);
    }

    /// <summary>
    /// 运行中的子站点配置信息存储器
    /// </summary>
    public class RunningShellTable : IRunningShellTable {
        private IEnumerable<ShellSettings> _shells = Enumerable.Empty<ShellSettings>();
        /// <summary>
        /// 子站点按照域名分组
        /// </summary>
        private IDictionary<string, IEnumerable<ShellSettings>> _shellsByHost;
        /// <summary>
        /// 已匹配的子站点缓存
        /// </summary>
        private readonly ConcurrentDictionary<string, ShellSettings> _shellsByHostAndPrefix = new ConcurrentDictionary<string, ShellSettings>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 默认/缺省的子站点设置
        /// </summary>
        private ShellSettings _fallback;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public void Add(ShellSettings settings) {
            _lock.EnterWriteLock();
            try {
                _shells = _shells
                    .Where(s => s.Name != settings.Name)
                    .Concat(new[] {settings})
                    .ToArray();

                Organize();
            }
            finally {
                _lock.ExitWriteLock();
            }
        }

        public void Remove(ShellSettings settings) {
            _lock.EnterWriteLock();
            try {
                _shells = _shells
                    .Where(s => s.Name != settings.Name)
                    .ToArray();

                Organize();
            }
            finally {
                _lock.ExitWriteLock();
            }
        }

        public void Update(ShellSettings settings) {
            _lock.EnterWriteLock();
            try {
                _shells = _shells
                    .Where(s => s.Name != settings.Name)
                    .ToArray();

                _shells = _shells
                    .Concat(new[] {settings})
                    .ToArray();

                Organize();
            }
            finally {
                _lock.ExitWriteLock();
            }
        }

        private void Organize() {
            //子站点域名 或 前缀不为空
            var qualified =
                _shells.Where(x => !string.IsNullOrEmpty(x.RequestUrlHost) || !string.IsNullOrEmpty(x.RequestUrlPrefix));
            //子站点域名 和 前缀均为空
            var unqualified = _shells
                .Where(x => string.IsNullOrEmpty(x.RequestUrlHost) && string.IsNullOrEmpty(x.RequestUrlPrefix))
                .ToList();
            //子站点按照域名分组
            _shellsByHost = qualified
                .SelectMany(s => s.RequestUrlHost == null || s.RequestUrlHost.IndexOf(',') == -1 ? new[] {s} : 
                    s.RequestUrlHost.Split(new [] {','}, StringSplitOptions.RemoveEmptyEntries)
                     .Select(h => new ShellSettings(s) {RequestUrlHost = h}))
                .GroupBy(s => s.RequestUrlHost ?? string.Empty)
                .OrderByDescending(g => g.Key.Length)
                .ToDictionary(x => x.Key, x => x.AsEnumerable(), StringComparer.OrdinalIgnoreCase);

            if (unqualified.Count() == 1) {
                // only one shell had no request url criteria
                _fallback = unqualified.Single();
            }
            else if (unqualified.Any()) {
                // two or more shells had no request criteria. 
                // this is technically a misconfiguration - so fallback to the default shell
                // if it's one which will catch all requests
                _fallback = unqualified.SingleOrDefault(x => x.Name == ShellSettings.DefaultName);
            }
            else {
                // no shells are unqualified - a request that does not match a shell's spec
                // will not be mapped to routes coming from orchard
                _fallback = null;
            }

            _shellsByHostAndPrefix.Clear();
        }
        /// <summary>
        /// 根据域名+请求的虚拟目录匹配子站点设置信息
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public ShellSettings Match(HttpContextBase httpContext) {
            // use Host header to prevent proxy alteration of the orignal request
            try {
                var httpRequest = httpContext.Request;
                if (httpRequest == null) {
                    return null;
                }
                //域名
                var host = httpRequest.Headers["Host"];
                //请求地址的虚拟目录
                var appRelativeCurrentExecutionFilePath = httpRequest.AppRelativeCurrentExecutionFilePath;

                return Match(host ?? string.Empty, appRelativeCurrentExecutionFilePath);
            }
            catch(HttpException) {
                // can happen on cloud service for an unknown reason
                return null;
            }
        }

        public ShellSettings Match(string host, string appRelativePath) {
            _lock.EnterReadLock();
            try {
                if (_shellsByHost == null) {
                    return null;
                }

                // optimized path when only one tenant (Default), configured with no custom host
                if (!_shellsByHost.Any() && _fallback != null) {
                    return _fallback;
                }

                // removing the port from the host
                var hostLength = host.IndexOf(':');
                if (hostLength != -1) {
                    host = host.Substring(0, hostLength);
                }

                string hostAndPrefix = host + "/" + appRelativePath.Split('/')[1];

                return _shellsByHostAndPrefix.GetOrAdd(hostAndPrefix, key => {
                    
                    // filtering shells by host
                    IEnumerable<ShellSettings> shells;

                    if (!_shellsByHost.TryGetValue(host, out shells)) {
                        if (!_shellsByHost.TryGetValue("", out shells)) {

                            // no specific match, then look for star mapping
                            var subHostKey = _shellsByHost.Keys.FirstOrDefault(x =>
                                x.StartsWith("*.") && host.EndsWith(x.Substring(2))
                                );

                            if (subHostKey == null) {
                                return _fallback; 
                            }

                            shells = _shellsByHost[subHostKey];
                        }
                    }
                    
                    // looking for a request url prefix match
                    var mostQualifiedMatch = shells.FirstOrDefault(settings => {
                        if (settings.State == TenantState.Disabled) {
                            return false;
                        }

                        if (String.IsNullOrWhiteSpace(settings.RequestUrlPrefix)) {
                            return true;
                        }

                        return key.Equals(host + "/" + settings.RequestUrlPrefix, StringComparison.OrdinalIgnoreCase);
                    });

                    return mostQualifiedMatch ?? _fallback;
                });
                
            }
            finally {
                _lock.ExitReadLock();
            }
        }
    }
}
