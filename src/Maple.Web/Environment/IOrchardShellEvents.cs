using Maple.Web.Events;

namespace Maple.Web.Environment {
    public interface IOrchardShellEvents : IEventHandler {
        void Activated();
        void Terminating();
    }
}
