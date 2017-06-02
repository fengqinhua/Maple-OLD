using System.Collections.Generic;

namespace Maple.Web.Mvc.ModelBinders {
    public interface IModelBinderProvider : IDependency {
        IEnumerable<ModelBinderDescriptor> GetModelBinders();
    }
}