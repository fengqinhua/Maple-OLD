using System.Collections.Generic;
using Maple.Web.Environment.Extensions.Models;

namespace Maple.Web.Environment.Extensions.Folders {
    public interface IExtensionHarvester {
        IEnumerable<ExtensionDescriptor> HarvestExtensions(IEnumerable<string> paths, string extensionType, string manifestName, bool manifestIsOptional);
    }
}