using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple.Localization
{
    public static class NullLocalizer
    {
        static NullLocalizer()
        {
            _instance = (format, args) => new LocalizedString((args == null || args.Length == 0) ? format : string.Format(format, args));
        }

        static readonly Localizer _instance;

        public static Localizer Instance { get { return _instance; } }
    }
}
