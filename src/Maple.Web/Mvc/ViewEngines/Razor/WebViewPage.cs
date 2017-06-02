using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
//using Maple.Web.ContentManagement;
//using Maple.Web.DisplayManagement;
//using Maple.Web.DisplayManagement.Shapes;
using Maple.Web.Environment.Configuration;
using Maple.Localization;
using Maple.Web.Mvc.Html;
using Maple.Web.Mvc.Spooling;
using Maple.Web.Security;
using Maple.Web.Security.Permissions;
using Maple.Web.UI.Resources;

namespace Maple.Web.Mvc.ViewEngines.Razor {

    public abstract class WebViewPage<TModel> : System.Web.Mvc.WebViewPage<TModel>, IOrchardViewPage
    {
        private IResourceManager _resourceManager;
        private ScriptRegister _scriptRegister;
        private ResourceRegister _stylesheetRegister;
        private Localizer _localizer = NullLocalizer.Instance;
        private IAuthorizer _authorizer;
        private string[] _commonLocations;
        private string _tenantPrefix;
        public Localizer T { 
            get {
                return _localizer;
            } 
        }
        public WorkContext WorkContext { get; set; }
        public IAuthorizer Authorizer { 
            get {
                return _authorizer ?? (_authorizer = WorkContext.Resolve<IAuthorizer>());
            }
        }
        public ScriptRegister Script {
            get {
                return _scriptRegister ??
                    (_scriptRegister = new WebViewScriptRegister(this, Html.ViewDataContainer, ResourceManager));
            }
        }
        public IResourceManager ResourceManager {
            get { return _resourceManager ?? (_resourceManager = WorkContext.Resolve<IResourceManager>()); }
        }
        public ResourceRegister Style {
            get {
                return _stylesheetRegister ??
                    (_stylesheetRegister = new ResourceRegister(Html.ViewDataContainer, ResourceManager, "stylesheet"));
            }
        }
        public string[] CommonLocations { get {
                return _commonLocations ?? (_commonLocations = WorkContext.Resolve<ExtensionLocations>().CommonLocations); }
        } 
        public void RegisterImageSet(string imageSet, string style = "", int size = 16) {
            // hack to fake the style "alternate" for now so we don't have to change stylesheet names when this is hooked up
            // todo: (heskew) deal in shapes so we have real alternates 
            var imageSetStylesheet = !string.IsNullOrWhiteSpace(style)
                ? string.Format("{0}-{1}.css", imageSet, style)
                : string.Format("{0}.css", imageSet);
            Style.Include(imageSetStylesheet);
        }
        public virtual void RegisterLink(LinkEntry link) {
            ResourceManager.RegisterLink(link);
        }
        public void SetMeta(string name = null, string content = null, string httpEquiv = null, string charset = null) {
            var metaEntry = new MetaEntry(name, content, httpEquiv, charset);
            SetMeta(metaEntry);
        }
        public virtual void SetMeta(MetaEntry meta) {
            ResourceManager.SetMeta(meta);
        }
        public void AppendMeta(string name, string content, string contentSeparator) {
            AppendMeta(new MetaEntry { Name = name, Content = content }, contentSeparator);
        }
        public virtual void AppendMeta(MetaEntry meta, string contentSeparator) {
            ResourceManager.AppendMeta(meta, contentSeparator);
        }
        public bool AuthorizedFor(Permission permission) {
            return Authorizer.Authorize(permission);
        }
        public bool HasText(object thing) {
            return !string.IsNullOrWhiteSpace(Convert.ToString(thing));
        } 
        public override string Href(string path, params object[] pathParts) {
            if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) {
                return path;
            }

            if (_tenantPrefix == null) {
                _tenantPrefix = WorkContext.Resolve<ShellSettings>().RequestUrlPrefix ?? "";
            }

            if (!String.IsNullOrEmpty(_tenantPrefix)
                && path.StartsWith("~/")  
                && !CommonLocations.Any(gpp=>path.StartsWith(gpp, StringComparison.OrdinalIgnoreCase))
            ) { 
                    return base.Href("~/" + _tenantPrefix + path.Substring(2), pathParts);
            }

            return base.Href(path, pathParts);
        }
        class WebViewScriptRegister : ScriptRegister {
            private readonly WebPageBase _viewPage;

            public WebViewScriptRegister(WebPageBase viewPage, IViewDataContainer container, IResourceManager resourceManager)
                : base(container, resourceManager) {
                _viewPage = viewPage;
            }

            public override IDisposable Head() {
                return null;
                //return new CaptureScope(_viewPage, s => ResourceManager.RegisterHeadScript(s.ToString()));
            }

            public override IDisposable Foot() {
                return null;
                //return new CaptureScope(_viewPage, s => ResourceManager.RegisterFootScript(s.ToString()));
            }
        }
    }

    public abstract class WebViewPage : WebViewPage<dynamic> {
    }
}
