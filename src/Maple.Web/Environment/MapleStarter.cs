﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using Autofac;
using Autofac.Configuration;
using System.Linq;
using System.Web.Configuration;
using Maple.Caching;
using Maple.Web.Environment.Extensions.Folders;
using Maple.Web.Environment.Extensions.Loaders;
using Maple.Web.Environment.Configuration;
using Maple.Web.DependentSetting;
using Maple.Web.FileSystems.WebSite;
using Maple.Web.FileSystems.AppData;
using Maple.Web.FileSystems.LockFile;
using Maple.Web.Services;
using Maple.Web.FileSystems.Dependencies;
using Maple.Web.FileSystems.VirtualPath;
using Maple.Web.Environment.Extensions.Compilers;
using Maple.Web.Environment.Extensions;
using Maple.Web.Exceptions;
using Maple.Web.Mvc;
using Maple.Web.Events;
using Maple.Web.Mvc.ViewEngines.Razor;
using Maple.Web.Environment.Descriptor;
using Maple.Web.Environment.ShellBuilders;
using Maple.Web.Environment.State;

namespace Maple.Web.Environment {
    public static class MapleStarter
    {
        public static IContainer CreateHostContainer(Action<ContainerBuilder> registrations) {
            ExtensionLocations extensionLocations = new ExtensionLocations();

            var builder = new ContainerBuilder();
            // Application paths and parameters
            builder.RegisterInstance(extensionLocations);

            //builder.RegisterModule(new CollectionOrderModule());
            builder.RegisterModule(new LoggingModule());
            builder.RegisterModule(new EventsModule());
            builder.RegisterModule(new CacheModule());

            // a single default host implementation is needed for bootstrapping a web app domain
            builder.RegisterType<DefaultOrchardEventBus>().As<IEventBus>().SingleInstance();

            builder.RegisterType<DefaultCacheHolder>().As<ICacheHolder>().SingleInstance();
            builder.RegisterType<DefaultCacheContextAccessor>().As<ICacheContextAccessor>().SingleInstance();
            builder.RegisterType<DefaultParallelCacheContext>().As<IParallelCacheContext>().SingleInstance();
            builder.RegisterType<DefaultAsyncTokenProvider>().As<IAsyncTokenProvider>().SingleInstance();

            builder.RegisterType<DefaultHostEnvironment>().As<IHostEnvironment>().SingleInstance();
            builder.RegisterType<DefaultHostLocalRestart>().As<IHostLocalRestart>().Named<IEventHandler>(typeof(IShellSettingsManagerEventHandler).Name).SingleInstance();
            builder.RegisterType<DefaultBuildManager>().As<IBuildManager>().SingleInstance();
            builder.RegisterType<DynamicModuleVirtualPathProvider>().As<ICustomVirtualPathProvider>().SingleInstance();
            builder.RegisterType<AppDataFolderRoot>().As<IAppDataFolderRoot>().SingleInstance();
            builder.RegisterType<DefaultExtensionCompiler>().As<IExtensionCompiler>().SingleInstance();
            builder.RegisterType<DefaultRazorCompilationEvents>().As<IRazorCompilationEvents>().SingleInstance();
            builder.RegisterType<DefaultProjectFileParser>().As<IProjectFileParser>().SingleInstance();
            builder.RegisterType<DefaultAssemblyLoader>().As<IAssemblyLoader>().SingleInstance();
            builder.RegisterType<AppDomainAssemblyNameResolver>().As<IAssemblyNameResolver>().SingleInstance();
            builder.RegisterType<GacAssemblyNameResolver>().As<IAssemblyNameResolver>().SingleInstance();
            builder.RegisterType<OrchardFrameworkAssemblyNameResolver>().As<IAssemblyNameResolver>().SingleInstance();
            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().InstancePerDependency();
            builder.RegisterType<ViewsBackgroundCompilation>().As<IViewsBackgroundCompilation>().SingleInstance();
            builder.RegisterType<DefaultExceptionPolicy>().As<IExceptionPolicy>().SingleInstance();
            builder.RegisterType<DefaultCriticalErrorProvider>().As<ICriticalErrorProvider>().SingleInstance();
            //builder.RegisterType<RazorTemplateCache>().As<IRazorTemplateProvider>().SingleInstance();

            RegisterVolatileProvider<WebSiteFolder, IWebSiteFolder>(builder);
            RegisterVolatileProvider<AppDataFolder, IAppDataFolder>(builder);
            RegisterVolatileProvider<DefaultLockFileManager, ILockFileManager>(builder);
            RegisterVolatileProvider<Clock, IClock>(builder);
            RegisterVolatileProvider<DefaultDependenciesFolder, IDependenciesFolder>(builder);
            RegisterVolatileProvider<DefaultExtensionDependenciesManager, IExtensionDependenciesManager>(builder);
            RegisterVolatileProvider<DefaultAssemblyProbingFolder, IAssemblyProbingFolder>(builder);
            RegisterVolatileProvider<DefaultVirtualPathMonitor, IVirtualPathMonitor>(builder);
            RegisterVolatileProvider<DefaultVirtualPathProvider, IVirtualPathProvider>(builder);


            builder.RegisterType<DefaultMapleHost>().As<IMapleHost>().As<IEventHandler>()
                .Named<IEventHandler>(typeof(IShellSettingsManagerEventHandler).Name)
                .Named<IEventHandler>(typeof(IShellDescriptorManagerEventHandler).Name)
                .SingleInstance();
            {
                builder.RegisterType<ShellSettingsManager>().As<IShellSettingsManager>().SingleInstance();

                builder.RegisterType<ShellContextFactory>().As<IShellContextFactory>().SingleInstance();
                {
                    builder.RegisterType<ShellDescriptorCache>().As<IShellDescriptorCache>().SingleInstance();

                    builder.RegisterType<CompositionStrategy>().As<ICompositionStrategy>().SingleInstance();
                    {
                        builder.RegisterType<ShellContainerRegistrations>().As<IShellContainerRegistrations>().SingleInstance();
                        builder.RegisterType<ExtensionLoaderCoordinator>().As<IExtensionLoaderCoordinator>().SingleInstance();
                        builder.RegisterType<ExtensionMonitoringCoordinator>().As<IExtensionMonitoringCoordinator>().SingleInstance();
                        builder.RegisterType<ExtensionManager>().As<IExtensionManager>().SingleInstance();
                        {
                            builder.RegisterType<ExtensionHarvester>().As<IExtensionHarvester>().SingleInstance();
                            builder.RegisterType<ModuleFolders>().As<IExtensionFolders>().SingleInstance()
                                .WithParameter(new NamedParameter("paths", extensionLocations.ModuleLocations));
                            builder.RegisterType<CoreModuleFolders>().As<IExtensionFolders>().SingleInstance()
                                .WithParameter(new NamedParameter("paths", extensionLocations.CoreLocations));
                            builder.RegisterType<ThemeFolders>().As<IExtensionFolders>().SingleInstance()
                                .WithParameter(new NamedParameter("paths", extensionLocations.ThemeLocations));

                            builder.RegisterType<CoreExtensionLoader>().As<IExtensionLoader>().SingleInstance();
                            builder.RegisterType<ReferencedExtensionLoader>().As<IExtensionLoader>().SingleInstance();
                            builder.RegisterType<PrecompiledExtensionLoader>().As<IExtensionLoader>().SingleInstance();
                            builder.RegisterType<DynamicExtensionLoader>().As<IExtensionLoader>().SingleInstance();
                            builder.RegisterType<RawThemeExtensionLoader>().As<IExtensionLoader>().SingleInstance();
                        }
                    }

                    builder.RegisterType<ShellContainerFactory>().As<IShellContainerFactory>().SingleInstance();
                }

                builder.RegisterType<DefaultProcessingEngine>().As<IProcessingEngine>().SingleInstance();
            }

            builder.RegisterType<RunningShellTable>().As<IRunningShellTable>().SingleInstance();
            builder.RegisterType<DefaultMapleShell>().As<IMapleShell>().InstancePerMatchingLifetimeScope("shell");
            //builder.RegisterType<SessionConfigurationCache>().As<ISessionConfigurationCache>().InstancePerMatchingLifetimeScope("shell");

            registrations(builder);

            //var autofacSection = ConfigurationManager.GetSection(ConfigurationSettingsReaderConstants.DefaultSectionName);
            //if (autofacSection != null)
            //    builder.RegisterModule(new ConfigurationSettingsReader());

            //var optionalHostConfig = HostingEnvironment.MapPath("~/Config/Host.config");
            //if (File.Exists(optionalHostConfig))
            //    builder.RegisterModule(new ConfigurationSettingsReader(ConfigurationSettingsReaderConstants.DefaultSectionName, optionalHostConfig));

            //var optionalComponentsConfig = HostingEnvironment.MapPath("~/Config/HostComponents.config");
            //if (File.Exists(optionalComponentsConfig))
            //    builder.RegisterModule(new HostComponentsConfigModule(optionalComponentsConfig));

            var container = builder.Build();

            ////
            //// Register Virtual Path Providers
            ////
            //if (HostingEnvironment.IsHosted) {
            //    foreach (var vpp in container.Resolve<IEnumerable<ICustomVirtualPathProvider>>()) {
            //        HostingEnvironment.RegisterVirtualPathProvider(vpp.Instance);
            //    }
            //}

            //ControllerBuilder.Current.SetControllerFactory(new OrchardControllerFactory());
            //FilterProviders.Providers.Add(new OrchardFilterProvider());

            //GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerSelector), new DefaultOrchardWebApiHttpControllerSelector(GlobalConfiguration.Configuration));
            //GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerActivator), new DefaultOrchardWebApiHttpControllerActivator(GlobalConfiguration.Configuration));
            //GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            //GlobalConfiguration.Configuration.Filters.Add(new OrchardApiActionFilterDispatcher());
            //GlobalConfiguration.Configuration.Filters.Add(new OrchardApiExceptionFilterDispatcher());
            //GlobalConfiguration.Configuration.Filters.Add(new OrchardApiAuthorizationFilterDispatcher());

            //ViewEngines.Engines.Clear();
            //ViewEngines.Engines.Add(new ThemeAwareViewEngineShim());

            //var hostContainer = new DefaultOrchardHostContainer(container);
            ////MvcServiceLocator.SetCurrent(hostContainer);
            //OrchardHostContainerRegistry.RegisterHostContainer(hostContainer);

            //// Register localized data annotations
            //ModelValidatorProviders.Providers.Clear();
            //ModelValidatorProviders.Providers.Add(new LocalizedModelValidatorProvider());

            return container;
        }

        private static void RegisterVolatileProvider<TRegister, TService>(ContainerBuilder builder) where TService : IVolatileProvider {
            builder.RegisterType<TRegister>()
                .As<TService>()
                .As<IVolatileProvider>()
                .SingleInstance();
        }

        public static IMapleHost CreateHost(Action<ContainerBuilder> registrations) {
            var container = CreateHostContainer(registrations);
            return container.Resolve<IMapleHost>();
        }
    }
}
