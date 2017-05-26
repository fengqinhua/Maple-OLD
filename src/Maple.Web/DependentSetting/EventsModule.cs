using Autofac;
using Maple.Web.Events;

namespace Maple.Web.DependentSetting
{
    internal class EventsModule : Module {
        protected override void Load(ContainerBuilder builder) {
            builder.RegisterSource(new EventsRegistrationSource());
            base.Load(builder);
        }
    }
}
