using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;

namespace DellFanManagement.App.TemperatureReaders
{
    /// <summary>
    /// Handles reading temperatures from Libre Hardware Monitor.
    /// 使用 SystemMonitor 的共享 Computer 实例，避免重复创建导致内存占用过高
    /// </summary>
    abstract class LibreHardwareMonitorTemperatureReader : TemperatureReader, IDisposable
    {
        /// <summary>
        /// Libre Hardware Monitor computer object（共享实例）
        /// </summary>
        protected Computer _computer;

        /// <summary>
        /// 构造函数，获取 SystemMonitor 的共享 Computer 实例
        /// </summary>
        protected LibreHardwareMonitorTemperatureReader()
        {
            _computer = SystemMonitor.Instance.Computer;
        }

        /// <summary>
        /// Read all of the available temperatures into a dictionary.
        /// </summary>
        /// <returns>Dictionary with temperatures, keyed by sensor name</returns>
        public override IReadOnlyDictionary<string, int> ReadTemperatures()
        {
            Dictionary<string, int> temperatures = new();

            // OpenHardwareMonitor values
            foreach (IHardware hardware in _computer.Hardware)
            {
                hardware.Update();

                foreach (ISensor sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue && sensor.Name.Contains("Package"))
                    {
                        if (!sensor.Name.Contains("Average") && !sensor.Name.Contains("Max"))
                        {
                            int temperature = sensor.Value != null ? (int)Math.Round(sensor.Value.Value) : 0;
                            //temperatures.Add(sensor.Name, temperature);
                            temperatures.Add("CPU", temperature);
                        }
                    }
                }
            }

            return temperatures;
        }

        /// <summary>
        /// Clean up.
        /// </summary>
        public void Dispose()
        {
            // 不关闭共享的 Computer 实例，由 SystemMonitor 负责清理
        }
    }
}
