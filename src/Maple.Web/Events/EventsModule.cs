﻿using Autofac;

namespace Maple.Web.Events {
    internal class EventsModule : Module {
        protected override void Load(ContainerBuilder builder) {
            builder.RegisterSource(new EventsRegistrationSource());
            base.Load(builder);
        }
    }
}
