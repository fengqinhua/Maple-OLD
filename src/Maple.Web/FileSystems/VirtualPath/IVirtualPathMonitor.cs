﻿using Maple.Caching;

namespace Maple.Web.FileSystems.VirtualPath {
    /// <summary>
    /// Enable monitoring changes over virtual path
    /// </summary>
    public interface IVirtualPathMonitor : IVolatileProvider {
        IVolatileToken WhenPathChanges(string virtualPath);
    }
}