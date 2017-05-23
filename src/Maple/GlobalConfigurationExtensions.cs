using Maple.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class GlobalConfigurationExtensions
    {
        public static IGlobalConfiguration<TStorage> UseLogger<TStorage>(
            this IGlobalConfiguration configuration,
           TStorage storage) where TStorage : ILogger
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (storage == null) throw new ArgumentNullException(nameof(storage));

            return configuration.Use(storage, x => LoggerManagers.Instance.UseLogger(x));
        }


        public static IGlobalConfiguration<T> Use<T>(this IGlobalConfiguration configuration, T entry, Action<T> entryAction)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            entryAction(entry);
            return new ConfigurationEntry<T>(entry);
        }


        private class ConfigurationEntry<T> : IGlobalConfiguration<T>
        {
            public ConfigurationEntry(T entry)
            {
                Entry = entry;
            }
            public T Entry { get; }
        }
    }
}
