using System.Collections.Generic;

namespace Maple.Web.Mvc.ModelBinders {
    public interface IModelBinderPublisher : IDependency {
        void Publish(IEnumerable<ModelBinderDescriptor> binders);
    }
}