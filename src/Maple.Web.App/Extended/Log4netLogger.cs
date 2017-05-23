using log4net;
using log4net.Util;
using Maple.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Maple.Web.App.Extended
{
    public class Log4netLogger : Maple.Logging.ILogger
    {
        private ILog _log = null;

        public Log4netLogger()
        {
            _log = LogManager.GetLogger("LogForFile");
        }

        #region ILogger

        public bool IsEnabled(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return _log.IsDebugEnabled;
                case LogLevel.Information:
                    return _log.IsInfoEnabled;
                case LogLevel.Warning:
                    return _log.IsWarnEnabled;
                case LogLevel.Error:
                    return _log.IsErrorEnabled;
                case LogLevel.Fatal:
                    return _log.IsFatalEnabled;
            }
            return false;
        }
        public void Log(LogLevel level, Exception exception, string format, params object[] args)
        {
            if (args == null)
            {
                switch (level)
                {
                    case LogLevel.Debug:
                        _log.Debug(format, exception);
                        break;
                    case LogLevel.Information:
                        _log.Info(format, exception);
                        break;
                    case LogLevel.Warning:
                        _log.Warn(format, exception);
                        break;
                    case LogLevel.Error:
                        _log.Error(format, exception);
                        break;
                    case LogLevel.Fatal:
                        _log.Fatal(format, exception);
                        break;
                }
            }
            else
            {
                switch (level)
                {
                    case LogLevel.Debug:
                        _log.Debug(new SystemStringFormat(CultureInfo.InvariantCulture, format, args), exception);
                        break;
                    case LogLevel.Information:
                        _log.Info(new SystemStringFormat(CultureInfo.InvariantCulture, format, args), exception);
                        break;
                    case LogLevel.Warning:
                        _log.Warn(new SystemStringFormat(CultureInfo.InvariantCulture, format, args), exception);
                        break;
                    case LogLevel.Error:
                        _log.Error(new SystemStringFormat(CultureInfo.InvariantCulture, format, args), exception);
                        break;
                    case LogLevel.Fatal:
                        _log.Fatal(new SystemStringFormat(CultureInfo.InvariantCulture, format, args), exception);
                        break;
                }
            }
        }

        #endregion
    }
}