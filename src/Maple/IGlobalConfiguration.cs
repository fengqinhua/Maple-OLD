using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IGlobalConfiguration
    {
    }


    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IGlobalConfiguration<out T> : IGlobalConfiguration
    {
        T Entry { get; }
    }

    public class GlobalConfiguration : IGlobalConfiguration
    {
        public static GlobalConfiguration Configuration { get; } = new GlobalConfiguration();

        internal GlobalConfiguration()
        {
        }
    }
}
