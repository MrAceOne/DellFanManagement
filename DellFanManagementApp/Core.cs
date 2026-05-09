using DellFanManagement.App.FanControllers;
using DellFanManagement.App.TemperatureReaders;
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
        /// RPM values above this are most likely bogus.
        /// </summary>
        public static readonly ulong RpmSanityCheck = 6500;

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
        /// Indicates whether or not the user has requested that EC fan control be enabled.
        /// </summary>
        private bool _ecFanControlRequested;

        /// <summary>
        /// User requested level for fan 1.
        /// </summary>
        private FanLevel? _fan1LevelRequested;

        /// <summary>
        /// User requested level for fan 2.
        /// </summary>
        private FanLevel? _fan2LevelRequested;

        /// <summary>
        /// CPU temperature threshold for manual mode (default 45 degrees).
        /// </summary>
        public int CpuTemperatureThreshold { get; private set; } = 45;

        /// <summary>
        /// GPU temperature threshold for manual mode (default 45 degrees).
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
            _ecFanControlRequested = true;
            _fan1LevelRequested = null;
            _fan2LevelRequested = null;

            TrayIconColor = TrayIconColor.Gray;

            // Load configuration values.
            LoadConfiguration();
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
        }

        /// <summary>
        /// Request that EC fan control be enabled or disabled.
        /// </summary>
        /// <param name="enabled">True to enable EC fan control, false to disable it</param>
        public void RequestEcFanControl(bool enabled)
        {
            _requestSemaphore.WaitOne();
            _ecFanControlRequested = enabled;
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
                if (_state.EcFanControlEnabled && IsAutomaticFanControlDisableSupported)
                {
                    _fanController.EnableAutomaticFanControl();
                    Log.Write("Enabled EC fan control – startup");
                }

                while (_state.BackgroundThreadRunning)
                {
                    _state.WaitOne();
                    _requestSemaphore.WaitOne();
                    releaseSemaphore = true;

                    // Update state.
                    _state.Update();

                    // Handle EC fan control state changes.
                    if (_ecFanControlRequested && !_state.EcFanControlEnabled)
                    {
                        _state.EcFanControlEnabled = true;
                        _fanController.EnableAutomaticFanControl();
                        Log.Write("Enabled EC fan control – automatic mode");

                        _state.Fan1Level = null;
                        _state.Fan2Level = null;
                        _fan1LevelRequested = null;
                        _fan2LevelRequested = null;
                    }
                    else if (!_ecFanControlRequested && _state.EcFanControlEnabled)
                    {
                        _state.EcFanControlEnabled = false;
                        _fanController.DisableAutomaticFanControl();
                        Log.Write("Disabled EC fan control – manual mode");

                        // Immediately apply temperature-based fan control when switching to manual mode.
                        if (IsAutomaticFanControlDisableSupported && IsSpecificFanControlSupported)
                        {
                            _temperatureCheckCounter = 0;
                            ApplyTemperatureBasedFanControl();
                        }
                    }

                    // In manual mode (EC fan control disabled), apply temperature-based fan control.
                    if (!_state.EcFanControlEnabled && IsAutomaticFanControlDisableSupported && IsSpecificFanControlSupported)
                    {
                        _temperatureCheckCounter++;
                        bool shouldCheckTemperature = _temperatureCheckCounter >= CheckIntervalSeconds;

                        if (shouldCheckTemperature)
                        {
                            _temperatureCheckCounter = 0;
                            ApplyTemperatureBasedFanControl();
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

                    // Apply user requested thermal setting.
                    if (RequestedThermalSetting != null && RequestedThermalSetting != _state.ThermalSetting)
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
                if (IsAutomaticFanControlDisableSupported)
                {
                    _fanController.EnableAutomaticFanControl();
                    Log.Write("Enabled EC fan control – shutdown");
                }

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

            UpdateForm();
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
                        _state.Fan1Level = FanLevel.Medium;
                        _fanController.SetFanLevel(FanLevel.Medium, IsIndividualFanControlSupported ? FanIndex.Fan1 : FanIndex.AllFans);
                        Log.Write($"Manual mode: CPU temp {cpuTemp}°C >= {CpuTemperatureThreshold}°C, Fan 1 set to Medium");
                    }
                }
                else
                {
                    if (_state.Fan1Level != FanLevel.Off)
                    {
                        _state.Fan1Level = FanLevel.Off;
                        _fanController.SetFanLevel(FanLevel.Off, IsIndividualFanControlSupported ? FanIndex.Fan1 : FanIndex.AllFans);
                        Log.Write($"Manual mode: CPU temp {cpuTemp}°C < {CpuTemperatureThreshold}°C, Fan 1 set to Off");
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
                        _state.Fan2Level = FanLevel.Medium;
                        if (IsIndividualFanControlSupported)
                        {
                            _fanController.SetFanLevel(FanLevel.Medium, FanIndex.Fan2);
                        }
                        Log.Write($"Manual mode: GPU temp {gpuTemp}°C >= {GpuTemperatureThreshold}°C, Fan 2 set to Medium");
                    }
                }
                else
                {
                    if (_state.Fan2Level != FanLevel.Off)
                    {
                        _state.Fan2Level = FanLevel.Off;
                        if (IsIndividualFanControlSupported)
                        {
                            _fanController.SetFanLevel(FanLevel.Off, FanIndex.Fan2);
                        }
                        Log.Write($"Manual mode: GPU temp {gpuTemp}°C < {GpuTemperatureThreshold}°C, Fan 2 set to Off");
                    }
                }
            }
        }

        /// <summary>
        /// Request that the GUI form update using current values from the state.
        /// </summary>
        private void UpdateForm()
        {
            MethodInvoker updateInvoker = new(_form.UpdateForm);

            if (!_state.FormClosed)
            {
                try
                {
                    _form.BeginInvoke(updateInvoker);
                }
                catch (Exception)
                {
                    // Take no action.
                }
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

            if (currentCpuTemp >= 0 && Math.Abs(currentCpuTemp - _lastCpuTemperature) >= 1)
            {
                changed = true;
                _lastCpuTemperature = currentCpuTemp;
            }

            if (currentGpuTemp >= 0 && Math.Abs(currentGpuTemp - _lastGpuTemperature) >= 1)
            {
                changed = true;
                _lastGpuTemperature = currentGpuTemp;
            }

            if (_state.Fan1Rpm.HasValue)
            {
                changed = true;
                _lastFan1Rpm = _state.Fan1Rpm;
            }

            if (_state.Fan2Rpm.HasValue)
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
