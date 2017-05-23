using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple.Caching
{
    /// <summary>
    /// ICacheHolder的缺省实现
    /// <para>用于管理ICache缓存项的接口 （生命周期：租户单例，主要是用来维护一个ICache接口集合的）</para>
    /// <para>使用线程安全的ConcurrentDictionary<CacheKey, object>型字典来保存缓存，这里称为类型缓存字典，注意该字典中并不存储实际的缓存值</para>
    /// </summary>
    public class DefaultCacheHolder : ICacheHolder
    {
        private readonly ICacheContextAccessor _cacheContextAccessor;
        private readonly ConcurrentDictionary<CacheKey, object> _caches = new ConcurrentDictionary<CacheKey, object>();

        public DefaultCacheHolder(ICacheContextAccessor cacheContextAccessor)
        {
            _cacheContextAccessor = cacheContextAccessor;
        }

        /// <summary>
        /// 在ICacheHolder中维护了一个ConcurrentDictionary<CacheKey, object>字典表，
        /// CacheKey为一个三元组，类型全是Type，分别为：使用缓存的组件类型，缓存Key类型，缓存结果类型。
        /// </summary>
        class CacheKey : Tuple<Type, Type, Type>
        {
            public CacheKey(Type component, Type key, Type result)
                : base(component, key, result)
            {
            }
        }

        /// <summary>
        /// Gets a Cache entry from the cache. If none is found, an empty one is created and returned.
        /// </summary>
        /// <typeparam name="TKey">The type of the key within the component.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="component">The component context.</param>
        /// <returns>An entry from the cache, or a new, empty one, if none is found.</returns>
        public ICache<TKey, TResult> GetCache<TKey, TResult>(Type component)
        {
            var cacheKey = new CacheKey(component, typeof(TKey), typeof(TResult));
            var result = _caches.GetOrAdd(cacheKey, k => new Cache<TKey, TResult>(_cacheContextAccessor));
            return (Cache<TKey, TResult>)result;
        }
    }
}
