using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple.Caching
{
    /// <summary>
    /// 缓存最终的执行接口（存储缓存数据的地方）
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class Cache<TKey, TResult> : ICache<TKey, TResult>
    {
        private readonly ICacheContextAccessor _cacheContextAccessor;
        private readonly ConcurrentDictionary<TKey, CacheEntry> _entries;

        public Cache(ICacheContextAccessor cacheContextAccessor)
        {
            _cacheContextAccessor = cacheContextAccessor;
            _entries = new ConcurrentDictionary<TKey, CacheEntry>();
        }

        /// <summary>
        /// 获取缓存内容，如果缓存不存在则使用委托函数添加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="acquire"></param>
        /// <returns></returns>
        public TResult Get(TKey key, Func<AcquireContext<TKey>, TResult> acquire)
        {
            var entry = _entries.AddOrUpdate(key,
                // "Add" lambda
                k => AddEntry(k, acquire),
                // "Update" lambda
                (k, currentEntry) => UpdateEntry(currentEntry, k, acquire));

            return entry.Result;
        }

        /// <summary>
        /// 新增缓存条目
        /// </summary>
        /// <param name="k"></param>
        /// <param name="acquire"></param>
        /// <returns></returns>
        private CacheEntry AddEntry(TKey k, Func<AcquireContext<TKey>, TResult> acquire)
        {
            //创建一个缓存条目
            var entry = CreateEntry(k, acquire);
            PropagateTokens(entry);
            return entry;
        }
        /// <summary>
        /// 更新缓存条目
        /// </summary>
        /// <param name="currentEntry"></param>
        /// <param name="k"></param>
        /// <param name="acquire"></param>
        /// <returns></returns>
        private CacheEntry UpdateEntry(CacheEntry currentEntry, TKey k, Func<AcquireContext<TKey>, TResult> acquire)
        { 
            //遍历缓存条码中所有的令牌，如果其中一个令牌标识为缓存失效则重新创建缓存条目，否则返回当前缓存条目
            var entry = (currentEntry.Tokens.Any(t => t != null && !t.IsCurrent)) ? CreateEntry(k, acquire) : currentEntry;
            PropagateTokens(entry);
            return entry;
        }

        /// <summary>
        /// 探测缓存是否失效
        /// </summary>
        /// <param name="entry"></param>
        private void PropagateTokens(CacheEntry entry)
        {
            // Bubble up volatile tokens to parent context
            if (_cacheContextAccessor.Current != null)
            {
                foreach (var token in entry.Tokens)
                    _cacheContextAccessor.Current.Monitor(token);
            }
        }

        /// <summary>
        /// 创建一个缓存条目
        /// </summary>
        /// <param name="k"></param>
        /// <param name="acquire"></param>
        /// <returns></returns>
        private CacheEntry CreateEntry(TKey k, Func<AcquireContext<TKey>, TResult> acquire)
        {            
            //创建一个空的缓存条目
            var entry = new CacheEntry();
            //创建一个新的缓存上下文
            var context = new AcquireContext<TKey>(k, entry.AddToken);

            //声明一个变量用于存储之前已经存在的缓存上下文
            IAcquireContext parentContext = null;
            try
            {
                //保存之前的缓存上下文
                parentContext = _cacheContextAccessor.Current;
                //设置新的缓存上下文
                _cacheContextAccessor.Current = context;
                //得到缓存的结果
                entry.Result = acquire(context);
            }
            finally
            {
                // 不论缓存条目是否添加成功都还原之前的缓存上下文
                _cacheContextAccessor.Current = parentContext;
            }

            //压缩令牌，删除重复的令牌实现
            entry.CompactTokens();
            return entry;
        }

        /// <summary>
        /// 缓存条目
        /// </summary>
        private class CacheEntry
        {
            /// <summary>
            /// 缓存失效规则集合
            /// </summary>
            private IList<IVolatileToken> _tokens;
            /// <summary>
            /// 缓存的内容
            /// </summary>
            public TResult Result { get; set; }
            /// <summary>
            /// 缓存失效规则集合
            /// </summary>
            public IEnumerable<IVolatileToken> Tokens
            {
                get
                {
                    return _tokens ?? Enumerable.Empty<IVolatileToken>();
                }
            }
            /// <summary>
            /// 添加缓存失效规则
            /// </summary>
            /// <param name="volatileToken"></param>
            public void AddToken(IVolatileToken volatileToken)
            {
                if (_tokens == null)
                {
                    _tokens = new List<IVolatileToken>();
                }

                _tokens.Add(volatileToken);
            }
            /// <summary>
            /// 去除重复的缓存失效规则
            /// </summary>
            public void CompactTokens()
            {
                if (_tokens != null)
                    _tokens = _tokens.Distinct().ToArray();
            }
        }
    }
}
