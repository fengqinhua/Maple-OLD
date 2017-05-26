using System.Collections.Generic;
using Maple.Web.Environment.Extensions.Models;

namespace Maple.Web.Environment.Extensions.Folders {
    public interface IExtensionFolders {
        IEnumerable<ExtensionDescriptor> AvailableExtensions();
    }
}