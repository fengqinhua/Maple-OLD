using System.Web;

namespace Maple.Web.Mvc {
    public interface IHttpContextAccessor {
        HttpContextBase Current();
        void Set(HttpContextBase httpContext);
    }
}
