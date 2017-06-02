﻿using System;

namespace Maple.Web.Security {
    public interface ISslSettingsProvider : IDependency {

        /// <summary>
        /// Gets whether authentication cookies should only be transmitted over SSL or not.
        /// </summary>
        bool GetRequiresSSL();
    }
}
