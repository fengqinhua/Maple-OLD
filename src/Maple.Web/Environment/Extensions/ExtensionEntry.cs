using System;
using System.Collections.Generic;
using System.Reflection;
using Maple.Web.Environment.Extensions.Models;

namespace Maple.Web.Environment.Extensions {
    public class ExtensionEntry {
        public ExtensionDescriptor Descriptor { get; set; }
        public Assembly Assembly { get; set; }
        public IEnumerable<Type> ExportedTypes { get; set; }
    }
}