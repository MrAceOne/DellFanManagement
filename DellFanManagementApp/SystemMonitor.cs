using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DellFanManagement.App
{
    /// <summary>
    /// 系统监测数据结构
    /// </summary>
    public class SystemMonitorData
    {
        /// <summary>
        /// CPU核心频率（MHz）
        /// </summary>
        public int? CpuFrequency { get; set; }

        /// <summary>
        /// GPU核心频率（MHz）
        /// </summary>
        public int? GpuFrequency { get; set; }

        /// <summary>
        /// 内存使用率（百分比）
        /// </summary>
        public float? MemoryUsagePercent { get; set; }

        /// <summary>
        /// 已使用内存（MB）
        /// </summary>
        public long? UsedMemoryMB { get; set; }

        /// <summary>
        /// 总内存（MB）
        /// </summary>
        public long? TotalMemoryMB { get; set; }

        /// <summary>
        /// CPU温度
        /// </summary>
        public int? CpuTemperature { get; set; }

        /// <summary>
        /// GPU温度
        /// </summary>
        public int? GpuTemperature { get; set; }
    }

    /// <summary>
    /// 系统监测类，使用LibreHardwareMonitor获取CPU频率、GPU频率、内存使用等信息
    /// 单例模式，避免重复创建Computer实例导致内存占用过高
    /// </summary>
    public class SystemMonitor : IDisposable
    {
        private static SystemMonitor _instance;
        private static readonly object _instanceLock = new object();
        
        private readonly LibreHardwareMonitor.Hardware.Computer _computer;
        private readonly PerformanceCounter _memoryAvailableCounter;
        private readonly PerformanceCounter _commitLimitCounter;
        private readonly Dictionary<string, int> _cachedTemperatures;
        private SystemMonitorData _lastData;
        private readonly object _lockObject = new object();
        private bool _disposed;

        /// <summary>
        /// 获取共享的SystemMonitor实例（单例模式）
        /// </summary>
        public static SystemMonitor Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_instanceLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new SystemMonitor();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 获取共享的Computer实例，供温度读取器使用
        /// </summary>
        public LibreHardwareMonitor.Hardware.Computer Computer => _computer;

        /// <summary>
        /// 私有构造函数，初始化LibreHardwareMonitor
        /// </summary>
        private SystemMonitor()
        {
            _cachedTemperatures = new Dictionary<string, int>();
            _lastData = new SystemMonitorData();

            // 初始化LibreHardwareMonitor（只创建一个实例）
            _computer = new LibreHardwareMonitor.Hardware.Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                // IsMemoryEnabled = true
            };
            _computer.Open();

            // 初始化性能计数器用于内存监测（备用方案）
            try
            {
                _memoryAvailableCounter = new PerformanceCounter("Memory", "Available MBytes");
                _commitLimitCounter = new PerformanceCounter("Memory", "Commit Limit");
            }
            catch (Exception)
            {
                // 如果性能计数器初始化失败，将使用LibreHardwareMonitor
                _memoryAvailableCounter = null;
                _commitLimitCounter = null;
            }
        }

        /// <summary>
        /// 获取所有系统监测数据
        /// </summary>
        /// <returns>SystemMonitorData对象包含所有监测数据</returns>
        public SystemMonitorData GetMonitorData()
        {
            lock (_lockObject)
            {
                var data = new SystemMonitorData();

                try
                {
                    // 遍历所有硬件并更新
                    foreach (IHardware hardware in _computer.Hardware)
                    {
                        hardware.Update();
                        ProcessHardware(hardware, data);
                    }

                    // 获取内存信息
                    GetMemoryInfo(data);

                    _lastData = data;
                }
                catch (Exception ex)
                {
                    Log.Write($"Error getting monitor data: {ex.Message}");
                    // 返回上次成功的数据
                    return _lastData ?? data;
                }

                return data;
            }
        }

        /// <summary>
        /// 处理硬件信息
        /// </summary>
        private void ProcessHardware(IHardware hardware, SystemMonitorData data)
        {
            switch (hardware.HardwareType)
            {
                case HardwareType.Cpu:
                    ProcessCpu(hardware, data);
                    break;
                case HardwareType.GpuNvidia:
                case HardwareType.GpuAmd:
                case HardwareType.GpuIntel:
                    ProcessGpu(hardware, data);
                    break;
                case HardwareType.Memory:
                    ProcessMemory(hardware, data);
                    break;
            }

            // 处理子硬件
            foreach (IHardware subHardware in hardware.SubHardware)
            {
                ProcessHardware(subHardware, data);
            }
        }

        /// <summary>
        /// 处理CPU信息
        /// </summary>
        private void ProcessCpu(IHardware hardware, SystemMonitorData data)
        {
            foreach (ISensor sensor in hardware.Sensors)
            {
                if (!sensor.Value.HasValue)
                    continue;

                // CPU温度
                if (sensor.SensorType == SensorType.Temperature)
                {
                    if (sensor.Name.Contains("Package") && !sensor.Name.Contains("Average") && !sensor.Name.Contains("Max"))
                    {
                        data.CpuTemperature = (int)Math.Round(sensor.Value.Value);
                    }
                }
                // CPU频率
                else if (sensor.SensorType == SensorType.Clock)
                {
                    // 获取核心频率，优先选择"Core #0"或"CPU Core #0"
                    if (sensor.Name.Contains("Core") && (sensor.Name.Contains("#0") || sensor.Name.Contains("1")))
                    {
                        data.CpuFrequency = (int)Math.Round(sensor.Value.Value);
                    }
                }
            }
        }

        /// <summary>
        /// 处理GPU信息
        /// </summary>
        private void ProcessGpu(IHardware hardware, SystemMonitorData data)
        {
            foreach (ISensor sensor in hardware.Sensors)
            {
                if (!sensor.Value.HasValue)
                    continue;

                // GPU温度
                if (sensor.SensorType == SensorType.Temperature)
                {
                    if (sensor.Name.Contains("GPU Core"))
                    {
                        data.GpuTemperature = (int)Math.Round(sensor.Value.Value);
                    }
                }
                // GPU频率
                else if (sensor.SensorType == SensorType.Clock)
                {
                    if (sensor.Name.Contains("GPU Core"))
                    {
                        data.GpuFrequency = (int)Math.Round(sensor.Value.Value);
                    }
                }
            }
        }

        /// <summary>
        /// 处理内存信息
        /// </summary>
        private void ProcessMemory(IHardware hardware, SystemMonitorData data)
        {
            // 移除 LibreHardwareMonitor 的内存数据处理，仅使用性能计数器
            // 此方法现在为空，因为内存数据完全由 GetMemoryInfo() 提供
        }

        /// <summary>
        /// 获取内存信息（主方案）
        /// </summary>
        private void GetMemoryInfo(SystemMonitorData data)
        {
            try
            {
                // 使用性能计数器获取可用内存
                if (_memoryAvailableCounter != null)
                {
                    long availableMB = (long)_memoryAvailableCounter.NextValue();
                    
                    // 获取总物理内存（使用 ComputerInfo 获取准确的总物理内存）
                    long totalMemoryMB = GetTotalPhysicalMemoryMB();
                    
                    if (totalMemoryMB > 0)
                    {
                        data.TotalMemoryMB = totalMemoryMB;
                        data.UsedMemoryMB = totalMemoryMB - availableMB;
                        data.MemoryUsagePercent = (float)((double)data.UsedMemoryMB / totalMemoryMB * 100);
                    }
                }
                else
                {
                    // 如果性能计数器不可用，使用系统信息作为最后备选
                    var computerInfo = new Microsoft.VisualBasic.Devices.ComputerInfo();
                    ulong totalMemoryBytes = computerInfo.TotalPhysicalMemory;
                    long totalMemoryMB = (long)(totalMemoryBytes / (1024 * 1024));
                    
                    // 由于性能计数器不可用，我们无法准确获取可用内存，因此使用总内存作为近似值
                    data.TotalMemoryMB = totalMemoryMB;
                    data.UsedMemoryMB = totalMemoryMB; // 近似值
                    data.MemoryUsagePercent = 100.0f; // 近似值
                }
            }
            catch (Exception ex)
            {
                Log.Write($"Error getting memory info: {ex.Message}");
                // 如果获取失败，使用默认值
                data.TotalMemoryMB = 0;
                data.UsedMemoryMB = 0;
                data.MemoryUsagePercent = 0.0f;
            }
        }

        /// <summary>
        /// 获取总物理内存（MB）
        /// </summary>
        private long GetTotalPhysicalMemoryMB()
        {
            try
            {
                // 使用 ComputerInfo 获取准确的总物理内存
                var computerInfo = new Microsoft.VisualBasic.Devices.ComputerInfo();
                ulong totalMemoryBytes = computerInfo.TotalPhysicalMemory;
                return (long)(totalMemoryBytes / (1024 * 1024));
            }
            catch (Exception ex)
            {
                Log.Write($"Error getting total physical memory: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            
            try
            {
                _memoryAvailableCounter?.Dispose();
                _commitLimitCounter?.Dispose();
                _computer?.Close();
            }
            catch (Exception)
            {
                // 忽略清理错误
            }
        }
    }
}
