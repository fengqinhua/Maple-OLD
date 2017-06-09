using System;
using Maple.Caching;

namespace Maple.Web.Environment.Extensions {
    /// <summary>
    /// 扩展监测协调者
    /// </summary>
    public interface IExtensionMonitoringCoordinator {
        /// <summary>
        /// 监控模块/皮肤的变化
        /// </summary>
        /// <param name="monitor"></param>
        void MonitorExtensions(Action<IVolatileToken> monitor);
    }
}