using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Maple.Web.Environment.Configuration;
//using Maple.Web.Environment.Descriptor.Models;
//using Maple.Web.Environment.Extensions;
//using Maple.Web.Environment.Extensions.Folders;
//using Maple.Web.Environment.Extensions.Loaders;
using Maple.Web.FileSystems.VirtualPath;
using Maple.Logging;

namespace Maple.Web.FileSystems.Dependencies {
    /// <summary>
    /// The purpose of this virtual path provider is to add file dependencies to .csproj files
    /// served from the "~/Modules" or "~/Themes" directory.
    /// </summary>
    public class DynamicModuleVirtualPathProvider : VirtualPathProvider, ICustomVirtualPathProvider {
        private readonly IExtensionDependenciesManager _extensionDependenciesManager;
        private string[] _extensionsVirtualPathPrefixes;


        public DynamicModuleVirtualPathProvider(IExtensionDependenciesManager extensionDependenciesManager, ExtensionLocations extensionLocations) {
            _extensionDependenciesManager = extensionDependenciesManager;
            Logger = NullLogger.Instance;

            _extensionsVirtualPathPrefixes = extensionLocations.ExtensionsVirtualPathPrefixes;
        }

        public ILogger Logger { get; set; }

        public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies) {
            var result = GetFileHashWorker(virtualPath, virtualPathDependencies);
            Logger.Debug("GetFileHash(\"{0}\"): {1}", virtualPath, result);
            return result;
        }

        private string GetFileHashWorker(string virtualPath, IEnumerable virtualPathDependencies) {
            virtualPath = VirtualPathUtility.ToAppRelative(virtualPath);

            var desc = GetExtensionDescriptor(virtualPath);
            if (desc != null) {
                if (desc.VirtualPath.Equals(virtualPath, StringComparison.OrdinalIgnoreCase)) {
                    return desc.Hash;
                }
            }
            return base.GetFileHash(virtualPath, virtualPathDependencies);
        }

        private ActivatedExtensionDescriptor GetExtensionDescriptor(string virtualPath) {
            var prefix = PrefixMatch(virtualPath, _extensionsVirtualPathPrefixes);
            if (prefix == null)
                return null;

            var moduleId = ModuleMatch(virtualPath, prefix);
            if (moduleId == null)
                return null;

            return _extensionDependenciesManager.GetDescriptor(moduleId);
        }

        private static string ModuleMatch(string virtualPath, string prefix) {
            var index = virtualPath.IndexOf('/', prefix.Length, virtualPath.Length - prefix.Length);
            if (index < 0)
                return null;

            var moduleId = virtualPath.Substring(prefix.Length, index - prefix.Length);
            return (string.IsNullOrEmpty(moduleId) ? null : moduleId);
        }

        private static string PrefixMatch(string virtualPath, params string[] prefixes) {
            return prefixes
                .FirstOrDefault(p => virtualPath.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        }

        VirtualPathProvider ICustomVirtualPathProvider.Instance {
            get { return this; }
        }
    }
}