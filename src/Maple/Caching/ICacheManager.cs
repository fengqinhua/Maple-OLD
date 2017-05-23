using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple.Caching
{
    /// <summary>
    /// 缓存管理器
    /// </summary>
    public interface ICacheManager
    {
        /// <summary>
        /// 从缓存持有者手中获取缓存值
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="key"></param>
        /// <param name="acquire"></param>
        /// <returns></returns>
        TResult Get<TKey, TResult>(TKey key, Func<AcquireContext<TKey>, TResult> acquire);
        /// <summary>
        /// 从缓存持有者手中获取缓存最终的执行接口（存储缓存数据的地方）
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        ICache<TKey, TResult> GetCache<TKey, TResult>();
    }

    public static class CacheManagerExtensions
    {
        /// <summary>
        /// 从缓存持有者手中获取缓存值（防止并发发生）
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="cacheManager"></param>
        /// <param name="key"></param>
        /// <param name="preventConcurrentCalls"></param>
        /// <param name="acquire"></param>
        /// <returns></returns>
        public static TResult Get<TKey, TResult>(this ICacheManager cacheManager, TKey key, bool preventConcurrentCalls, Func<AcquireContext<TKey>, TResult> acquire)
        {
            if (preventConcurrentCalls)
            {
                lock (key)
                {
                    return cacheManager.Get(key, acquire);
                }
            }
            else
            {
                return cacheManager.Get(key, acquire);
            }
        }
    }
}
