using System.Collections.Generic;
using Maple.Web.Mvc.Routes;

namespace Maple.Web.WebApi.Routes {
    public interface IHttpRouteProvider : IDependency {
        IEnumerable<RouteDescriptor> GetRoutes();
        void GetRoutes(ICollection<RouteDescriptor> routes);
    }
}
