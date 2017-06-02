namespace Maple.Web.Settings {
    public interface ISiteService : IDependency {
        ISite GetSiteSettings();
    }
}
