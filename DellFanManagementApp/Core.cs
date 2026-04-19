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
        /// Lower temperature threshold for consistency mode.
        /// </summary>
        public int? LowerTemperatureThreshold { get; private set; }

        /// <summary>
        /// Upper temperature threshold for consistency mode.
        /// </summary>
        public int? UpperTemperatureThreshold { get; private set; }

        /// <summary>
        /// Fan RPM threshold for consistency mode.
        /// </summary>
        public ulong? RpmThreshold { get; private set; }

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

            RequestedThermalSetting = null;
            _ecFanControlRequested = true;
            _fan1LevelRequested = null;
            _fan2LevelRequested = null;

            LowerTemperatureThreshold = null;
            UpperTemperatureThreshold = null;
            RpmThreshold = null;
            TrayIconColor = TrayIconColor.Gray;
        }

        /// <summary>
        /// Switch configuration to automatic mode.
        /// </summary>
        public void SetAutomaticMode()
        {
            _state.WaitOne();
            _state.OperationMode = OperationMode.Automatic;
            _state.ConsistencyModeStatus = " ";
            _state.Release();
            TrayIconColor = TrayIconColor.Gray;
        }

        /// <summary>
        /// Switch configuration to manual mode.
        /// </summary>
        public void SetManualMode()
        {
            _state.WaitOne();
            _state.OperationMode = OperationMode.Manual;
            _ecFanControlRequested = _state.EcFanControlEnabled;
            _state.ConsistencyModeStatus = " ";
            _state.Fan1Level = null;
            _state.Fan2Level = null;
            _fan1LevelRequested = null;
            _fan2LevelRequested = null;
            _state.Release();
            TrayIconColor = TrayIconColor.Gray;
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

                    // Take action based on configuration.
                    if (_state.OperationMode == OperationMode.Automatic)
                    {
                        if (!_state.EcFanControlEnabled && IsAutomaticFanControlDisableSupported)
                        {
                            _state.EcFanControlEnabled = true;
                            _fanController.EnableAutomaticFanControl();
                            Log.Write("Enabled EC fan control – automatic mode");
                        }
                    }
                    else if (_state.OperationMode == OperationMode.Manual && IsAutomaticFanControlDisableSupported && IsSpecificFanControlSupported)
                    {
                        // Check for EC control state changes that need to be applied.
                        if (_ecFanControlRequested && !_state.EcFanControlEnabled)
                        {
                            _state.EcFanControlEnabled = true;
                            _fanController.EnableAutomaticFanControl();
                            Log.Write("Enabled EC fan control – manual mode");

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
                        }

                        // Check for fan control state changes that need to be applied.
                        if (!_state.EcFanControlEnabled)
                        {
                            if (_state.Fan1Level != _fan1LevelRequested)
                            {
                                _state.Fan1Level = _fan1LevelRequested;
                                if (_fan1LevelRequested != null)
                                {
                                    _fanController.SetFanLevel((FanLevel)_fan1LevelRequested, IsIndividualFanControlSupported ? FanIndex.Fan1 : FanIndex.AllFans);
                                }
                            }

                            if (_state.Fan2Present && IsIndividualFanControlSupported && _state.Fan2Level != _fan2LevelRequested)
                            {
                                _state.Fan2Level = _fan2LevelRequested;
                                if (_fan2LevelRequested != null)
                                {
                                    _fanController.SetFanLevel((FanLevel)_fan2LevelRequested, FanIndex.Fan2);
                                }
                            }
                        }

                        // Warn if a fan is set to completely off.
                        if (!_state.EcFanControlEnabled && (_state.Fan1Level == FanLevel.Off || (_state.Fan2Present && _state.Fan2Level == FanLevel.Off)))
                        {
                            _state.ConsistencyModeStatus = "Warning: Fans set to \"off\" will not turn on regardless of temperature or load on the system";
                        }
                        else
                        {
                            _state.ConsistencyModeStatus = " ";
                        }
                    }

                    // 应用用户请求的热设置
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

                    // 智能UI更新：只在数据变化或达到强制更新间隔时更新
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
                    // (There could be an error if trying to update the form after it has been closed... let it slide.)
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

            // 获取当前温度数据
            int currentCpuTemp = GetCpuTemperature();
            int currentGpuTemp = GetGpuTemperature();

            // 检查CPU温度变化（超过1度才更新）
            if (currentCpuTemp >= 0 && Math.Abs(currentCpuTemp - _lastCpuTemperature) >= 1)
            {
                changed = true;
                _lastCpuTemperature = currentCpuTemp;
            }

            // 检查GPU温度变化（超过1度才更新）
            if (currentGpuTemp >= 0 && Math.Abs(currentGpuTemp - _lastGpuTemperature) >= 1)
            {
                changed = true;
                _lastGpuTemperature = currentGpuTemp;
            }

            // 检查风扇1转速变化（超过50 RPM才更新）
            if (_state.Fan1Rpm.HasValue)
            {
                if (!_lastFan1Rpm.HasValue || Math.Abs((int)(_state.Fan1Rpm.Value - _lastFan1Rpm.Value)) >= 50)
                {
                    changed = true;
                    _lastFan1Rpm = _state.Fan1Rpm;
                }
            }

            // 检查风扇2转速变化（超过50 RPM才更新）
            if (_state.Fan2Rpm.HasValue)
            {
                if (!_lastFan2Rpm.HasValue || Math.Abs((int)(_state.Fan2Rpm.Value - _lastFan2Rpm.Value)) >= 50)
                {
                    changed = true;
                    _lastFan2Rpm = _state.Fan2Rpm;
                }
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
                // 忽略异常
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
                        return temp; // 返回第一个GPU温度
                    }
                }
            }
            catch (Exception)
            {
                // 忽略异常
            }
            return -1;
        }

        /// <summary>
        /// Write the consistency mode configuration.
        /// </summary>
        /// <param name="lowerTemperatureThreshold">Lower temperature threshold</param>
        /// <param name="upperTemperatureThreshold">Upper temperature threshold</param>
        /// <param name="rpmThreshold">Fan speed threshold</param>
        public void WriteConsistencyModeConfiguration(int lowerTemperatureThreshold, int upperTemperatureThreshold, int rpmThreshold)
        {
            LowerTemperatureThreshold = lowerTemperatureThreshold;
            UpperTemperatureThreshold = upperTemperatureThreshold;
            RpmThreshold = ulong.Parse(rpmThreshold.ToString());
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
