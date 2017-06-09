using Maple.Web.Events;

namespace Maple.Web.Environment {
    public interface IMapleShellEvents : IEventHandler {
        void Activated();
        void Terminating();
    }
}
