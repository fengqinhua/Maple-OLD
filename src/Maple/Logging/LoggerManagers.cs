using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple.Logging
{
    /// <summary>
    /// 日志管理器
    /// </summary>
    internal class LoggerManagers
    {
        private ILogger _logger = null;
        private LoggerManagers()
        {
            _logger = new NullLogger();
        }

        #region 单例模式
        public static LoggerManagers Instance
        {
            get
            {
                return Nested.instance;
            }
        }
         
        class Nested
        {
            static Nested()
            {
            }
            internal static readonly LoggerManagers instance = new LoggerManagers();
        }

        #endregion


        internal void UseLogger(ILogger logger)
        {
            _logger = logger;
        }

        internal ILogger Logger
        {
            get
            {
                return _logger;
            }
        }
    }
}
