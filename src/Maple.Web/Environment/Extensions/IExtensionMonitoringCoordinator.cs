using System;
using Maple.Caching;

namespace Maple.Web.Environment.Extensions {
    public interface IExtensionMonitoringCoordinator {
        void MonitorExtensions(Action<IVolatileToken> monitor);
    }
}