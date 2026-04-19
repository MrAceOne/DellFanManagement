using LibreHardwareMonitor.Hardware;

namespace DellFanManagement.App.TemperatureReaders
{
    /// <summary>
    /// Handles reading system CPU temperatures.
    /// 使用 SystemMonitor 的共享 Computer 实例，避免重复创建导致内存占用过高
    /// </summary>
    class CpuTemperatureReader : LibreHardwareMonitorTemperatureReader
    {
        /// <summary>
        /// Constructor.  使用 SystemMonitor 的共享 Computer 实例读取 CPU 温度
        /// </summary>
        public CpuTemperatureReader() : base()
        {
            // 无需创建 Computer 实例，使用基类的共享实例
        }
    }
}
