﻿using System.Web.Mvc;
using System.Web.Mvc.Html;
using Maple.Web.Environment.Extensions.Models;
using Maple.Web.Validation;

namespace Maple.Web.Mvc.Html {
    public static class ThemeExtensions {
        /// <summary>
        /// Include a view from the current view.
        /// </summary>
        public static void Include(this HtmlHelper helper, string viewName) {
            Argument.ThrowIfNull(helper, "helper");
            helper.RenderPartial(viewName);
        }

        public static string ThemePath(this HtmlHelper helper, ExtensionDescriptor theme, string path) {
            return theme.Location + "/" + theme.Id + path;
        }
    }
}
