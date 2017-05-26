using Maple.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Maple.Web
{
    [Serializable]
    public class MapleWebCoreException : Exception
    {
        private readonly LocalizedString _localizedMessage;

        public MapleWebCoreException(LocalizedString message)
            : base(message.Text)
        {
            _localizedMessage = message;
        }

        public MapleWebCoreException(LocalizedString message, Exception innerException)
            : base(message.Text, innerException)
        {
            _localizedMessage = message;
        }

        protected MapleWebCoreException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public LocalizedString LocalizedMessage { get { return _localizedMessage; } }
    }
}
