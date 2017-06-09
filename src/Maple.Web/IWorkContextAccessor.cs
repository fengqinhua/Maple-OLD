using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Maple.Web
{
    /// <summary>
    /// WorkContext访问器
    /// </summary>
    public interface IWorkContextAccessor
    {
        /// <summary>
        /// 获得工作上下文
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
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

    /// <summary>
    /// 工作上下文管理器
    /// </summary>
    public interface IWorkContextScope : IDisposable
    {
        WorkContext WorkContext { get; }
        TService Resolve<TService>();
        bool TryResolve<TService>(out TService service);
    }
}
