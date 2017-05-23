using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple.Caching
{
    public interface IAsyncTokenProvider
    {
        IVolatileToken GetToken(Action<Action<IVolatileToken>> task);
    }
}
