using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple.Caching
{
    public interface IVolatileToken
    {
        /// <summary>
        /// 如果为false则代表缓存失效，如果为true则代表缓存有效
        /// </summary>
        bool IsCurrent { get; }
    }
}
