using System;
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
    public interface ICache<TKey, TResult>
    {
        /// <summary>
        /// 获取缓存内容
        /// </summary>
        /// <param name="key"></param>
        /// <param name="acquire"></param>
        /// <returns></returns>
        TResult Get(TKey key, Func<AcquireContext<TKey>, TResult> acquire);
    }
}
