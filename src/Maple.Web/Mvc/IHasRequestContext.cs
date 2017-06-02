using System.Web.Routing;

namespace Maple.Web.Mvc {
    public interface IHasRequestContext {
        RequestContext RequestContext { get; }
    }
}