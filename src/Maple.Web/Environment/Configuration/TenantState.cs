namespace Maple.Web.Environment.Configuration {
    public enum TenantState {
        /// <summary>
        /// 未初始化
        /// </summary>
        Uninitialized,
        /// <summary>
        /// 初始化
        /// </summary>
        Initializing,
        /// <summary>
        /// 运行中
        /// </summary>
        Running,
        /// <summary>
        /// 禁用
        /// </summary>
        Disabled,
        /// <summary>
        /// 无效
        /// </summary>
        Invalid
    }
}
