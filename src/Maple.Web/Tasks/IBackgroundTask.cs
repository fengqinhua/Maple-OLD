namespace Maple.Web.Tasks {
    public interface IBackgroundTask : IDependency {
        void Sweep();
    }
}
