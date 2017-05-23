using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Maple.Web.WarmupStarter
{
    public class WarmupHttpModule : IHttpModule
    {
        private HttpApplication _context;
        private static object _synLock = new object();
        private static IList<Action> _awaiting = new List<Action>();
         
        public void Init(HttpApplication context)
        {
            _context = context;
            context.AddOnBeginRequestAsync(BeginBeginRequest, EndBeginRequest, null);
        }

        public void Dispose()
        {
        }

        private static bool InWarmup()
        {
            lock (_synLock)
            {
                return _awaiting != null;
            }
        }

        /// <summary>
        /// 标识正在执行网络应用程序初始化，直至调用SignalWarmupDone方才结束
        /// </summary>
        public static void SignalWarmupStart()
        {
            lock (_synLock)
            {
                if (_awaiting == null)
                {
                    _awaiting = new List<Action>();
                }
            }
        }

        /// <summary>
        /// 标识网络应用程序已初始化完成，并执行初始化过程中的缓存的请求
        /// </summary>
        public static void SignalWarmupDone()
        {
            IList<Action> temp;

            lock (_synLock)
            {
                temp = _awaiting;
                _awaiting = null;
            }

            if (temp != null)
            {
                foreach (var action in temp)
                {
                    action();
                }
            }
        }

        /// <summary>
        /// 将初始化过程中发生的Web请求入队
        /// </summary>
        private void Await(Action action)
        {
            Action temp = action;

            lock (_synLock)
            {
                if (_awaiting != null)
                {
                    temp = null;
                    _awaiting.Add(action);
                }
            }

            if (temp != null)
            {
                temp();
            }
        }

        private IAsyncResult BeginBeginRequest(object sender, EventArgs e, AsyncCallback cb, object extradata)
        {
            if (!InWarmup() || WarmupUtility.DoBeginRequest(_context))
            {
                //如果web应用程序已完成初始化 或 请求地址存在输出缓存，那么立即执行
                var asyncResult = new DoneAsyncResult(extradata);
                cb(asyncResult);
                return asyncResult;
            }
            else
            {
                //如果web应用程序正在初始化 并且 请求地址无输出缓存，那么将请求挂起等待初始化完成
                var asyncResult = new WarmupAsyncResult(cb, extradata);
                Await(asyncResult.Completed);
                return asyncResult;
            }
        }

        private static void EndBeginRequest(IAsyncResult ar)
        {
        }

        /// <summary>
        /// AsyncResult for "on hold" request (resumes when "Completed()" is called)
        /// </summary>
        private class WarmupAsyncResult : IAsyncResult
        {
            private readonly EventWaitHandle _eventWaitHandle = new AutoResetEvent(false/*initialState*/);
            private readonly AsyncCallback _cb;
            private readonly object _asyncState;
            private bool _isCompleted;

            public WarmupAsyncResult(AsyncCallback cb, object asyncState)
            {
                _cb = cb;
                _asyncState = asyncState;
                _isCompleted = false;
            }

            public void Completed()
            {
                _isCompleted = true;
                _eventWaitHandle.Set();
                _cb(this);
            }

            bool IAsyncResult.CompletedSynchronously
            {
                get { return false; }
            }

            bool IAsyncResult.IsCompleted
            {
                get { return _isCompleted; }
            }

            object IAsyncResult.AsyncState
            {
                get { return _asyncState; }
            }

            WaitHandle IAsyncResult.AsyncWaitHandle
            {
                get { return _eventWaitHandle; }
            }
        }

        /// <summary>
        /// Async result for "ok to process now" requests
        /// </summary>
        private class DoneAsyncResult : IAsyncResult
        {
            private readonly object _asyncState;
            private static readonly WaitHandle _waitHandle = new ManualResetEvent(true/*initialState*/);

            public DoneAsyncResult(object asyncState)
            {
                _asyncState = asyncState;
            }

            bool IAsyncResult.CompletedSynchronously
            {
                get { return true; }
            }

            bool IAsyncResult.IsCompleted
            {
                get { return true; }
            }

            WaitHandle IAsyncResult.AsyncWaitHandle
            {
                get { return _waitHandle; }
            }

            object IAsyncResult.AsyncState
            {
                get { return _asyncState; }
            }
        }
    }
}
