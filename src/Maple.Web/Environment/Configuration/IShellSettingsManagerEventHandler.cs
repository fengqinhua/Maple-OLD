using Maple.Web.Events;

namespace Maple.Web.Environment.Configuration {
    public interface IShellSettingsManagerEventHandler : IEventHandler {
        void Saved(ShellSettings settings);
    }
}