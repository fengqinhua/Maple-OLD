using System;
using System.Web.Mvc;

namespace Maple.Web.Mvc.ModelBinders {
    public class ModelBinderDescriptor {
        public Type Type { get; set; }
        public IModelBinder ModelBinder { get; set; }
    }
}