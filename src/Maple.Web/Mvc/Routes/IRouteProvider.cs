using System.Collections.Generic;

namespace Maple.Web.Mvc.Routes {
    public interface IRouteProvider : IDependency {
        /// <summary>
        /// obsolete, prefer other format for extension methods
        /// </summary>
        IEnumerable<RouteDescriptor> GetRoutes();

        void GetRoutes(ICollection<RouteDescriptor> routes);
    }
}