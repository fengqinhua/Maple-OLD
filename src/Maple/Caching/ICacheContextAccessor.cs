using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple.Caching
{
    /// <summary>
    /// 缓存上下文访问器
    /// </summary>
    public interface ICacheContextAccessor
    {
        /// <summary>
        /// 缓存上下文
        /// </summary>
        IAcquireContext Current { get; set; }
    }
}
