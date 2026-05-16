namespace DellFanManagement.Service
{
    /// <summary>
    /// 风扇运行模式
    /// </summary>
    public enum FanMode
    {
        /// <summary>
        /// 0 - 手动控制模式（用户自定义风扇参数）
        /// </summary>
        Manual = 0,

        /// <summary>
        /// 1 - 优化散热模式（Optimized）
        /// </summary>
        Optimized = 1,

        /// <summary>
        /// 2 - 酷凉模式（Cool）
        /// </summary>
        Cool = 2,

        /// <summary>
        /// 3 - 静音模式（Quiet）
        /// </summary>
        Quiet = 3,

        /// <summary>
        /// 4 - 极速模式（Performance）
        /// </summary>
        Performance = 4
    }
}
