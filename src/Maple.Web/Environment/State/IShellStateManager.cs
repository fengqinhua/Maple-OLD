using Maple.Web.Environment.State.Models;

namespace Maple.Web.Environment.State {
    public interface IShellStateManager : IDependency {
        ShellState GetShellState();
        void UpdateEnabledState(ShellFeatureState featureState, ShellFeatureState.State value);
        void UpdateInstalledState(ShellFeatureState featureState, ShellFeatureState.State value);
    }
}