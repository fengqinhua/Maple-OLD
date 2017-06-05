
using Maple.Web.Environment.Configuration;
using Maple.Web.Environment.ShellBuilders;

namespace Maple.Web.Environment {
    /// <summary>
    /// 网络应用程序宿主
    /// </summary>
    public interface IMapleHost
    {
        /// <summary>
        /// 当网络应用程序启动时调用，且仅调用一次，用于初始化框架
        /// </summary>
        void Initialize();
        /// <summary>
        /// 重新加载扩展信息
        /// </summary>
        void ReloadExtensions();
        /// <summary>
        /// 当每次网络请求进入时执行
        /// </summary>
        void BeginRequest();
        /// <summary>
        /// 当每次网络请求结束时执行
        /// </summary>
        void EndRequest();

        ShellContext GetShellContext(ShellSettings shellSettings);
        /// <summary>
        /// 用于构建shell配置代码的临时独立实例
        /// </summary>
        IWorkContextScope CreateStandaloneEnvironment(ShellSettings shellSettings);
    }
}
