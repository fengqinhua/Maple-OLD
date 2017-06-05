using Maple.Web.Environment.Extensions.Models;
using Maple.Web.Events;

namespace Maple.Web.Environment {
    public interface IFeatureEventHandler : IEventHandler {
        void Installing(Feature feature);
        void Installed(Feature feature);
        void Enabling(Feature feature);
        void Enabled(Feature feature);
        void Disabling(Feature feature);
        void Disabled(Feature feature);
        void Uninstalling(Feature feature);
        void Uninstalled(Feature feature);
    }
}