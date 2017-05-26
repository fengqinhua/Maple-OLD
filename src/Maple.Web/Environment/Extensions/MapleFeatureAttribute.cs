using System;

namespace Maple.Web.Environment.Extensions {
    [AttributeUsage(AttributeTargets.Class)]
    public class MapleFeatureAttribute : Attribute {
        public MapleFeatureAttribute(string text) {
            FeatureName = text;
        }

        public string FeatureName { get; set; }
    }
}