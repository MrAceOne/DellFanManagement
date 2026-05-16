using DellFanManagement.App.FanSpeedReaders;
using DellFanManagement.DellSmbiosSmiLib;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DellFanManagement.App
{
    /// <summary>
    /// Defines types of components that have temperatures that we care about.
    /// </summary>
    public enum TemperatureComponent
    {
        /// <summary>
        /// CPU temperatures.
        /// </summary>
        CPU,

        /// <summary>
        /// GPU temperatures.
        /// </summary>
        GPU
    }

    /// <summary>
    /// Represents, basically, the current state of the application.  Used for sharing data between the UI and
    /// background threads.
    /// </summary>
    public class State
    {
        /// <summary>
        /// Object for reading the fan speeds from the system.
        /// </summary>
        private readonly IFanSpeedReader _fanSpeedReader;

        /// <summary>
        /// Object for reading system monitor data (CPU/GPU temperatures, frequency, memory, etc.)
        /// </summary>
        private readonly SystemMonitor _systemMonitor;

        /// <summary>
        /// Semaphore for protecting access to state changes.
        /// </summary>
        private readonly Semaphore _semaphore;

        /// <summary>
        /// Only allow changes to the state when this value is true.
        /// </summary>
        private bool _changesAllowed;

        /// <summary>
        /// If there is an error message to display, it goes here.
        /// </summary>
        private string _error;

        /// <summary>
        /// Indicates which configuration mode the app is currently running under.
        /// </summary>
        private OperationMode _configuration;

        /// <summary>
        /// Whether or not the "background thread" is running.
        /// </summary>
        private bool _backgroundThreadRunning;

        /// <summary>
        /// Whether or not the "audio keep alive" thread is running.
        /// </summary>
        private bool _audioThreadRunning;

        /// <summary>
        /// Whether or not the form hosting the main application has been closed.
        /// </summary>
        private bool _formClosed;

        /// <summary>
        /// Whether or not the main window is currently visible (not minimized to tray).
        /// </summary>
        private bool _windowVisible;

        /// <summary>
        /// Indicates whether or not EC fan control is enabled.
        /// </summary>
        private bool _ecFanControlEnabled;

        /// <summary>
        /// User-selected fan control mode on the UI (decoupled from actual EC state).
        /// </summary>
        private FanMode _fanMode;

        /// <summary>
        /// Current level manually set for fan 1.
        /// </summary>
        private FanLevel? _fan1Level;

        /// <summary>
        /// Current level manually set for fan 2.
        /// </summary>
        private FanLevel? _fanLevel2;

        /// <summary>
        /// Status of the consistency mode system.
        /// </summary>
        private string _consistencyModeStatus;

        /// <summary>
        /// Number of times in a row that the thermal setting has failed to update.
        /// </summary>
        private int _consecutiveThermalSettingFailures;

        /// <summary>
        /// Number of times before trying to read the thermal setting again (exponential backoff).
        /// </summary>
        private int _thermalSettingReadBackoff;

        /// <summary>
        /// Constructor; initialize everything.
        /// </summary>
        /// <param name="configurationStore">Configuration store.</param>
        public State(ConfigurationStore configurationStore)
        {
            _backgroundThreadRunning = false;
            _audioThreadRunning = false;
            _formClosed = false;
            _windowVisible = true;
            _ecFanControlEnabled = true;
            _fanMode = FanMode.Automatic;
            _error = null;

            _semaphore = new(1, 1);
            _changesAllowed = false;

            _consistencyModeStatus = " ";

            _consecutiveThermalSettingFailures = 0;
            _thermalSettingReadBackoff = 0;

            // Initialize fan speed reader.
            _fanSpeedReader = FanSpeedReaderFactory.GetFanSpeedReader();

            // Initialize system monitor using singleton pattern to avoid memory leaks
            _systemMonitor = SystemMonitor.Instance;

            Temperatures = new();
            MinimumTemperatures = new();
            MaximumTemperatures = new();

            Fan2Present = true;
            ActivePowerProfile = null;

            WaitOne();
            Update();
            Release();
        }

        /// <summary>
        /// Update the state.
        /// </summary>
        /// <param name="reader">A temperature reader</param>
        public void Update()
        {
            AccessCheck();

            UpdateFanRpms();

            // Single call to SystemMonitor — shared for both temperatures and monitor data.
            SystemMonitorData monitorData = _systemMonitor.GetMonitorData();
            UpdateTemperatures(monitorData);
            UpdatePowerProfile();
            UpdateSystemMonitorData(monitorData);
        }

        /// <summary>
        /// Update system monitor data (CPU frequency, GPU frequency, memory usage).
        /// </summary>
        /// <param name="monitorData">Pre-fetched monitor data to avoid duplicate hardware enumeration.</param>
        private void UpdateSystemMonitorData(SystemMonitorData monitorData)
        {
            try
            {
                CpuFrequency = monitorData.CpuFrequency;
                GpuFrequency = monitorData.GpuFrequency;
                MemoryUsagePercent = monitorData.MemoryUsagePercent;
                UsedMemoryMB = monitorData.UsedMemoryMB;
                TotalMemoryMB = monitorData.TotalMemoryMB;
            }
            catch (Exception ex)
            {
                Log.Write($"Error updating system monitor data: {ex.Message}");
            }
        }

        /// <summary>
        /// Update the fan RPMs.
        /// </summary>
        private void UpdateFanRpms()
        {
            // Update state: RPM.
            FanSpeeds fanSpeeds = _fanSpeedReader.GetFanSpeeds();
            Fan1Rpm = fanSpeeds.Fan1Rpm;
            Fan2Rpm = fanSpeeds.Fan2Rpm;

            if (Fan1Rpm != null && Fan2Rpm == null)
            {
                Fan2Present = false;
            }
            else if (Fan2Rpm != null)
            {
                Fan2Present = true;
            }
        }

        /// <summary>
        /// Update "thermal setting".
        /// </summary>
        public void UpdateThermalSetting()
        {
            // If a backoff has been set, decrement it.
            if (_thermalSettingReadBackoff > 0)
            {
                _thermalSettingReadBackoff--;
            }
            else
            {
                ThermalSetting = DellSmbiosSmi.GetThermalSetting();

                if (ThermalSetting == ThermalSetting.Error)
                {
                    // Reading this setting on an unsupported system can cause high CPU usage from WMI.
                    // Exponential backoff before trying to read it again.
                    _consecutiveThermalSettingFailures++;
                    _thermalSettingReadBackoff = 1;
                    for (int index = 0; index < _consecutiveThermalSettingFailures; index++)
                    {
                        _thermalSettingReadBackoff *= 4;
                    }
                }
                else
                {
                    _consecutiveThermalSettingFailures = 0;
                }
            }
        }

        /// <summary>
        /// Update temperatures from pre-fetched SystemMonitor data.
        /// </summary>
        /// <param name="monitorData">Monitor data already obtained from SystemMonitor.</param>
        private void UpdateTemperatures(SystemMonitorData monitorData)
        {
            // Update CPU temperature.
            if (monitorData.CpuTemperature.HasValue)
            {
                int cpuTemp = monitorData.CpuTemperature.Value;
                Temperatures[TemperatureComponent.CPU] = new Dictionary<string, int> { { "CPU", cpuTemp } };

                if (!MinimumTemperatures.ContainsKey(TemperatureComponent.CPU))
                {
                    MinimumTemperatures[TemperatureComponent.CPU] = new();
                }
                if (!MaximumTemperatures.ContainsKey(TemperatureComponent.CPU))
                {
                    MaximumTemperatures[TemperatureComponent.CPU] = new();
                }

                if (!MinimumTemperatures[TemperatureComponent.CPU].ContainsKey("CPU") || (cpuTemp < MinimumTemperatures[TemperatureComponent.CPU]["CPU"] && cpuTemp > 0))
                {
                    MinimumTemperatures[TemperatureComponent.CPU]["CPU"] = cpuTemp;
                }
                if (!MaximumTemperatures[TemperatureComponent.CPU].ContainsKey("CPU") || (cpuTemp > MaximumTemperatures[TemperatureComponent.CPU]["CPU"] && cpuTemp > 0))
                {
                    MaximumTemperatures[TemperatureComponent.CPU]["CPU"] = cpuTemp;
                }
            }

            // Update GPU temperature.
            if (monitorData.GpuTemperature.HasValue)
            {
                int gpuTemp = monitorData.GpuTemperature.Value;
                Temperatures[TemperatureComponent.GPU] = new Dictionary<string, int> { { "GPU", gpuTemp } };

                if (!MinimumTemperatures.ContainsKey(TemperatureComponent.GPU))
                {
                    MinimumTemperatures[TemperatureComponent.GPU] = new();
                }
                if (!MaximumTemperatures.ContainsKey(TemperatureComponent.GPU))
                {
                    MaximumTemperatures[TemperatureComponent.GPU] = new();
                }

                if (!MinimumTemperatures[TemperatureComponent.GPU].ContainsKey("GPU") || (gpuTemp < MinimumTemperatures[TemperatureComponent.GPU]["GPU"] && gpuTemp > 0))
                {
                    MinimumTemperatures[TemperatureComponent.GPU]["GPU"] = gpuTemp;
                }
                if (!MaximumTemperatures[TemperatureComponent.GPU].ContainsKey("GPU") || (gpuTemp > MaximumTemperatures[TemperatureComponent.GPU]["GPU"] && gpuTemp > 0))
                {
                    MaximumTemperatures[TemperatureComponent.GPU]["GPU"] = gpuTemp;
                }
            }
        }

        /// <summary>
        /// Read the currently active Windows power profile.
        /// </summary>
        private void UpdatePowerProfile()
        {
            Guid? activeProfile = CpuPowerManager.GetActivePowerProfile();
            if (activeProfile != null)
            {
                ActivePowerProfile = activeProfile;
            }
        }

        /// <summary>
        /// Throw an exception if changes to the state are not presently allowed.
        /// </summary>
        private void AccessCheck()
        {
            if (!_changesAllowed)
            {
                throw new StateAccessException("Changes are not allowed until WaitOne is called.");
            }
        }

        /// <summary>
        /// Request access to make state changes (blocks until other threads are done).
        /// </summary>
        public void WaitOne()
        {
            _ = _semaphore.WaitOne();
            _changesAllowed = true;
        }

        /// <summary>
        /// Indicate that a thread is done making changes to the state.
        /// </summary>
        public void Release()
        {
            _changesAllowed = false;
            _semaphore.Release();
        }

        // ===========================================
        // Limited public propery declarations follow:
        // ===========================================

        /// <summary>
        /// If there is an error message to display, it goes here.
        /// </summary>
        public string Error
        {
            get { return _error; }
            set { AccessCheck(); _error = value; }
        }

        /// <summary>
        /// Indicates which configuration mode the app is currently running under.
        /// </summary>
        public OperationMode OperationMode
        {
            get { return _configuration; }
            set { AccessCheck(); _configuration = value; }
        }

        /// <summary>
        /// Whether or not the "background thread" is running.
        /// </summary>
        public bool BackgroundThreadRunning
        {
            get { return _backgroundThreadRunning; }
            set { AccessCheck(); _backgroundThreadRunning = value; }
        }

        /// <summary>
        /// Whether or not the "audio keep alive" thread is running.
        /// </summary>
        public bool AudioThreadRunning
        {
            get { return _audioThreadRunning; }
            set { AccessCheck(); _audioThreadRunning = value; }
        }

        /// <summary>
        /// Whether or not the form hosting the main application has been closed.
        /// </summary>
        public bool FormClosed
        {
            get { return _formClosed; }
            set { AccessCheck(); _formClosed = value; }
        }

        /// <summary>
        /// Whether or not the main window is currently visible (not minimized to tray).
        /// </summary>
        public bool WindowVisible
        {
            get { return _windowVisible; }
            set { AccessCheck(); _windowVisible = value; }
        }

        /// <summary>
        /// Indicates whether or not EC fan control is enabled.
        /// </summary>
        public bool EcFanControlEnabled
        {
            get { return _ecFanControlEnabled; }
            set { AccessCheck(); _ecFanControlEnabled = value; }
        }

        /// <summary>
        /// User-selected fan control mode on the UI (decoupled from actual EC state).
        /// </summary>
        public FanMode FanMode
        {
            get { return _fanMode; }
            set { AccessCheck(); _fanMode = value; }
        }

        /// <summary>
        /// Current level manually set for fan 1.
        /// </summary>
        public FanLevel? Fan1Level
        {
            get { return _fan1Level; }
            set { AccessCheck(); _fan1Level = value; }
        }

        /// <summary>
        /// Current level manually set for fan 2.
        /// </summary>
        public FanLevel? Fan2Level
        {
            get { return _fanLevel2; }
            set { AccessCheck(); _fanLevel2 = value; }
        }

        /// <summary>
        /// Status of the consistency mode system.
        /// </summary>
        public string ConsistencyModeStatus
        {
            get { return _consistencyModeStatus; }
            set { AccessCheck(); _consistencyModeStatus = value; }
        }

        /// <summary>
        /// RPM value for fan 1.
        /// </summary>
        public uint? Fan1Rpm { get; private set; }

        /// <summary>
        /// RPM value for fan 2.
        /// </summary>
        public uint? Fan2Rpm { get; private set; }

        /// <summary>
        /// Indicates whether or not a second fan is present in the system.
        /// </summary>
        public bool Fan2Present { get; private set; }

        /// <summary>
        /// Currently active power profile.
        /// </summary>
        public Guid? ActivePowerProfile { get; private set; }

        /// <summary>
        /// Current temperatures.
        /// </summary>
        public Dictionary<TemperatureComponent, IReadOnlyDictionary<string, int>> Temperatures { get; private set; }

        /// <summary>
        /// Minimum temperatures.
        /// </summary>
        public Dictionary<TemperatureComponent, Dictionary<string, int>> MinimumTemperatures { get; private set; }

        /// <summary>
        /// Maximum temperatures.
        /// </summary>
        public Dictionary<TemperatureComponent, Dictionary<string, int>> MaximumTemperatures { get; private set; }

        /// <summary>
        /// Current "thermal setting".
        /// </summary>
        public ThermalSetting ThermalSetting { get; private set; }
        
        /// <summary>
        /// Internal method to set thermal setting from Core class
        /// </summary>
        internal void SetThermalSetting(ThermalSetting setting)
        {
            AccessCheck();
            ThermalSetting = setting;
        }

        /// <summary>
        /// CPU核心频率（MHz）
        /// </summary>
        public int? CpuFrequency { get; private set; }

        /// <summary>
        /// GPU核心频率（MHz）
        /// </summary>
        public int? GpuFrequency { get; private set; }

        /// <summary>
        /// 内存使用率（百分比）
        /// </summary>
        public float? MemoryUsagePercent { get; private set; }

        /// <summary>
        /// 已使用内存（MB）
        /// </summary>
        public long? UsedMemoryMB { get; private set; }

        /// <summary>
        /// 总内存（MB）
        /// </summary>
        public long? TotalMemoryMB { get; private set; }
    }
}
