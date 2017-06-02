using System.Web.Security;

namespace Maple.Web.Security.Providers {
    public class DefaultSslSettingsProvider : ISslSettingsProvider {
        public bool RequireSSL { get; set; }

        public DefaultSslSettingsProvider() {
            RequireSSL = FormsAuthentication.RequireSSL;
        }

        public bool GetRequiresSSL() {
            return RequireSSL;
        }
    }
}
