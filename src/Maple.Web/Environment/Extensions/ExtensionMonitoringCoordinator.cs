using System;
using System.Collections.Generic;
using System.Linq;
using Maple.Caching;
using Maple.Web.Environment.Extensions.Loaders;
using Maple.Web.Environment.Extensions.Models;
using Maple.Web.FileSystems.VirtualPath;
using Maple.Logging;

namespace Maple.Web.Environment.Extensions {
    /// <summary>
    /// ��չ���Э����
    /// </summary>
    public class ExtensionMonitoringCoordinator : IExtensionMonitoringCoordinator {
        private readonly IVirtualPathMonitor _virtualPathMonitor;
        private readonly IAsyncTokenProvider _asyncTokenProvider;
        private readonly IExtensionManager _extensionManager;
        private readonly IEnumerable<IExtensionLoader> _loaders;

        public ExtensionMonitoringCoordinator(
            IVirtualPathMonitor virtualPathMonitor,
            IAsyncTokenProvider asyncTokenProvider,
            IExtensionManager extensionManager,
            IEnumerable<IExtensionLoader> loaders) {

            _virtualPathMonitor = virtualPathMonitor;
            _asyncTokenProvider = asyncTokenProvider;
            _extensionManager = extensionManager;
            _loaders = loaders;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public bool Disabled { get; set; }

        /// <summary>
        /// ���ģ��/Ƥ���ı仯
        /// </summary>
        /// <param name="monitor"></param>
        public void MonitorExtensions(Action<IVolatileToken> monitor) {
            // ��������ԭ�����ǿ��ܻ���øù���
            if (Disabled)
                return;

            //PERF: �첽�����չ��Ϣ.
            monitor(_asyncTokenProvider.GetToken(MonitorExtensionsWork));
        }

        public void MonitorExtensionsWork(Action<IVolatileToken> monitor) {
            var locations = _extensionManager.AvailableExtensions().Select(e => e.Location).Distinct(StringComparer.InvariantCultureIgnoreCase);

            Logger.Information("Start monitoring extension files...");
            // ����Ƿ��������Ƴ�ģ��/Ƥ��
            foreach (string location in locations) {
                Logger.Debug("Monitoring virtual path \"{0}\"", location);
                monitor(_virtualPathMonitor.WhenPathChanges(location));
            }

            // ��װ�ػ�һ������������κζ���ı仯
            var extensions = _extensionManager.AvailableExtensions().Where(d => DefaultExtensionTypes.IsModule(d.ExtensionType) || DefaultExtensionTypes.IsTheme(d.ExtensionType)).ToList();
            foreach (var extension in extensions) {
                foreach (var loader in _loaders) {
                    loader.Monitor(extension, monitor);
                }
            }
            Logger.Information("Done monitoring extension files...");
        }
    }
}