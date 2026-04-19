using LibreHardwareMonitor.Hardware;

namespace DellFanManagement.App.TemperatureReaders
{
    class GenericGpuTemperatureReader : LibreHardwareMonitorTemperatureReader
    {
        /// <summary>
        /// Constructor.  使用 SystemMonitor 的共享 Computer 实例读取 GPU 温度
        /// </summary>
        public GenericGpuTemperatureReader() : base()
        {
            // 无需创建 Computer 实例，使用基类的共享实例
        }
    }
}
