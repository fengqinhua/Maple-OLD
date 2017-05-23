using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple.Logging
{
    public sealed class DefaultLogger : ILogger
    {
        public DefaultLogger()
        {

        }

        #region ILogger

        public bool IsEnabled(LogLevel level)
        {
            return LoggerManagers.Instance.Logger.IsEnabled(level);
        }
        public void Log(LogLevel level, Exception exception, string format, params object[] args)
        {
            LoggerManagers.Instance.Logger.Log(level, exception, format, args);
        }

        #endregion
    }
}
