using System.Web;

namespace Maple.Web.Mvc.Extensions {
    public static class HttpContextBaseExtensions {
        public static bool IsBackgroundContext(this HttpContextBase httpContextBase) {
            return httpContextBase == null || httpContextBase is DependentSetting.MvcModule.HttpContextPlaceholder;
        }
    }
}
