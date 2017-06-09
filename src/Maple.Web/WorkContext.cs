using System;
using System.Web;
using Maple.Web.Environment.Extensions.Models;
using Maple.Web.Security;
using Maple.Web.Settings;

namespace Maple.Web
{
    /// <summary>
    /// 工作上下文
    /// </summary>
    public abstract class WorkContext
    {
        /// <summary>
        /// 获得已注册的类型
        /// </summary>
        /// <typeparam name="T">The type of the dependency.</typeparam>
        /// <returns>An instance of the dependency if it could be resolved.</returns>
        public abstract T Resolve<T>();

        /// <summary>
        /// 获得已注册的类型
        /// </summary>
        /// <param name="serviceType">The type of the dependency.</param>
        /// <returns>An instance of the dependency if it could be resolved.</returns>
        public abstract object Resolve(Type serviceType);

        /// <summary>
        /// 获得已注册的类型
        /// </summary>
        /// <typeparam name="T">The type of the dependency.</typeparam>
        /// <param name="service">An instance of the dependency if it could be resolved.</param>
        /// <returns>True if the dependency could be resolved, false otherwise.</returns>
        public abstract bool TryResolve<T>(out T service);

        /// <summary>
        /// 获得已注册的类型
        /// </summary>
        /// <param name="serviceType">The type of the dependency.</param>
        /// <param name="service">An instance of the dependency if it could be resolved.</param>
        /// <returns>True if the dependency could be resolved, false otherwise.</returns>
        public abstract bool TryResolve(Type serviceType, out object service);

        /// <summary>
        /// 设置状态信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public abstract T GetState<T>(string name);
        /// <summary>
        /// 读取状态信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public abstract void SetState<T>(string name, T value);

        /// <summary>
        /// 获得工作上下文中的 HttpContext 信息
        /// </summary>
        public HttpContextBase HttpContext
        {
            get { return GetState<HttpContextBase>("HttpContext"); }
            set { SetState("HttpContext", value); }
        }

        /// <summary>
        /// 获得工作上下文中的 Layout 信息
        /// </summary>
        public dynamic Layout
        {
            get { return GetState<dynamic>("Layout"); }
            set { SetState("Layout", value); }
        }

        /// <summary>
        /// 获得工作上下文中的 网站站点 信息
        /// </summary>
        public ISite CurrentSite
        {
            get { return GetState<ISite>("CurrentSite"); }
            set { SetState("CurrentSite", value); }
        }

        /// <summary>
        /// 获得工作上下文中的 当前用户 信息
        /// </summary>
        public IUser CurrentUser
        {
            get { return GetState<IUser>("CurrentUser"); }
            set { SetState("CurrentUser", value); }
        }

        /// <summary>
        /// 获得工作上下文中的 皮肤 信息
        /// </summary>
        public ExtensionDescriptor CurrentTheme
        {
            get { return GetState<ExtensionDescriptor>("CurrentTheme"); }
            set { SetState("CurrentTheme", value); }
        }

        /// <summary>
        /// 获得工作上下文中的 语言 信息
        /// </summary>
        public string CurrentCulture
        {
            get { return GetState<string>("CurrentCulture"); }
            set { SetState("CurrentCulture", value); }
        }

        /// <summary>
        /// 获得工作上下文中的 日历 信息
        /// </summary>
        public string CurrentCalendar
        {
            get { return GetState<string>("CurrentCalendar"); }
            set { SetState("CurrentCalendar", value); }
        }

        /// <summary>
        /// 获得工作上下文中的 时区 信息
        /// </summary>
        public TimeZoneInfo CurrentTimeZone
        {
            get { return GetState<TimeZoneInfo>("CurrentTimeZone"); }
            set { SetState("CurrentTimeZone", value); }
        }
    }
}
