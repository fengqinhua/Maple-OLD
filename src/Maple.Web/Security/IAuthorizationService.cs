using Maple.Web.Security.Permissions;

namespace Maple.Web.Security {
    /// <summary>
    /// Entry-point for configured authorization scheme. Role-based system
    /// provided by default. 
    /// </summary>
    public interface IAuthorizationService : IDependency {
        void CheckAccess(Permission permission, IUser user);
        bool TryCheckAccess(Permission permission, IUser user);
    }
}