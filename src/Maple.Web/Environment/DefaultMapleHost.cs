using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Maple.Caching;
using Maple.Web.Environment.Configuration;
using Maple.Web.Environment.Extensions;
using Maple.Web.Environment.ShellBuilders;
using Maple.Web.Environment.State;
using Maple.Web.Environment.Descriptor;
using Maple.Web.Environment.Descriptor.Models;
using Maple.Localization;
using Maple.Logging;
using Maple.Web.Mvc;
using Maple.Web.Mvc.Extensions;
using Maple.Web.Utility.Extensions;
using Maple.Web.Utility;
using System.Threading;


namespace Maple.Web.Environment
{
    // All the event handlers that DefaultOrchardHost implements have to be declared in OrchardStarter.
    public class DefaultMapleHost : IMapleHost, IShellSettingsManagerEventHandler, IShellDescriptorManagerEventHandler
    {
        private readonly IHostLocalRestart _hostLocalRestart;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IShellContextFactory _shellContextFactory;
        private readonly IRunningShellTable _runningShellTable;
        private readonly IProcessingEngine _processingEngine;
        private readonly IExtensionLoaderCoordinator _extensionLoaderCoordinator;
        private readonly IExtensionMonitoringCoordinator _extensionMonitoringCoordinator;
        private readonly ICacheManager _cacheManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly static object _syncLock = new object();
        private readonly static object _shellContextsWriteLock = new object();
        private readonly NamedReaderWriterLock _shellActivationLock = new NamedReaderWriterLock();

        private IEnumerable<ShellContext> _shellContexts;
        private readonly ContextState<IList<ShellSettings>> _tenantsToRestart;

        public int Retries { get; set; }
        public bool DelayRetries { get; set; }

        public DefaultMapleHost(
            IShellSettingsManager shellSettingsManager,
            IShellContextFactory shellContextFactory,
            IRunningShellTable runningShellTable,
            IProcessingEngine processingEngine,
            IExtensionLoaderCoordinator extensionLoaderCoordinator,
            IExtensionMonitoringCoordinator extensionMonitoringCoordinator,
            ICacheManager cacheManager,
            IHostLocalRestart hostLocalRestart,
            IHttpContextAccessor httpContextAccessor)
        {

            _shellSettingsManager = shellSettingsManager;
            _shellContextFactory = shellContextFactory;
            _runningShellTable = runningShellTable;
            _processingEngine = processingEngine;
            _extensionLoaderCoordinator = extensionLoaderCoordinator;
            _extensionMonitoringCoordinator = extensionMonitoringCoordinator;
            _cacheManager = cacheManager;
            _hostLocalRestart = hostLocalRestart;
            _httpContextAccessor = httpContextAccessor;

            _tenantsToRestart = new ContextState<IList<ShellSettings>>("DefaultOrchardHost.TenantsToRestart", () => new List<ShellSettings>());

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public IList<ShellContext> Current
        {
            get { return BuildCurrent().ToReadOnlyCollection(); }
        }

        public ShellContext GetShellContext(ShellSettings shellSettings)
        {
            return BuildCurrent().SingleOrDefault(shellContext => shellContext.Settings.Name.Equals(shellSettings.Name));
        }

        void IMapleHost.Initialize()
        {
            Logger.Information("开始Maple应用程序初始化");
            BuildCurrent();
            Logger.Information("Maple应用程序初始化完成");
        }

        void IMapleHost.ReloadExtensions()
        {
            DisposeShellContext();
        }

        void IMapleHost.BeginRequest()
        {
            Logger.Debug("BeginRequest");
            BeginRequest();
        }

        void IMapleHost.EndRequest()
        {
            Logger.Debug("EndRequest");
            EndRequest();
        }
        /// <summary>
        /// 为租户创建独立环境
        /// </summary>
        /// <param name="shellSettings"></param>
        /// <returns></returns>
        IWorkContextScope IMapleHost.CreateStandaloneEnvironment(ShellSettings shellSettings)
        {
            Logger.Debug("为租户创建独立环境 {0}", shellSettings.Name);
            //监控扩展模块是否发生变化以及Host是否需要重启
            MonitorExtensions();
            BuildCurrent();

            var shellContext = CreateShellContext(shellSettings);
            var workContext = shellContext.LifetimeScope.CreateWorkContextScope();
            return new StandaloneEnvironmentWorkContextScopeWrapper(workContext, shellContext);
        }

        /// <summary>
        /// 确保shell被激活，或如果扩展已经改变，重新激活shell
        /// </summary>
        IEnumerable<ShellContext> BuildCurrent()
        {
            if (_shellContexts == null)
            {
                lock (_syncLock)
                {
                    if (_shellContexts == null)
                    {
                        //加载应用程序扩展信息
                        SetupExtensions();
                        //监控扩展模块是否发生变化以及Host是否需要重启
                        MonitorExtensions();
                        //创建并激活Shell
                        CreateAndActivateShells();
                    }
                }
            }

            return _shellContexts;
        }
        /// <summary>
        /// 如果shell配置发生变化，而且无需重启整个host，则重新激活shell
        /// </summary>
        void StartUpdatedShells()
        {
            while (_tenantsToRestart.GetState().Any())
            {
                var settings = _tenantsToRestart.GetState().First();
                _tenantsToRestart.GetState().Remove(settings);
                Logger.Debug("更新Shell: " + settings.Name);
                lock (_syncLock)
                {
                    ActivateShell(settings);
                }
            }
        }

        /// <summary>
        /// 创建并激活租户
        /// </summary>
        void CreateAndActivateShells()
        {
            Logger.Information("开始创建Shell");

            // 从App_Data/site文件夹中读取Shell子站点信息
            var allSettings = _shellSettingsManager.LoadSettings()
                .Where(settings => settings.State == TenantState.Running || settings.State == TenantState.Uninitialized || settings.State == TenantState.Initializing)
                .ToArray();

            // Load all tenants, and activate their shell.
            if (allSettings.Any())
            {
                Parallel.ForEach(allSettings, settings =>
                {
                    for (var i = 0; i <= Retries; i++)
                    {

                        // Not the first attempt, wait for a while ...
                        if (DelayRetries && i > 0)
                        {

                            // Wait for i^2 which means 1, 2, 4, 8 ... seconds
                            Thread.Sleep(TimeSpan.FromSeconds(Math.Pow(i, 2)));
                        }

                        try
                        {
                            var context = CreateShellContext(settings);
                            ActivateShell(context);

                            // If everything went well, return to stop the retry loop
                            break;
                        }
                        catch (Exception ex)
                        {
                            if (i == Retries)
                            {
                                Logger.Fatal("A tenant could not be started: {0} after {1} retries.", settings.Name, Retries);
                                return;
                            }
                            else
                            {
                                Logger.Error(ex, "A tenant could not be started: " + settings.Name + " Attempt number: " + i);
                            }
                        }

                    }

                    while (_processingEngine.AreTasksPending())
                    {
                        Logger.Debug("Processing pending task after activate Shell");
                        _processingEngine.ExecuteNextTask();
                    }
                });
            }
            else
            {
                //如果未发现站点信息,那么创建缺省的Defalut子站点信息并激活之
                var setupContext = CreateSetupContext();
                ActivateShell(setupContext);
            }

            Logger.Information("Shell创建并激活完成.");
        }

        /// <summary>
        /// 注册、启用一个租户并将其加入 RunningShellTable
        /// </summary>
        private void ActivateShell(ShellContext context)
        {
            Logger.Debug("启用租户 -- {0}", context.Settings.Name);
            context.Shell.Activate();

            lock (_shellContextsWriteLock)
            {
                _shellContexts = (_shellContexts ?? Enumerable.Empty<ShellContext>())
                                .Where(c => c.Settings.Name != context.Settings.Name)
                                .Concat(new[] { context })
                                .ToArray();
            }

            _runningShellTable.Add(context.Settings);
        }

        /// <summary>
        /// 创建缺省得子站点上下文
        /// </summary>
        private ShellContext CreateSetupContext()
        {
            Logger.Debug("创建缺省得子站点上下文.");
            return _shellContextFactory.CreateSetupContext(new ShellSettings { Name = ShellSettings.DefaultName });
        }
        /// <summary>
        /// 基于Shell设置创建子上下文
        /// </summary>
        private ShellContext CreateShellContext(ShellSettings settings)
        {
            if (settings.State == TenantState.Uninitialized || settings.State == TenantState.Invalid)
            {
                Logger.Debug("创建站点 {0} 上下文 .", settings.Name);
                return _shellContextFactory.CreateSetupContext(settings);
            }

            Logger.Debug("创建站点 {0} 上下文 .", settings.Name);
            return _shellContextFactory.CreateShellContext(settings);
        }
        /// <summary>
        /// 通过ExtensionLoaderCoordinator扩展协调器，加载扩展
        /// </summary>
        private void SetupExtensions()
        {
            _extensionLoaderCoordinator.SetupExtensions();
        }

        /// <summary>
        /// 监控应用程序及模块是否发生变化
        /// </summary>
        private void MonitorExtensions()
        {
            // This is a "fake" cache entry to allow the extension loader coordinator
            // notify us (by resetting _current to "null") when an extension has changed
            // on disk, and we need to reload new/updated extensions.
            _cacheManager.Get("OrchardHost_Extensions", true,
                              ctx =>
                              {
                                  //监控模块/皮肤的变化
                                  _extensionMonitoringCoordinator.MonitorExtensions(ctx.Monitor);
                                  //监控重启标识文件 app/App_data/hrestart.txt
                                  _hostLocalRestart.Monitor(ctx.Monitor);
                                  //终止所有shell上下文
                                  DisposeShellContext();

                                  return "";
                              });
        }
        /// <summary>
        /// 终止所有活动的shell上下文，并处理它们的作用域，以便重新启动它们
        /// </summary>
        private void DisposeShellContext()
        {
            Logger.Information("终止所有活动的租户");

            if (_shellContexts != null)
            {
                lock (_syncLock)
                {
                    if (_shellContexts != null)
                    {
                        foreach (var shellContext in _shellContexts)
                        {
                            shellContext.Shell.Terminate();
                            shellContext.Dispose();
                        }
                    }
                }
                _shellContexts = null;
            }
        }

        protected virtual void BeginRequest()
        {
            //当站点未完成初始化时，阻止用户访问
            BlockRequestsDuringSetup();

            //委托事件： 确保已加载shell上下文，或者如果扩展模块发生变化重新加载shell上下文
            Action ensureInitialized = () =>
            {
                //监控扩展模块是否发生变化以及Host是否需要重启
                MonitorExtensions();
                //确保shell被激活，或如果扩展已经改变，重新激活shell
                BuildCurrent();
            };

            //1、根据域名+请求的虚拟目录匹配子站点设置信息
            ShellSettings currentShellSettings = null;
            var httpContext = _httpContextAccessor.Current();
            if (httpContext != null)
            {
                currentShellSettings = _runningShellTable.Match(httpContext);
            }

            if (currentShellSettings == null)
            {
                //2、如果无法匹配子站点设置信息，那么重新加载shell上下文
                ensureInitialized();
            }
            else
            {
                _shellActivationLock.RunWithReadLock(currentShellSettings.Name, () =>
                {
                    ensureInitialized();
                });
            }

            // StartUpdatedShells can cause a writer shell activation lock so it should run outside the reader lock.
            // 如果shell配置发生变化，而且无需重启整个host，则重新激活shell
            StartUpdatedShells();
        }

        protected virtual void EndRequest()
        {
            // Synchronously process all pending tasks. It's safe to do this at this point
            // of the pipeline, as the request transaction has been closed, so creating a new
            // environment and transaction for these tasks will behave as expected.)
            while (_processingEngine.AreTasksPending())
            {
                Logger.Debug("Processing pending task");
                _processingEngine.ExecuteNextTask();
            }

            // 如果shell配置发生变化，而且无需重启整个host，则重新激活shell
            StartUpdatedShells();
        }

        void IShellSettingsManagerEventHandler.Saved(ShellSettings settings)
        {
            Logger.Debug("Shell saved: " + settings.Name);

            // if a tenant has been created
            if (settings.State != TenantState.Invalid)
            {
                if (!_tenantsToRestart.GetState().Any(t => t.Name.Equals(settings.Name)))
                {
                    Logger.Debug("设置子站点重启: " + settings.Name + " " + settings.State);
                    _tenantsToRestart.GetState().Add(settings);
                }
            }
        }
        /// <summary>
        /// 启用一个租户Shell
        /// </summary>
        /// <param name="settings"></param>
        public void ActivateShell(ShellSettings settings)
        {
            Logger.Debug("启用一个Shell: " + settings.Name);

            // 如果列表中不存在当前Shell则返回
            var shellContext = _shellContexts.FirstOrDefault(c => c.Settings.Name == settings.Name);
            if (shellContext == null && settings.State == TenantState.Disabled)
            {
                return;
            }

            // is this is a new tenant ? or is it a tenant waiting for setup ?
            if (shellContext == null || settings.State == TenantState.Uninitialized)
            {
                // create the Shell
                var context = CreateShellContext(settings);

                // activate the Shell
                ActivateShell(context);
            }
            // terminate the shell if the tenant was disabled
            else if (settings.State == TenantState.Disabled)
            {
                shellContext.Shell.Terminate();
                _runningShellTable.Remove(settings);

                // Forcing enumeration with ToArray() so a lazy execution isn't causing issues by accessing the disposed context.
                _shellContexts = _shellContexts.Where(shell => shell.Settings.Name != settings.Name).ToArray();

                shellContext.Dispose();
            }
            // reload the shell as its settings have changed
            else
            {
                _shellActivationLock.RunWithWriteLock(settings.Name, () =>
                {
                    // dispose previous context
                    shellContext.Shell.Terminate();

                    var context = _shellContextFactory.CreateShellContext(settings);

                    // Activate and register modified context.
                    // Forcing enumeration with ToArray() so a lazy execution isn't causing issues by accessing the disposed shell context.
                    _shellContexts = _shellContexts.Where(shell => shell.Settings.Name != settings.Name).Union(new[] { context }).ToArray();

                    shellContext.Dispose();
                    context.Shell.Activate();

                    _runningShellTable.Update(settings);
                });
            }
        }

        /// <summary>
        /// 租户信息发生变化时（一个功能已启用/禁用），租户需要重新启动
        /// </summary>
        void IShellDescriptorManagerEventHandler.Changed(ShellDescriptor descriptor, string tenant)
        {
            if (_shellContexts == null)
            {
                return;
            }

            Logger.Debug("Shell changed: " + tenant);

            var context = _shellContexts.FirstOrDefault(x => x.Settings.Name == tenant);

            if (context == null)
            {
                return;
            }

            // don't restart when tenant is in setup
            if (context.Settings.State != TenantState.Running)
            {
                return;
            }

            // don't flag the tenant if already listed
            if (_tenantsToRestart.GetState().Any(x => x.Name == tenant))
            {
                return;
            }

            Logger.Debug("设置子站点重启: " + tenant);
            _tenantsToRestart.GetState().Add(context.Settings);
        }

        /// <summary>
        /// 当站点未完成初始化时，阻止用户访问
        /// </summary>
        private void BlockRequestsDuringSetup()
        {
            var httpContext = _httpContextAccessor.Current();
            if (httpContext.IsBackgroundContext())
                return;

            // Get the requested shell.
            var runningShell = _runningShellTable.Match(httpContext);
            if (runningShell == null)
                return;

            // If the requested shell is currently initializing, return a Service Unavailable HTTP status code.
            if (runningShell.State == TenantState.Initializing)
            {
                var response = httpContext.Response;
                response.StatusCode = 503;
                response.StatusDescription = "此站点当前正在初始化。请稍后再试.";
                response.Write("此站点当前正在初始化。请稍后再试.");
            }
        }

        // To be used from CreateStandaloneEnvironment(), also disposes the ShellContext LifetimeScope.
        private class StandaloneEnvironmentWorkContextScopeWrapper : IWorkContextScope
        {
            private readonly ShellContext _shellContext;
            private readonly IWorkContextScope _workContextScope;

            public WorkContext WorkContext
            {
                get { return _workContextScope.WorkContext; }
            }

            public StandaloneEnvironmentWorkContextScopeWrapper(IWorkContextScope workContextScope, ShellContext shellContext)
            {
                _workContextScope = workContextScope;
                _shellContext = shellContext;
            }

            public TService Resolve<TService>()
            {
                return _workContextScope.Resolve<TService>();
            }

            public bool TryResolve<TService>(out TService service)
            {
                return _workContextScope.TryResolve<TService>(out service);
            }

            public void Dispose()
            {
                _workContextScope.Dispose();
                _shellContext.Dispose();
            }
        }
    }
}
