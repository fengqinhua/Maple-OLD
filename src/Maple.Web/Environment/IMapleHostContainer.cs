namespace Maple.Web.Environment {
    public interface IMapleHostContainer {
        T Resolve<T>();
    }
}