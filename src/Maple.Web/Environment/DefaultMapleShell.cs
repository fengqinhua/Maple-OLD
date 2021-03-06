using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Features.OwnedInstances;
using Microsoft.Owin.Builder;
using Maple.Web.Environment.Configuration;
using Maple.Logging;
using Maple.Web.Mvc.ModelBinders;
using Maple.Web.Mvc.Routes;
using Maple.Web.Owin;
using Maple.Web.Tasks;
using Maple.Web.UI;
using Maple.Web.WebApi.Routes;
using IModelBinderProvider = Maple.Web.Mvc.ModelBinders.IModelBinderProvider;

namespace Maple.Web.Environment
{
    public class DefaultMapleShell : IMapleShell
    {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IEnumerable<IRouteProvider> _routeProviders;
        private readonly IEnumerable<IHttpRouteProvider> _httpRouteProviders;
        private readonly IRoutePublisher _routePublisher;
        private readonly IEnumerable<IModelBinderProvider> _modelBinderProviders;
        private readonly IModelBinderPublisher _modelBinderPublisher;
        private readonly ISweepGenerator _sweepGenerator;
        private readonly IEnumerable<IOwinMiddlewareProvider> _owinMiddlewareProviders;
        private readonly ShellSettings _shellSettings;

        public DefaultMapleShell(
            IWorkContextAccessor workContextAccessor,
            IEnumerable<IRouteProvider> routeProviders,
            IEnumerable<IHttpRouteProvider> httpRouteProviders,
            IRoutePublisher routePublisher,
            IEnumerable<IModelBinderProvider> modelBinderProviders,
            IModelBinderPublisher modelBinderPublisher,
            ISweepGenerator sweepGenerator,
            IEnumerable<IOwinMiddlewareProvider> owinMiddlewareProviders,
            ShellSettings shellSettings)
        {
            _workContextAccessor = workContextAccessor;
            _routeProviders = routeProviders;
            _httpRouteProviders = httpRouteProviders;
            _routePublisher = routePublisher;
            _modelBinderProviders = modelBinderProviders;
            _modelBinderPublisher = modelBinderPublisher;
            _sweepGenerator = sweepGenerator;
            _owinMiddlewareProviders = owinMiddlewareProviders;
            _shellSettings = shellSettings;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Activate()
        {
            var appBuilder = new AppBuilder();
            appBuilder.Properties["host.AppName"] = _shellSettings.Name;

            using (var scope = _workContextAccessor.CreateWorkContextScope())
            {
                var orderedMiddlewares = _owinMiddlewareProviders
                    .SelectMany(p => p.GetOwinMiddlewares())
                    .OrderBy(obj => obj.Priority, new FlatPositionComparer());

                foreach (var middleware in orderedMiddlewares)
                {
                    middleware.Configure(appBuilder);
                }
            }

            // Register the Orchard middleware after all others.
            appBuilder.UseMaple();

            var pipeline = appBuilder.Build();
            var allRoutes = new List<RouteDescriptor>();
            allRoutes.AddRange(_routeProviders.SelectMany(provider => provider.GetRoutes()));
            allRoutes.AddRange(_httpRouteProviders.SelectMany(provider => provider.GetRoutes()));

            _routePublisher.Publish(allRoutes, pipeline);
            _modelBinderPublisher.Publish(_modelBinderProviders.SelectMany(provider => provider.GetModelBinders()));

            using (var scope = _workContextAccessor.CreateWorkContextScope())
            {
                using (var events = scope.Resolve<Owned<IMapleShellEvents>>())
                {
                    events.Value.Activated();
                }
            }

            _sweepGenerator.Activate();
        }

        public void Terminate()
        {
            SafelyTerminate(() =>
            {
                using (var scope = _workContextAccessor.CreateWorkContextScope())
                {
                    using (var events = scope.Resolve<Owned<IMapleShellEvents>>())
                    {
                        SafelyTerminate(() => events.Value.Terminating());
                    }
                }
            });

            SafelyTerminate(() => _sweepGenerator.Terminate());
        }

        private void SafelyTerminate(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                if (ex.IsFatal())
                {
                    throw;
                }

                Logger.Error(ex, "An unexpected error occurred while terminating the Shell");
            }
        }
    }
}
