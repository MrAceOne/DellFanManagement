using DellFanManagement.App.FanControllers;
using DellFanManagement.DellSmbiosSmiLib;
using System;
using System.Threading;
using System.Windows.Forms;

namespace DellFanManagement.App
{
    public class Core
    {
        /// <summary>
        /// How often to refresh the system state, in milliseconds.
        /// </summary>
        private static readonly int RefreshInterval = 1000;

        /// <summary>
        /// Shared object which contains the state of the application.
        /// </summary>
        private readonly State _state;

        /// <summary>
        /// Form object running the application.
        /// </summary>
        private readonly DellFanManagementGuiForm _form;

        /// <summary>
        /// Fan controller for making fan speed adjustments.
        /// </summary>
        private readonly FanController _fanController;

        /// <summary>
        /// Block state change requests while a state update is in progress.
        /// </summary>
        private readonly Semaphore _requestSemaphore;

        /// <summary>
        /// User-selected fan control mode (decoupled from actual EC state).
        /// </summary>
        private FanMode _fanMode;

        /// <summary>
        /// User requested level for fan 1.
        /// </summary>
        private FanLevel? _fan1LevelRequested;

        /// <summary>
        /// User requested level for fan 2.
        /// </summary>
        private FanLevel? _fan2LevelRequested;

        /// <summary>
        /// CPU temperature threshold for manual mode fan control (default 45 degrees).
        /// </summary>
        public int CpuTemperatureThreshold { get; private set; } = 45;

        /// <summary>
        /// GPU temperature threshold for manual mode fan control (default 45 degrees).
        /// </summary>
        public int GpuTemperatureThreshold { get; private set; } = 45;

        /// <summary>
        /// Temperature check interval in seconds for manual mode (default 30 seconds).
        /// </summary>
        public int CheckIntervalSeconds { get; private set; } = 30;

        /// <summary>
        /// Counter for temperature check interval.
        /// </summary>
        private int _temperatureCheckCounter = 0;

        // ========== 高温禁用睿频触发/恢复配置（统一可配置） ==========

        /// <summary>
        /// 手动模式下触发禁用睿频的CPU温度阈值（默认90度）
        /// </summary>
        public int TriggerCpuTemp { get; private set; } = 90;

        /// <summary>
        /// 手动模式下触发禁用睿频的GPU温度阈值（默认80度）
        /// </summary>
        public int TriggerGpuTemp { get; private set; } = 80;

        /// <summary>
        /// 恢复睿频的CPU温度上限（默认80度）
        /// </summary>
        public int RecoveryCpuTemp { get; private set; } = 80;

        /// <summary>
        /// 恢复睿频的GPU温度上限（默认70度）
        /// </summary>
        public int RecoveryGpuTemp { get; private set; } = 70;

        /// <summary>
        /// 恢复睿频前需要持续满足低温条件的秒数（默认20秒）
        /// </summary>
        public int RecoveryDurationSeconds { get; private set; } = 20;

        /// <summary>
        /// 持续低温计数器（用于恢复手动模式）
        /// </summary>
        private int _recoveryCounter = 0;

        public TrayIconColor TrayIconColor { get; set; }

        /// <summary>
        /// Thermal setting that has been requested by the user but not yet applied.
        /// </summary>
        public ThermalSetting? RequestedThermalSetting { get; private set; }

        /// <summary>
        /// 上次CPU温度（用于智能UI更新）
        /// </summary>
        private int _lastCpuTemperature = -1;

        /// <summary>
        /// 上次GPU温度（用于智能UI更新）
        /// </summary>
        private int _lastGpuTemperature = -1;

        /// <summary>
        /// 上次风扇1转速（用于智能UI更新）
        /// </summary>
        private uint? _lastFan1Rpm = null;

        /// <summary>
        /// 上次风扇2转速（用于智能UI更新）
        /// </summary>
        private uint? _lastFan2Rpm = null;

        /// <summary>
        /// 强制更新计数器（每5秒强制更新一次）
        /// </summary>
        private int _forceUpdateCounter = 0;

        /// <summary>
        /// 强制更新间隔（秒）
        /// </summary>
        private const int ForceUpdateInterval = 5;

        /// <summary>
        /// Configuration store for reading settings.
        /// </summary>
        private readonly ConfigurationStore _configurationStore;

        /// <summary>
        /// 是否因为高温而禁用了睿频
        /// </summary>
        private bool _overrideByTemperature;

        /// <summary>
        /// 是否启用高温睿频保护（默认true）
        /// </summary>
        private bool _turboBoostEnabled = true;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="state">Shared state object</param>
        /// <param name="form">Form object (hosting the application)</param>
        public Core(State state, DellFanManagementGuiForm form)
        {
            _state = state;
            _form = form;
            _fanController = FanControllerFactory.GetFanFanController();
            _requestSemaphore = new(1, 1);
            _configurationStore = new ConfigurationStore();

            RequestedThermalSetting = null;
            _fan1LevelRequested = null;
            _fan2LevelRequested = null;

            _overrideByTemperature = false;

            // 加载睿频保护设置（默认启用）
            int? turboBoostSaved = _configurationStore.GetIntOption(ConfigurationOption.TurboBoostEnabled);
            _turboBoostEnabled = turboBoostSaved != 0;

            TrayIconColor = TrayIconColor.Gray;

            // Load configuration values (including saved fan mode).
            LoadConfiguration();

            // 同步风扇模式到 State，确保启动时 UI 状态一致
            _state.WaitOne();
            _state.FanMode = _fanMode;
            _state.Release();
        }

        /// <summary>
        /// Load configuration values from registry.
        /// </summary>
        private void LoadConfiguration()
        {
            int? cpuThreshold = _configurationStore.GetIntOption(ConfigurationOption.ManualModeCpuTemperatureThreshold);
            if (cpuThreshold.HasValue && cpuThreshold.Value > 0)
            {
                CpuTemperatureThreshold = cpuThreshold.Value;
            }

            int? gpuThreshold = _configurationStore.GetIntOption(ConfigurationOption.ManualModeGpuTemperatureThreshold);
            if (gpuThreshold.HasValue && gpuThreshold.Value > 0)
            {
                GpuTemperatureThreshold = gpuThreshold.Value;
            }

            int? checkInterval = _configurationStore.GetIntOption(ConfigurationOption.ManualModeCheckIntervalSeconds);
            if (checkInterval.HasValue && checkInterval.Value > 0)
            {
                CheckIntervalSeconds = checkInterval.Value;
            }

            // 加载上次保存的风扇控制模式
            int? savedFanMode = _configurationStore.GetIntOption(ConfigurationOption.FanControlMode);
            if (savedFanMode.HasValue && Enum.IsDefined(typeof(FanMode), savedFanMode.Value))
            {
                _fanMode = (FanMode)savedFanMode.Value;
                Log.Write($"Loaded saved fan mode from registry: {_fanMode} (value={savedFanMode.Value})");
            }
            else
            {
                _fanMode = FanMode.Automatic;
                Log.Write($"No saved fan mode found in registry, defaulting to Automatic (saved={savedFanMode})");
            }
        }

        /// <summary>
        /// Request that fan control mode be changed.
        /// </summary>
        /// <param name="fanMode">Fan mode to set</param>
        public void RequestFanMode(FanMode fanMode)
        {
            _requestSemaphore.WaitOne();
            _fanMode = fanMode;
            _configurationStore.SetOption(ConfigurationOption.FanControlMode, (int)fanMode);
            _requestSemaphore.Release();
        }

        /// <summary>
        /// Requested a specific fan level for level 1.
        /// </summary>
        /// <param name="level">Fan level to set</param>
        public void RequestFan1Level(FanLevel? level)
        {
            _requestSemaphore.WaitOne();
            _fan1LevelRequested = level;
            _requestSemaphore.Release();
        }

        /// <summary>
        /// Requested a specific fan level for level 2.
        /// </summary>
        /// <param name="level">Fan level to set</param>
        public void RequestFan2Level(FanLevel? level)
        {
            _requestSemaphore.WaitOne();
            _fan2LevelRequested = level;
            _requestSemaphore.Release();
        }

        /// <summary>
        /// Request that the thermal setting be updated.
        /// </summary>
        /// <param name="requestedThermalSetting">Requested thermal setting</param>
        public void RequestThermalSetting(ThermalSetting requestedThermalSetting)
        {
            _requestSemaphore.WaitOne();

            if (requestedThermalSetting != _state.ThermalSetting)
            {
                RequestedThermalSetting = requestedThermalSetting;
            }

            _requestSemaphore.Release();
        }

        /// <summary>
        /// 设置是否启用高温睿频保护
        /// </summary>
        /// <param name="enabled">true 表示启用，false 表示禁用</param>
        public void SetTurboBoostEnabled(bool enabled)
        {
            _requestSemaphore.WaitOne();
            _turboBoostEnabled = enabled;

            // 如果禁用睿频保护且当前因为高温已禁用睿频，则恢复睿频
            if (!enabled && _overrideByTemperature)
            {
                CpuPowerManager.SetGuid(CpuPowerManager.GUID_PROCESSOR_TURBOBOOST, 2);
                _overrideByTemperature = false;
                _recoveryCounter = 0;
                Log.Write("Turbo boost protection disabled, restored turbo boost");
            }

            _requestSemaphore.Release();
        }

        /// <summary>
        /// Start up the application "background thread", which monitors the system state.
        /// </summary>
        public void StartBackgroundThread()
        {
            new Thread(new ThreadStart(BackgroundThread)).Start();
        }

        /// <summary>
        /// The background thread runs in a loop.  It collects RPM and temperature data and handles the program's main
        /// background behavior.
        /// </summary>
        private void BackgroundThread()
        {
            _state.WaitOne();
            _state.BackgroundThreadRunning = true;
            _state.Release();

            bool releaseSemaphore = false;

            try
            {
                // 启动时：根据上次保存的模式恢复状态
                if (_fanMode == FanMode.Automatic)
                {
                    if (IsAutomaticFanControlDisableSupported)
                    {
                        _fanController.EnableAutomaticFanControl();
                    }
                    _state.WaitOne();
                    _state.EcFanControlEnabled = true;
                    _state.FanMode = FanMode.Automatic;
                    _state.Release();
                    Log.Write("Started in automatic mode – enabled EC fan control");
                }
                else
                {
                    if (IsAutomaticFanControlDisableSupported)
                    {
                        _fanController.DisableAutomaticFanControl();
                    }
                    _state.WaitOne();
                    _state.EcFanControlEnabled = false;
                    _state.FanMode = FanMode.Manual;

                    // 手动模式启动后立即应用一次温度控制
                    if (IsAutomaticFanControlDisableSupported && IsSpecificFanControlSupported)
                    {
                        _temperatureCheckCounter = 0;
                        ApplyTemperatureBasedFanControl();
                    }

                    _state.Release();
                    Log.Write("Started in manual mode – disabled EC fan control");
                }
                // 强制同步UI，确保启动后立即显示正确模式
                UpdateForm();

                while (_state.BackgroundThreadRunning)
                {
                    _state.WaitOne();
                    _requestSemaphore.WaitOne();
                    releaseSemaphore = true;

                    bool windowVisible = _state.WindowVisible;
                    int cpuTemp;
                    int gpuTemp;

                    // 窗口隐藏到托盘时的处理逻辑
                    if (!windowVisible)
                    {
                        if (_fanMode == FanMode.Automatic)
                        {
                            // 自动模式：窗口隐藏时直接暂停，不做任何处理
                            _requestSemaphore.Release();
                            _state.Release();
                            releaseSemaphore = false;
                            Thread.Sleep(Core.RefreshInterval);
                            continue;
                        }
                        else
                        {
                            // 手动模式：窗口隐藏时只保留温度控制，跳过其他逻辑和UI更新
                            cpuTemp = GetCpuTemperature();
                            gpuTemp = GetGpuTemperature();

                            // 仅应用基于温度的风扇控制（高温保护）
                            if (!_state.EcFanControlEnabled && IsAutomaticFanControlDisableSupported && IsSpecificFanControlSupported)
                            {
                                bool cpuHot = cpuTemp >= TriggerCpuTemp;
                                bool gpuHot = gpuTemp >= TriggerGpuTemp;
                                if (_turboBoostEnabled && (cpuHot || gpuHot))
                                {
                                    Log.Write($"High temperature detected (CPU: {cpuTemp}°C, GPU: {gpuTemp}°C), disabling turbo boost");
                                    CpuPowerManager.SetGuid(CpuPowerManager.GUID_PROCESSOR_TURBOBOOST, 0);
                                    _overrideByTemperature = true;
                                    _recoveryCounter = 0;
                                }
                                else if (_overrideByTemperature)
                                {
                                    bool cpuCooled = cpuTemp < 0 || cpuTemp < RecoveryCpuTemp;
                                    bool gpuCooled = gpuTemp < 0 || gpuTemp < RecoveryGpuTemp;
                                    if (cpuCooled && gpuCooled)
                                    {
                                        _recoveryCounter++;
                                        if (_recoveryCounter >= RecoveryDurationSeconds)
                                        {
                                            CpuPowerManager.SetGuid(CpuPowerManager.GUID_PROCESSOR_TURBOBOOST, 2);
                                            _overrideByTemperature = false;
                                            _recoveryCounter = 0;
                                            _state.Fan1Level = null;
                                            _state.Fan2Level = null;
                                            ApplyTemperatureBasedFanControl();
                                            Log.Write($"Temperature stayed below recovery thresholds for {RecoveryDurationSeconds}s, restored turbo boost");
                                        }
                                    }
                                    else
                                    {
                                        if (_recoveryCounter > 0)
                                        {
                                            _recoveryCounter = 0;
                                        }
                                    }
                                }
                                else
                                {
                                    _temperatureCheckCounter++;
                                    if (_temperatureCheckCounter >= CheckIntervalSeconds)
                                    {
                                        _temperatureCheckCounter = 0;
                                        ApplyTemperatureBasedFanControl();
                                    }
                                }
                            }

                            _requestSemaphore.Release();
                            _state.Release();
                            releaseSemaphore = false;
                            Thread.Sleep(Core.RefreshInterval);
                            continue;
                        }
                    }

                    // Update state.
                    _state.Update();

                    // 获取当前温度
                    cpuTemp = GetCpuTemperature();
                    gpuTemp = GetGpuTemperature();

                    // Handle turbo boost state changes.
                    if (_overrideByTemperature)
                    {
                        // 当前因为高温而禁用了睿频
                        if (!_turboBoostEnabled)
                        {
                            // 睿频保护已禁用，恢复睿频
                            CpuPowerManager.SetGuid(CpuPowerManager.GUID_PROCESSOR_TURBOBOOST, 2);
                            _overrideByTemperature = false;
                            _recoveryCounter = 0;
                        }
                        else if (_fanMode == FanMode.Manual)
                        {
                            // 用户还是手动模式，检查温度是否降低
                            bool cpuCooled = cpuTemp < 0 || cpuTemp < RecoveryCpuTemp;
                            bool gpuCooled = gpuTemp < 0 || gpuTemp < RecoveryGpuTemp;
                            if (cpuCooled && gpuCooled)
                            {
                                _recoveryCounter++;
                                if (_recoveryCounter >= RecoveryDurationSeconds)
                                {
                                    // 持续低温达到设定时间，恢复睿频
                                    CpuPowerManager.SetGuid(CpuPowerManager.GUID_PROCESSOR_TURBOBOOST, 2);
                                    _overrideByTemperature = false;
                                    _recoveryCounter = 0;
                                    _temperatureCheckCounter = 0;

                                    // 重置风扇级别状态，强制重新应用温度控制
                                    _state.Fan1Level = null;
                                    _state.Fan2Level = null;

                                    ApplyTemperatureBasedFanControl();
                                    Log.Write($"Temperature stayed below recovery thresholds for {RecoveryDurationSeconds}s, restored turbo boost");
                                }
                            }
                            else
                            {
                                // 温度又升高了，重置计数器
                                if (_recoveryCounter > 0)
                                {
                                    _recoveryCounter = 0;
                                    Log.Write("Temperature rose again, resetting recovery counter");
                                }
                            }
                        }
                        else
                        {
                            // 用户切换到了自动模式，清除覆盖标记并恢复睿频
                            CpuPowerManager.SetGuid(CpuPowerManager.GUID_PROCESSOR_TURBOBOOST, 2);
                            _overrideByTemperature = false;
                            _recoveryCounter = 0;
                            _state.FanMode = FanMode.Automatic;
                        }
                    }
                    else
                    {
                        // 正常处理用户请求的EC模式
                        if (_fanMode == FanMode.Automatic && !_state.EcFanControlEnabled)
                        {
                            _state.EcFanControlEnabled = true;
                            _state.FanMode = FanMode.Automatic;
                            _fanController.EnableAutomaticFanControl();
                            Log.Write("Enabled EC fan control – automatic mode");

                            _state.Fan1Level = null;
                            _state.Fan2Level = null;
                            _fan1LevelRequested = null;
                            _fan2LevelRequested = null;
                        }
                        else if (_fanMode == FanMode.Manual && _state.EcFanControlEnabled)
                        {
                            _state.EcFanControlEnabled = false;
                            _state.FanMode = FanMode.Manual;
                            _fanController.DisableAutomaticFanControl();
                            Log.Write("Disabled EC fan control – manual mode");

                            // Immediately apply temperature-based fan control when switching to manual mode.
                            if (IsAutomaticFanControlDisableSupported && IsSpecificFanControlSupported)
                            {
                                _temperatureCheckCounter = 0;
                                ApplyTemperatureBasedFanControl();
                            }
                        }
                    }

                    // In manual mode (EC fan control disabled), apply temperature-based fan control.
                    if (!_state.EcFanControlEnabled && IsAutomaticFanControlDisableSupported && IsSpecificFanControlSupported)
                    {
                        // 检查是否需要因为高温而禁用睿频
                        bool cpuHot = cpuTemp >= TriggerCpuTemp;
                        bool gpuHot = gpuTemp >= TriggerGpuTemp;
                        if (_turboBoostEnabled && (cpuHot || gpuHot))
                        {
                            Log.Write($"High temperature detected (CPU: {cpuTemp}°C, GPU: {gpuTemp}°C), disabling turbo boost");
                            CpuPowerManager.SetGuid(CpuPowerManager.GUID_PROCESSOR_TURBOBOOST, 0);
                            _overrideByTemperature = true;
                            _recoveryCounter = 0;
                        }
                        else
                        {
                            // 原有逻辑：基于配置阈值的温度控制
                            _temperatureCheckCounter++;
                            bool shouldCheckTemperature = _temperatureCheckCounter >= CheckIntervalSeconds;

                            if (shouldCheckTemperature)
                            {
                                _temperatureCheckCounter = 0;
                                ApplyTemperatureBasedFanControl();
                            }
                        }
                    }
                    else
                    {
                        _temperatureCheckCounter = 0;
                    }

                    // Apply user requested fan levels (if any).
                    if (!_state.EcFanControlEnabled)
                    {
                        if (_fan1LevelRequested != null && _state.Fan1Level != _fan1LevelRequested)
                        {
                            _state.Fan1Level = _fan1LevelRequested;
                            _fanController.SetFanLevel((FanLevel)_fan1LevelRequested, IsIndividualFanControlSupported ? FanIndex.Fan1 : FanIndex.AllFans);
                        }
                        // Clear the one-shot request so temperature-based control can resume.
                        _fan1LevelRequested = null;

                        if (_state.Fan2Present && IsIndividualFanControlSupported && _fan2LevelRequested != null && _state.Fan2Level != _fan2LevelRequested)
                        {
                            _state.Fan2Level = _fan2LevelRequested;
                            _fanController.SetFanLevel((FanLevel)_fan2LevelRequested, FanIndex.Fan2);
                        }
                        // Clear the one-shot request so temperature-based control can resume.
                        _fan2LevelRequested = null;
                    }

                    // Apply user requested thermal setting (only in automatic mode).
                    if (_state.EcFanControlEnabled && RequestedThermalSetting != null && RequestedThermalSetting != _state.ThermalSetting)
                    {
                        if (DellSmbiosSmi.SetThermalSetting(RequestedThermalSetting.Value))
                        {
                            Log.Write($"Thermal setting applied: {RequestedThermalSetting.Value}");
                            _state.SetThermalSetting(RequestedThermalSetting.Value);
                            RequestedThermalSetting = null;
                        }
                        else
                        {
                            Log.Write($"Failed to apply thermal setting: {RequestedThermalSetting.Value}");
                        }
                    }

                    _requestSemaphore.Release();
                    _state.Release();
                    releaseSemaphore = false;

                    // Smart UI update.
                    _forceUpdateCounter++;
                    bool forceUpdate = _forceUpdateCounter >= ForceUpdateInterval;
                    bool dataChanged = HasDataChanged();

                    if (dataChanged || forceUpdate)
                    {
                        UpdateForm();
                        _forceUpdateCounter = 0;
                    }

                    Thread.Sleep(Core.RefreshInterval);
                }

                // If we got out of the loop without error, the program is terminating.
                // 退出时：强制开启EC自动控制，恢复睿频
                if (IsAutomaticFanControlDisableSupported)
                {
                    _fanController.EnableAutomaticFanControl();
                    Log.Write("Enabled EC fan control – shutdown");
                }
                CpuPowerManager.SetGuid(CpuPowerManager.GUID_PROCESSOR_TURBOBOOST, 2);
                Log.Write("Restored turbo boost – shutdown");

                // Clean up as the program terminates.
                _fanController.Shutdown();
            }
            catch (Exception exception)
            {
                if (releaseSemaphore)
                {
                    _state.Release();
                }

                _state.WaitOne();
                _state.Error = string.Format("{0}: {1}\n{2}", exception.GetType().ToString(), exception.Message, exception.StackTrace);
                _state.Release();

                Log.Write(_state.Error);
            }

            _state.WaitOne();
            _state.BackgroundThreadRunning = false;
            _state.Release();
        }

        /// <summary>
        /// Apply temperature-based fan control in manual mode.
        /// CPU: above threshold -> Fan1 Medium, below threshold -> Fan1 Off.
        /// GPU: above threshold -> Fan2 Medium, below threshold -> Fan2 Off.
        /// </summary>
        private void ApplyTemperatureBasedFanControl()
        {
            int cpuTemp = GetCpuTemperature();
            int gpuTemp = GetGpuTemperature();

            // CPU temperature control for Fan 1.
            if (cpuTemp >= 0)
            {
                if (cpuTemp >= CpuTemperatureThreshold)
                {
                    if (_state.Fan1Level != FanLevel.Medium)
                    {
                        if (_fanController.SetFanLevel(FanLevel.Medium, IsIndividualFanControlSupported ? FanIndex.Fan1 : FanIndex.AllFans))
                        {
                            _state.Fan1Level = FanLevel.Medium;
                            Log.Write($"Manual mode: CPU temp {cpuTemp}°C >= {CpuTemperatureThreshold}°C, Fan 1 set to Medium");
                        }
                        else
                        {
                            Log.Write($"Manual mode: Failed to set Fan 1 to Medium");
                        }
                    }
                }
                else
                {
                    if (_state.Fan1Level != FanLevel.Off)
                    {
                        if (_fanController.SetFanLevel(FanLevel.Off, IsIndividualFanControlSupported ? FanIndex.Fan1 : FanIndex.AllFans))
                        {
                            _state.Fan1Level = FanLevel.Off;
                            Log.Write($"Manual mode: CPU temp {cpuTemp}°C < {CpuTemperatureThreshold}°C, Fan 1 set to Off");
                        }
                        else
                        {
                            Log.Write($"Manual mode: Failed to set Fan 1 to Off");
                        }
                    }
                }
            }

            // GPU temperature control for Fan 2.
            if (_state.Fan2Present && gpuTemp >= 0)
            {
                if (gpuTemp >= GpuTemperatureThreshold)
                {
                    if (_state.Fan2Level != FanLevel.Medium)
                    {
                        bool result = true;
                        if (IsIndividualFanControlSupported)
                        {
                            result = _fanController.SetFanLevel(FanLevel.Medium, FanIndex.Fan2);
                        }
                        if (result)
                        {
                            _state.Fan2Level = FanLevel.Medium;
                            Log.Write($"Manual mode: GPU temp {gpuTemp}°C >= {GpuTemperatureThreshold}°C, Fan 2 set to Medium");
                        }
                        else
                        {
                            Log.Write($"Manual mode: Failed to set Fan 2 to Medium");
                        }
                    }
                }
                else
                {
                    if (_state.Fan2Level != FanLevel.Off)
                    {
                        bool result = true;
                        if (IsIndividualFanControlSupported)
                        {
                            result = _fanController.SetFanLevel(FanLevel.Off, FanIndex.Fan2);
                        }
                        if (result)
                        {
                            _state.Fan2Level = FanLevel.Off;
                            Log.Write($"Manual mode: GPU temp {gpuTemp}°C < {GpuTemperatureThreshold}°C, Fan 2 set to Off");
                        }
                        else
                        {
                            Log.Write($"Manual mode: Failed to set Fan 2 to Off");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Request that the GUI form update using current values from the state.
        /// </summary>
        private void UpdateForm()
        {
            if (_state.FormClosed || _form.IsDisposed)
            {
                return;
            }

            MethodInvoker updateInvoker = new(_form.UpdateForm);

            try
            {
                _form.BeginInvoke(updateInvoker);
            }
            catch (Exception)
            {
                // Take no action.
            }
        }

        /// <summary>
        /// 检查数据是否发生变化，用于智能UI更新
        /// </summary>
        /// <returns>如果数据发生变化返回true，否则返回false</returns>
        private bool HasDataChanged()
        {
            bool changed = false;

            int currentCpuTemp = GetCpuTemperature();
            int currentGpuTemp = GetGpuTemperature();

            if (currentCpuTemp >= 0 && currentCpuTemp != _lastCpuTemperature)
            {
                changed = true;
                _lastCpuTemperature = currentCpuTemp;
            }

            if (currentGpuTemp >= 0 && currentGpuTemp != _lastGpuTemperature)
            {
                changed = true;
                _lastGpuTemperature = currentGpuTemp;
            }

            if (_state.Fan1Rpm != _lastFan1Rpm)
            {
                changed = true;
                _lastFan1Rpm = _state.Fan1Rpm;
            }

            if (_state.Fan2Rpm != _lastFan2Rpm)
            {
                changed = true;
                _lastFan2Rpm = _state.Fan2Rpm;
            }

            return changed;
        }

        /// <summary>
        /// 获取CPU温度
        /// </summary>
        /// <returns>CPU温度，如果无法获取则返回-1</returns>
        private int GetCpuTemperature()
        {
            try
            {
                if (_state.Temperatures.ContainsKey(TemperatureComponent.CPU))
                {
                    var cpuTemps = _state.Temperatures[TemperatureComponent.CPU];
                    if (cpuTemps.ContainsKey("CPU"))
                    {
                        return cpuTemps["CPU"];
                    }
                }
            }
            catch (Exception)
            {
            }
            return -1;
        }

        /// <summary>
        /// 获取GPU温度
        /// </summary>
        /// <returns>GPU温度，如果无法获取则返回-1</returns>
        private int GetGpuTemperature()
        {
            try
            {
                if (_state.Temperatures.ContainsKey(TemperatureComponent.GPU))
                {
                    var gpuTemps = _state.Temperatures[TemperatureComponent.GPU];
                    foreach (var temp in gpuTemps.Values)
                    {
                        return temp;
                    }
                }
            }
            catch (Exception)
            {
            }
            return -1;
        }

        /// <summary>
        /// Whether or not the system's automatic fan control can be specifically engaged and disengaged.
        /// </summary>
        public bool IsAutomaticFanControlDisableSupported
        {
            get { return _fanController.IsAutomaticFanControlDisableSupported; }
        }

        /// <summary>
        /// Whether or not the system fans can be set to run at a specific level.
        /// </summary>
        public bool IsSpecificFanControlSupported
        {
            get { return _fanController.IsSpecificFanControlSupported; }
        }

        /// <summary>
        /// Whether or not the system fans may be individually controlled.
        /// </summary>
        public bool IsIndividualFanControlSupported
        {
            get { return _fanController.IsIndividualFanControlSupported; }
        }
    }
}
