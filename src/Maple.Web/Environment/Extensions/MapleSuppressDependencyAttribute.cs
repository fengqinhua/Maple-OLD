using System;

namespace Maple.Web.Environment.Extensions {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class MapleSuppressDependencyAttribute : Attribute {
        public MapleSuppressDependencyAttribute(string fullName) {
            FullName = fullName;
        }

        public string FullName { get; set; }
    }
}