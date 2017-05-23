using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple.Caching
{
    /// <summary>
    /// 缓存的持有者
    /// <para>用于管理ICache缓存项的接口 （生命周期：租户单例，主要是用来维护一个ICache接口集合的）</para>
    /// </summary>
    public interface ICacheHolder : ISingletonDependency
    {
        /// <summary>
        /// 按照类型获取ICache缓存
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="component"></param>
        /// <returns></returns>
        ICache<TKey, TResult> GetCache<TKey, TResult>(Type component);
    }
}
