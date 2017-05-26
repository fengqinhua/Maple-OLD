using System.Collections.Generic;
using Maple.Localization;

namespace Maple.Web.Environment.Extensions {
    public interface ICriticalErrorProvider {
        IEnumerable<LocalizedString> GetErrors();

        /// <summary>
        /// Called by any to notice the system of a critical issue at the system level, e.g. incorrect extensions
        /// </summary>
        void RegisterErrorMessage(LocalizedString message);

        /// <summary>
        /// Removes all error message
        /// </summary>
        void Clear();

    }
}
