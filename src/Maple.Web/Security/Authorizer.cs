//using Maple.Web.ContentManagement;
using Maple.Localization;
using Maple.Web.Security.Permissions;
using System;
//using Maple.Web.UI.Notify;

namespace Maple.Web.Security
{
    /// <summary>
    /// Authorization services for the current user
    /// </summary>
    public interface IAuthorizer : IDependency
    {
        /// <summary>
        /// Authorize the current user against a permission
        /// </summary>
        /// <param name="permission">A permission to authorize against</param>
        bool Authorize(Permission permission);

        /// <summary>
        /// Authorize the current user against a permission; if authorization fails, the specified
        /// message will be displayed
        /// </summary>
        /// <param name="permission">A permission to authorize against</param>
        /// <param name="message">A localized message to display if authorization fails</param>
        bool Authorize(Permission permission, LocalizedString message);
    }

    public class Authorizer : IAuthorizer
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IWorkContextAccessor _workContextAccessor;

        public Authorizer(IAuthorizationService authorizationService,
            IWorkContextAccessor workContextAccessor)
        {
            _authorizationService = authorizationService;
            _workContextAccessor = workContextAccessor;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public bool Authorize(Permission permission)
        {
            return Authorize(permission, null);
        }

        public bool Authorize(Permission permission, LocalizedString message)
        {
            IUser user = _workContextAccessor.GetContext().CurrentUser;

            if (_authorizationService != null && _authorizationService.TryCheckAccess(permission, user))
                return true;

            if (message != null)
            {
                if (user == null)
                {
                    //_notifier.Error(T("{0}. Anonymous users do not have {1} permission.",
                    //                  message, permission.Name));

                    System.Diagnostics.Debug.WriteLine(T("{0}. Anonymous users do not have {1} permission.",
                                      message, permission.Name));
                }
                else
                {
                    //_notifier.Error(T("{0}. Current user, {2}, does not have {1} permission.",
                    //                  message, permission.Name, user.UserName));

                    System.Diagnostics.Debug.WriteLine(T("{0}. Current user, {2}, does not have {1} permission.",
                            message, permission.Name, user.UserName));
                }
            }

            return false;
        }

    }


    //public class Authorizer : IAuthorizer {
    //    private readonly IAuthorizationService _authorizationService;
    //    private readonly INotifier _notifier;
    //    private readonly IWorkContextAccessor _workContextAccessor;

    //    public Authorizer(
    //        IAuthorizationService authorizationService,
    //        INotifier notifier,
    //        IWorkContextAccessor workContextAccessor) {
    //        _authorizationService = authorizationService;
    //        _notifier = notifier;
    //        _workContextAccessor = workContextAccessor;
    //        T = NullLocalizer.Instance;
    //    }

    //    public Localizer T { get; set; }

    //    public bool Authorize(Permission permission) {
    //        return Authorize(permission, null, null);
    //    }

    //    public bool Authorize(Permission permission, LocalizedString message) {
    //        return Authorize(permission, null, message);
    //    }

    //    public bool Authorize(Permission permission, IContent content) {
    //        return Authorize(permission, content, null);
    //    }

    //    public bool Authorize(Permission permission, IContent content, LocalizedString message) {
    //        if (_authorizationService.TryCheckAccess(permission, _workContextAccessor.GetContext().CurrentUser, content))
    //            return true;

    //        if (message != null) {
    //            if (_workContextAccessor.GetContext().CurrentUser == null) {
    //                _notifier.Error(T("{0}. Anonymous users do not have {1} permission.",
    //                                  message, permission.Name));
    //            }
    //            else {
    //                _notifier.Error(T("{0}. Current user, {2}, does not have {1} permission.",
    //                                  message, permission.Name, _workContextAccessor.GetContext().CurrentUser.UserName));
    //            }
    //        }

    //        return false;
    //    }

    //}
}
