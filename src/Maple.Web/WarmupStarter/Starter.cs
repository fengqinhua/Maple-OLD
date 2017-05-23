using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Maple.Web.WarmupStarter
{
    /// <summary>
    /// 预热启动实现类 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Starter<T> where T : class
    {
        private readonly Func<HttpApplication, T> _initialization;
        private readonly Action<HttpApplication, T> _beginRequest;
        private readonly Action<HttpApplication, T> _endRequest;
        private readonly object _synLock = new object();
        /// <summary>
        /// 表示需要预热加载的对象。该对象仅当初始完成且无异常时才被赋值
        /// </summary>
        private volatile T _initializationResult;
        /// <summary>
        /// 初始化的异常信息，当初始化发生异常时将被赋值。
        /// <para>该值将作为一个标识，如果不未空则表示初始化异常，下一个Web请求进入时将应用程序重新初始化</para>
        /// </summary>
        private volatile Exception _error;
        /// <summary>
        /// 上一次初始化失败的异常信息
        /// </summary>
        private volatile Exception _previousError;
        /// <summary>
        /// 预热启动实现类
        /// </summary>
        /// <param name="initialization"></param>
        /// <param name="beginRequest"></param>
        /// <param name="endRequest"></param>
        public Starter(Func<HttpApplication, T> initialization, Action<HttpApplication, T> beginRequest, Action<HttpApplication, T> endRequest)
        {
            _initialization = initialization;
            _beginRequest = beginRequest;
            _endRequest = endRequest;
        }

        public void OnApplicationStart(HttpApplication application)
        {
            LaunchStartupThread(application);
        }

        public void OnBeginRequest(HttpApplication application)
        {
            if (_error != null)
            {
                // 保存错误信息为下一次请求使用
                bool restartInitialization = false;
                lock (_synLock)
                {
                    if (_error != null)
                    {
                        _previousError = _error;
                        _error = null;
                        restartInitialization = true;
                    }
                }

                if (restartInitialization)
                {
                    LaunchStartupThread(application);
                }
            }

            // 上一次请求的异常信息
            if (_previousError != null)
            {
                throw new ApplicationException("当网络应用程序初始化时发生了未处理的异常", _previousError);
            }

            // 如果初始化已经完成，则处理请求
            if (_initializationResult != null)
            {
                _beginRequest(application, _initializationResult);
            }
        }

        public void OnEndRequest(HttpApplication application)
        {
            // 如果初始化已经完成，则处理请求
            if (_initializationResult != null)
            {
                _endRequest(application, _initializationResult);
            }
        }

        /// <summary>
        /// 执行初始化
        /// </summary>
        public void LaunchStartupThread(HttpApplication application)
        {
            // Make sure incoming requests are queued
            WarmupHttpModule.SignalWarmupStart();

            ThreadPool.QueueUserWorkItem(
                state => {
                    try
                    {
                        var result = _initialization(application);
                        _initializationResult = result;
                    }
                    catch (Exception ex)
                    {
                        lock (_synLock)
                        {
                            _error = ex;
                            _previousError = null;
                        }
                    }
                    finally
                    {
                        // Execute pending requests as the initialization is over
                        WarmupHttpModule.SignalWarmupDone();
                    }
                });
        }
    }
}
