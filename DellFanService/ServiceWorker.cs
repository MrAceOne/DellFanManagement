using DellFanManagement.DellSmbiozBzhLib;
using DellFanManagement.DellSmbiosSmiLib;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Management;
using System.Threading;
using System.Threading.Tasks;

namespace DellFanManagement.Service
{
    /// <summary>
    /// 风扇控制后台服务
    /// </summary>
    public class ServiceWorker : BackgroundService
    {
        private readonly ILogger<ServiceWorker> _logger;
        private readonly Configuration _config;

        // 风扇控制器状态
        private bool _bzhInitialized = false;
        private bool _smiAvailable = false;

        // 运行状态
        private FanMode _currentMode = FanMode.Optimized;
        private bool _ecFanControlEnabled = true;
        private int _temperatureCheckCounter = 0;
        private int _triggerCounter = 0;        // 高温持续计数器
        private int _recoveryCounter = 0;
        private bool _overrideByTemperature = false;

        // 风扇当前档位
        private int _fan1CurrentLevel = -1;
        private int _fan2CurrentLevel = -1;

        // 刷新间隔（毫秒）
        private const int RefreshInterval = 1000;

        public ServiceWorker(ILogger<ServiceWorker> logger)
        {
            _logger = logger;
            _config = new Configuration();
            _config.Load();
            _currentMode = _config.CurrentMode;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DellFanService 启动中...");

            // 初始化驱动
            if (!InitializeDriver())
            {
                _logger.LogError("无法初始化风扇控制驱动，服务退出");
                return;
            }

            _logger.LogInformation("驱动初始化成功，当前模式: {Mode}", _currentMode);

            // 应用初始模式
            ApplyMode(_currentMode);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // 重新加载配置（支持运行时修改）
                    _config.Load();

                    // 如果配置中的模式发生变化，应用新模式
                    if (_config.CurrentMode != _currentMode)
                    {
                        _logger.LogInformation("模式切换: {OldMode} -> {NewMode}", _currentMode, _config.CurrentMode);
                        _currentMode = _config.CurrentMode;
                        ApplyMode(_currentMode);
                    }

                    // 根据当前模式执行相应逻辑
                    switch (_currentMode)
                    {
                        case FanMode.Manual:
                            HandleManualMode();
                            break;
                        case FanMode.Optimized:
                        case FanMode.Cool:
                        case FanMode.Quiet:
                        case FanMode.Performance:
                            // 这些模式依赖EC自动控制，只需确保EC控制已启用
                            EnsureEcControlEnabled();
                            break;
                    }

                    await Task.Delay(RefreshInterval, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("服务正在停止...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "服务运行异常");
            }
            finally
            {
                Shutdown();
            }
        }

        /// <summary>
        /// 初始化驱动
        /// </summary>
        private bool InitializeDriver()
        {
            try
            {
                // 尝试初始化 BZH 驱动
                if (DellSmbiosBzh.Initialize())
                {
                    if (DellSmbiosBzh.GetFanRpm(BzhFanIndex.Fan1) != null)
                    {
                        _bzhInitialized = true;
                        _logger.LogInformation("BZH 驱动初始化成功");
                        return true;
                    }
                    else
                    {
                        DellSmbiosBzh.Shutdown();
                        _logger.LogWarning("BZH 驱动加载成功但无法控制风扇");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "BZH 驱动初始化失败");
            }

            // 回退到 SMI
            try
            {
                _smiAvailable = DellSmbiosSmi.IsFanControlOverrideAvailable();
                if (_smiAvailable)
                {
                    _logger.LogInformation("SMI 接口可用");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SMI 接口检查失败");
            }

            return false;
        }

        /// <summary>
        /// 应用指定模式
        /// </summary>
        private void ApplyMode(FanMode mode)
        {
            switch (mode)
            {
                case FanMode.Manual:
                    DisableEcFanControl();
                    _temperatureCheckCounter = 0;
                    _recoveryCounter = 0;
                    _overrideByTemperature = false;
                    _fan1CurrentLevel = -1;
                    _fan2CurrentLevel = -1;
                    break;

                case FanMode.Optimized:
                    EnableEcFanControl();
                    SetThermalSetting(ThermalSetting.Optimized);
                    break;

                case FanMode.Cool:
                    EnableEcFanControl();
                    SetThermalSetting(ThermalSetting.Cool);
                    break;

                case FanMode.Quiet:
                    EnableEcFanControl();
                    SetThermalSetting(ThermalSetting.Quiet);
                    break;

                case FanMode.Performance:
                    EnableEcFanControl();
                    SetThermalSetting(ThermalSetting.Performance);
                    break;
            }
        }

        /// <summary>
        /// 处理手动模式
        /// </summary>
        private void HandleManualMode()
        {
            if (_ecFanControlEnabled)
            {
                DisableEcFanControl();
            }

            if (!_bzhInitialized)
            {
                _logger.LogWarning("手动模式需要 BZH 驱动，但驱动未初始化");
                return;
            }

            int cpuTemp = GetCpuTemperature();
            int gpuTemp = GetGpuTemperature();

            // 高温保护：禁用睿频（需持续高温达到设定时间才触发）
            if (_config.EnableTurboBoostProtection)
            {
                bool cpuHot = cpuTemp >= _config.TriggerCpuTemp;
                bool gpuHot = gpuTemp >= _config.TriggerGpuTemp;

                if (cpuHot || gpuHot)
                {
                    if (!_overrideByTemperature)
                    {
                        _triggerCounter++;
                        if (_triggerCounter >= _config.TriggerDurationSeconds)
                        {
                            _logger.LogWarning("高温持续 {Duration} 秒: CPU={CpuTemp}°C, GPU={GpuTemp}°C，禁用睿频",
                                _config.TriggerDurationSeconds, cpuTemp, gpuTemp);
                            SetTurboBoost(false);
                            _overrideByTemperature = true;
                            _triggerCounter = 0;
                            _recoveryCounter = 0;
                        }
                    }
                }
                else
                {
                    // 温度降低，重置触发计数器
                    if (_triggerCounter > 0)
                    {
                        _triggerCounter = 0;
                        _logger.LogDebug("温度降低，重置高温触发计数器");
                    }

                    // 恢复睿频逻辑
                    if (_overrideByTemperature)
                    {
                        bool cpuCooled = cpuTemp < 0 || cpuTemp < _config.RecoveryCpuTemp;
                        bool gpuCooled = gpuTemp < 0 || gpuTemp < _config.RecoveryGpuTemp;

                        if (cpuCooled && gpuCooled)
                        {
                            _recoveryCounter++;
                            if (_recoveryCounter >= _config.RecoveryDurationSeconds)
                            {
                                _logger.LogInformation("温度持续低于恢复阈值 {Duration} 秒，恢复睿频", _config.RecoveryDurationSeconds);
                                SetTurboBoost(true);
                                _overrideByTemperature = false;
                                _recoveryCounter = 0;
                                _temperatureCheckCounter = 0;
                            }
                        }
                        else
                        {
                            _recoveryCounter = 0;
                        }
                    }
                }
            }

            // 温度控制风扇
            if (_config.EnableTemperatureControl && !_overrideByTemperature)
            {
                _temperatureCheckCounter++;
                if (_temperatureCheckCounter >= _config.CheckIntervalSeconds)
                {
                    _temperatureCheckCounter = 0;
                    ApplyTemperatureBasedFanControl(cpuTemp, gpuTemp);
                }
            }
            else if (!_config.EnableTemperatureControl)
            {
                // 固定档位模式
                SetFanLevel(1, _config.Fan1ManualLevel);
                SetFanLevel(2, _config.Fan2ManualLevel);
            }
        }

        /// <summary>
        /// 基于温度控制风扇
        /// </summary>
        private void ApplyTemperatureBasedFanControl(int cpuTemp, int gpuTemp)
        {
            // CPU -> Fan1
            if (cpuTemp >= 0)
            {
                int targetLevel = cpuTemp >= _config.CpuTemperatureThreshold ? 1 : 0;
                SetFanLevel(1, targetLevel);
            }

            // GPU -> Fan2
            if (gpuTemp >= 0)
            {
                int targetLevel = gpuTemp >= _config.GpuTemperatureThreshold ? 1 : 0;
                SetFanLevel(2, targetLevel);
            }
        }

        /// <summary>
        /// 设置风扇档位
        /// </summary>
        private void SetFanLevel(int fanNumber, int level)
        {
            if (fanNumber == 1 && _fan1CurrentLevel == level) return;
            if (fanNumber == 2 && _fan2CurrentLevel == level) return;

            BzhFanIndex fanIndex = fanNumber == 1 ? BzhFanIndex.Fan1 : BzhFanIndex.Fan2;
            BzhFanLevel fanLevel = level switch
            {
                0 => BzhFanLevel.Level0,
                1 => BzhFanLevel.Level1,
                2 => BzhFanLevel.Level2,
                _ => BzhFanLevel.Level1
            };

            if (_bzhInitialized)
            {
                bool result = DellSmbiosBzh.SetFanLevel(fanIndex, fanLevel);
                if (result)
                {
                    if (fanNumber == 1) _fan1CurrentLevel = level;
                    else _fan2CurrentLevel = level;
                    _logger.LogDebug("风扇{Fan} 设置为档位 {Level}", fanNumber, level);
                }
                else
                {
                    _logger.LogWarning("设置风扇{Fan}档位失败", fanNumber);
                }
            }
        }

        /// <summary>
        /// 禁用EC风扇自动控制
        /// </summary>
        private void DisableEcFanControl()
        {
            if (!_ecFanControlEnabled) return;

            bool result = false;
            if (_bzhInitialized)
            {
                result = DellSmbiosBzh.DisableAutomaticFanControl();
            }
            else if (_smiAvailable)
            {
                result = DellSmbiosSmi.DisableAutomaticFanControl();
            }

            if (result)
            {
                _ecFanControlEnabled = false;
                _logger.LogInformation("已禁用EC风扇自动控制");
            }
            else
            {
                _logger.LogWarning("禁用EC风扇自动控制失败");
            }
        }

        /// <summary>
        /// 启用EC风扇自动控制
        /// </summary>
        private void EnableEcFanControl()
        {
            if (_ecFanControlEnabled) return;

            bool result = false;
            if (_bzhInitialized)
            {
                result = DellSmbiosBzh.EnableAutomaticFanControl();
            }
            else if (_smiAvailable)
            {
                result = DellSmbiosSmi.EnableAutomaticFanControl();
            }

            if (result)
            {
                _ecFanControlEnabled = true;
                _fan1CurrentLevel = -1;
                _fan2CurrentLevel = -1;
                _logger.LogInformation("已启用EC风扇自动控制");
            }
            else
            {
                _logger.LogWarning("启用EC风扇自动控制失败");
            }
        }

        /// <summary>
        /// 确保EC控制已启用
        /// </summary>
        private void EnsureEcControlEnabled()
        {
            if (!_ecFanControlEnabled)
            {
                EnableEcFanControl();
            }
        }

        /// <summary>
        /// 设置散热模式
        /// </summary>
        private void SetThermalSetting(ThermalSetting setting)
        {
            try
            {
                if (DellSmbiosSmi.SetThermalSetting(setting))
                {
                    _logger.LogInformation("散热模式已设置为: {Setting}", setting);
                }
                else
                {
                    _logger.LogWarning("设置散热模式失败: {Setting}", setting);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置散热模式异常");
            }
        }

        /// <summary>
        /// 设置睿频状态
        /// </summary>
        private void SetTurboBoost(bool enabled)
        {
            try
            {
                // 使用 powercfg 命令设置处理器最大状态来间接控制睿频
                // 100% = 启用睿频, 99% = 禁用睿频
                int value = enabled ? 100 : 99;
                string scheme = GetActivePowerScheme();
                if (!string.IsNullOrEmpty(scheme))
                {
                    string args = $"/setacvalueindex {scheme} SUB_PROCESSOR PROCTHROTTLEMAX {value}";
                    ExecutePowerCfg(args);
                    args = $"/setdcvalueindex {scheme} SUB_PROCESSOR PROCTHROTTLEMAX {value}";
                    ExecutePowerCfg(args);
                    ExecutePowerCfg("/setactive " + scheme);
                    _logger.LogInformation("睿频已{State}", enabled ? "启用" : "禁用");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置睿频状态失败");
            }
        }

        /// <summary>
        /// 获取当前电源方案GUID
        /// </summary>
        private string GetActivePowerScheme()
        {
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "powercfg",
                        Arguments = "/getactivescheme",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                // 解析输出: "电源方案 GUID: 381b4222-f694-41f0-9685-ff5bb260df2e  (平衡)"
                int start = output.IndexOf("GUID: ");
                if (start >= 0)
                {
                    start += 6;
                    int end = output.IndexOf(" ", start);
                    if (end > start)
                    {
                        return output.Substring(start, end - start).Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取电源方案失败");
            }
            return string.Empty;
        }

        /// <summary>
        /// 执行 powercfg 命令
        /// </summary>
        private void ExecutePowerCfg(string arguments)
        {
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "powercfg",
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行 powercfg 失败: {Args}", arguments);
            }
        }

        /// <summary>
        /// 获取CPU温度
        /// </summary>
        private int GetCpuTemperature()
        {
            try
            {
                using ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");
                foreach (ManagementObject obj in searcher.Get())
                {
                    uint tempK = (uint)obj["CurrentTemperature"];
                    return (int)(tempK / 10.0 - 273.15);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "获取CPU温度失败");
            }
            return -1;
        }

        /// <summary>
        /// 获取GPU温度
        /// </summary>
        private int GetGpuTemperature()
        {
            try
            {
                using ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
                foreach (ManagementObject obj in searcher.Get())
                {
                    // 尝试获取温度信息
                    // 注意：Win32_VideoController 不直接提供温度，这里简化处理
                    // 实际项目中可能需要使用 NVAPI 或其他方式
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "获取GPU温度失败");
            }
            return -1;
        }

        /// <summary>
        /// 关闭驱动
        /// </summary>
        private void Shutdown()
        {
            _logger.LogInformation("正在清理资源...");

            // 恢复睿频
            SetTurboBoost(true);

            // 恢复EC自动控制
            EnableEcFanControl();

            // 关闭BZH驱动
            if (_bzhInitialized)
            {
                DellSmbiosBzh.Shutdown();
                _bzhInitialized = false;
            }

            _logger.LogInformation("服务已停止");
        }
    }
}
