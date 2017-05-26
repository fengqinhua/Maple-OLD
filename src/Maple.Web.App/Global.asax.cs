using Autofac;
using Maple.Web.Environment;
using Maple.Web.WarmupStarter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace Maple.Web.App
{
    public class Global : System.Web.HttpApplication
    {
        private static Starter<IMapleHost> _starter;

        public Global()
        {
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalConfiguration.Configuration.UseLogger(new Extended.Log4netLogger());

            RegisterRoutes(RouteTable.Routes);
            _starter = new Starter<IMapleHost>(HostInitialization, HostBeginRequest, HostEndRequest);
            _starter.OnApplicationStart(this);
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            _starter.OnBeginRequest(this);
        }
        protected void Application_EndRequest(object sender, EventArgs e)
        {
            _starter.OnEndRequest(this);
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }


        private static void HostBeginRequest(HttpApplication application, IMapleHost host)
        {
            application.Context.Items["originalHttpContext"] = application.Context;
            host.BeginRequest();
        }

        private static void HostEndRequest(HttpApplication application, IMapleHost host)
        {
            host.EndRequest();
        }

        private static IMapleHost HostInitialization(HttpApplication application)
        {
            var host = MapleStarter.CreateHost(MvcSingletons);

            host.Initialize();

            host.BeginRequest();
            host.EndRequest();

            return host;
        }

        static void MvcSingletons(ContainerBuilder builder)
        {
            builder.Register(ctx => RouteTable.Routes).SingleInstance();
            builder.Register(ctx => ModelBinders.Binders).SingleInstance();
            builder.Register(ctx => ViewEngines.Engines).SingleInstance();
        }
    }
}