﻿using System;
using System.ComponentModel.DataAnnotations;
using Maple.Localization;

namespace Maple.Web.Mvc.DataAnnotations {
    public class LocalizedRequiredAttribute : RequiredAttribute {
        public LocalizedRequiredAttribute(RequiredAttribute attribute, Localizer t) {
            AllowEmptyStrings = attribute.AllowEmptyStrings;

            if ( !String.IsNullOrEmpty(attribute.ErrorMessage) )
                ErrorMessage = attribute.ErrorMessage;

            T = t;
        }

        public Localizer T { get; set; }

        public override string FormatErrorMessage(string name) {
            return String.IsNullOrEmpty(ErrorMessage)
                ? T("The {0} field is required.", name).Text
                : T(ErrorMessage, name).Text;
        }
    }
}
