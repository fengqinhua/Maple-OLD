using System.Web;

namespace Maple.Web.Mvc {
    /// <summary>
    /// HttpContext访问器
    /// </summary>
    public interface IHttpContextAccessor {
        HttpContextBase Current();
        void Set(HttpContextBase httpContext);
    }
}
