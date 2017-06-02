using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Maple.Web
{
    public interface IWorkContextAccessor
    {
        WorkContext GetContext(HttpContextBase httpContext);
        IWorkContextScope CreateWorkContextScope(HttpContextBase httpContext);

        WorkContext GetContext();
        IWorkContextScope CreateWorkContextScope();
    }

    public interface ILogicalWorkContextAccessor : IWorkContextAccessor
    {
        WorkContext GetLogicalContext();
    }

    public interface IWorkContextStateProvider : IDependency
    {
        Func<WorkContext, T> Get<T>(string name);
    }

    public interface IWorkContextScope : IDisposable
    {
        WorkContext WorkContext { get; }
        TService Resolve<TService>();
        bool TryResolve<TService>(out TService service);
    }
}
