using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple.Caching
{
    /// <summary>
    /// 缓存上下文
    /// </summary>
    public interface IAcquireContext
    {
        /// <summary>
        /// Monitor委托用于获取缓存失效方式
        /// </summary>
        Action<IVolatileToken> Monitor { get; }
    }

    /// <summary>
    /// 缓存上下文
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class AcquireContext<TKey> : IAcquireContext
    {
        /// <summary>
        /// 缓存上下文
        /// </summary>
        /// <param name="key"></param>
        /// <param name="monitor"></param>
        public AcquireContext(TKey key, Action<IVolatileToken> monitor)
        {
            Key = key;
            Monitor = monitor;
        }
        /// <summary>
        /// 缓存对应的Key
        /// </summary>
        public TKey Key { get; private set; }

        /// <summary>
        /// 委托事件：为缓存条目添加缓存规则
        /// </summary>
        public Action<IVolatileToken> Monitor { get; private set; }
    }

    /// <summary>
    /// 最基础的缓存上下文
    /// </summary>
    public class SimpleAcquireContext : IAcquireContext
    {
        private readonly Action<IVolatileToken> _monitor;

        public SimpleAcquireContext(Action<IVolatileToken> monitor)
        {
            _monitor = monitor;
        }
        /// <summary>
        /// Monitor委托用于获取缓存失效方式
        /// </summary>
        public Action<IVolatileToken> Monitor
        {
            get { return _monitor; }
        }
    }

}
