using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple
{
    public class DataConst
    {
        /// <summary>
        /// 无效时间(缺省时间)
        /// </summary>
        public static readonly DateTime INVALID_DATETIME = new DateTime(1900, 1, 1);
        /// <summary>
        /// 无效整数(缺省整数)
        /// </summary>
        public static readonly int INVALID_INT = -99999;
        /// <summary>
        /// 无效浮点数(缺省浮点数)
        /// </summary>
        public static readonly double INVALID_DOUBLE = -99999;

        /// <summary>
        /// 当前系统时间
        /// </summary>
        public static DateTime SystemTime
        {
            get
            {
                return DateTime.Now;
            }
        }
    }
}
