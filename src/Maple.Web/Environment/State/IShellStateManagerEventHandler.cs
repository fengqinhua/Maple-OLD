using Maple.Web.Events;

namespace Maple.Web.Environment.State {
    public interface IShellStateManagerEventHandler : IEventHandler {
        void ApplyChanges();
    }
}