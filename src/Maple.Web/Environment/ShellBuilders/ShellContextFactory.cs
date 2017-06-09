using System.Linq;
using Autofac;
using Maple.Web.Environment.Configuration;
using Maple.Web.Environment.Descriptor;
using Maple.Web.Environment.Descriptor.Models;
using Maple.Logging;

namespace Maple.Web.Environment.ShellBuilders {
    /// <summary>
    /// High-level coordinator that exercises other component capabilities to
    /// build all of the artifacts for a running shell given a tenant settings.
    /// </summary>
    public interface IShellContextFactory {
        /// <summary>
        /// Builds a shell context given a specific tenant settings structure
        /// </summary>
        ShellContext CreateShellContext(ShellSettings settings);

        /// <summary>
        /// ����һ��ȱʡ��Shell������ʵ��. 
        /// Needed to display setup user interface.
        /// </summary>
        ShellContext CreateSetupContext(ShellSettings settings);

        /// <summary>
        /// Builds a shell context given a specific description of features and parameters.
        /// Shell's actual current descriptor has no effect. Does not use or update descriptor cache.
        /// </summary>
        ShellContext CreateDescribedContext(ShellSettings settings, ShellDescriptor shellDescriptor);

    }

    public class ShellContextFactory : IShellContextFactory {
        private readonly IShellDescriptorCache _shellDescriptorCache;
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly IShellContainerFactory _shellContainerFactory;

        public ShellContextFactory(
            IShellDescriptorCache shellDescriptorCache,
            ICompositionStrategy compositionStrategy,
            IShellContainerFactory shellContainerFactory) {
            _shellDescriptorCache = shellDescriptorCache;
            _compositionStrategy = compositionStrategy;
            _shellContainerFactory = shellContainerFactory;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public ShellContext CreateShellContext(ShellSettings settings) {           

            Logger.Debug("Creating shell context for tenant {0}", settings.Name);

            var knownDescriptor = _shellDescriptorCache.Fetch(settings.Name);
            if (knownDescriptor == null) {
                Logger.Information("No descriptor cached. Starting with minimum components.");
                knownDescriptor = MinimumShellDescriptor();
            }

            var blueprint = _compositionStrategy.Compose(settings, knownDescriptor);
            var shellScope = _shellContainerFactory.CreateContainer(settings, blueprint);

            ShellDescriptor currentDescriptor;
            using (var standaloneEnvironment = shellScope.CreateWorkContextScope()) {
                var shellDescriptorManager = standaloneEnvironment.Resolve<IShellDescriptorManager>();
                currentDescriptor = shellDescriptorManager.GetShellDescriptor();
            }

            if (currentDescriptor != null && knownDescriptor.SerialNumber != currentDescriptor.SerialNumber) {
                Logger.Information("Newer descriptor obtained. Rebuilding shell container.");

                _shellDescriptorCache.Store(settings.Name, currentDescriptor);
                blueprint = _compositionStrategy.Compose(settings, currentDescriptor);
                shellScope.Dispose();
                shellScope = _shellContainerFactory.CreateContainer(settings, blueprint);
            }

            return new ShellContext {
                Settings = settings,
                Descriptor = currentDescriptor,
                Blueprint = blueprint,
                LifetimeScope = shellScope,
                Shell = shellScope.Resolve<IMapleShell>(),
            };
        }

        private static ShellDescriptor MinimumShellDescriptor() {
            return new ShellDescriptor {
                SerialNumber = -1,
                Features = new[] {
                    new ShellFeature {Name = "Orchard.Framework"},
                    new ShellFeature {Name = "Settings"},
                },
                Parameters = Enumerable.Empty<ShellParameter>(),
            };
        }

        /// <summary>
        ///  ����һ��ȱʡ��Shell������ʵ��.  Needed to display setup user interface.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public ShellContext CreateSetupContext(ShellSettings settings) {
            Logger.Debug("δ��ȡShell������Ϣ������һ��ȱʡ��Shell��������Ϣ");

            var descriptor = new ShellDescriptor {
                SerialNumber = -1,
                Features = new[] {
                    new ShellFeature { Name = "Maple.Setup" },
                    new ShellFeature { Name = "Maple.Resources" }
                },
            };

            var blueprint = _compositionStrategy.Compose(settings, descriptor);
            var shellScope = _shellContainerFactory.CreateContainer(settings, blueprint);

            return new ShellContext {
                Settings = settings,
                Descriptor = descriptor,
                Blueprint = blueprint,
                LifetimeScope = shellScope,
                Shell = shellScope.Resolve<IMapleShell>(),
            };
        }

        public ShellContext CreateDescribedContext(ShellSettings settings, ShellDescriptor shellDescriptor) {
            Logger.Debug("Creating described context for tenant {0}", settings.Name);

            var blueprint = _compositionStrategy.Compose(settings, shellDescriptor);
            var shellScope = _shellContainerFactory.CreateContainer(settings, blueprint);

            return new ShellContext
            {
                Settings = settings,
                Descriptor = shellDescriptor,
                Blueprint = blueprint,
                LifetimeScope = shellScope,
                Shell = shellScope.Resolve<IMapleShell>(),
            };
        }
    }
}