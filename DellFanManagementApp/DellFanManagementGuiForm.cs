using DellFanManagement.App.TemperatureReaders;
using DellFanManagement.DellSmbiosSmiLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace DellFanManagement.App
{
    /// <summary>
    /// This class manages the Windows Forms application for the app.
    /// </summary>
    public partial class DellFanManagementGuiForm : Form
    {
        /// <summary>
        /// Shared object which contains the application state.
        /// </summary>
        private readonly State _state;

        /// <summary>
        /// The "Core" object does the actual system interations.
        /// </summary>
        private readonly Core _core;

        /// <summary>
        /// Handles storing the options selected in the program in the registry.
        /// </summary>
        private readonly ConfigurationStore _configurationStore;

        /// <summary>
        /// Pre-loaded icons to use in the system tray.
        /// </summary>
        private readonly Icon[] _trayIcons;

        /// <summary>
        /// Next tray icon animation to be displayed.
        /// </summary>
        private int _trayIconIndex;

        /// <summary>
        /// Indicates that the program is closing, so background threads should stop.
        /// </summary>
        private bool _formClosed;

        /// <summary>
        /// Indicates whether the initial setup code has finished or not.
        /// </summary>
        private readonly bool _initializationComplete;

        /// <summary>
        /// Last observed Windows power profile.
        /// </summary>
        private Guid? _registeredPowerProfile;

        /// <summary>
        /// Current "thermal setting".
        /// </summary>
        public ThermalSetting ThermalSetting { get; private set; }

        /// <summary>
        /// Constructor.  Get everything set up before the window is displayed.
        /// </summary>
        public DellFanManagementGuiForm()
        {
            _initializationComplete = false;

            InitializeComponent();

            // Initialize objects.
            _configurationStore = new();
            _state = new State(_configurationStore);
            _core = new Core(_state, this);
            _formClosed = false;

            _trayIcons = new Icon[48];
            _trayIconIndex = 0;
            LoadTrayIcons();

            // Disclaimer.
            if (_configurationStore.GetIntOption(ConfigurationOption.DisclaimerShown) != 1)
            {
                ShowDisclaimer();
                _configurationStore.SetOption(ConfigurationOption.DisclaimerShown, 1);
            }

            // Version number in the about box.
            //aboutProductLabel.Text = string.Format("Dell Fan Management, version {0}", DellFanManagementApp.Version);

            // Set event handlers.
            FormClosed += new FormClosedEventHandler(FormClosedEventHandler);
            Resize += new EventHandler(OnResizeEventHandler);
            trayIcon.Click += new EventHandler(TrayIconOnClickEventHandler);
            trayMenuItemShow.Click += new EventHandler(TrayMenuItemShowClickEventHandler);
            trayMenuItemExit.Click += new EventHandler(TrayMenuItemExitClickEventHandler);

            // ...Thermal setting radio buttons...
            thermalSettingRadioButtonOptimized.CheckedChanged += new EventHandler(ThermalSettingChangedEventHandler);
            thermalSettingRadioButtonCool.CheckedChanged += new EventHandler(ThermalSettingChangedEventHandler);
            thermalSettingRadioButtonQuiet.CheckedChanged += new EventHandler(ThermalSettingChangedEventHandler);
            thermalSettingRadioButtonPerformance.CheckedChanged += new EventHandler(ThermalSettingChangedEventHandler);

            // ...EC fan control radio buttons...
            ecFanControlRadioButtonOn.CheckedChanged += new EventHandler(EcFanControlSettingChangedEventHandler);
            ecFanControlRadioButtonOff.CheckedChanged += new EventHandler(EcFanControlSettingChangedEventHandler);

            // ...Manual fan control radio buttons...
            manualFan1RadioButtonOff.CheckedChanged += new EventHandler(FanLevelChangedEventHandler);
            manualFan1RadioButtonMedium.CheckedChanged += new EventHandler(FanLevelChangedEventHandler);
            manualFan1RadioButtonHigh.CheckedChanged += new EventHandler(FanLevelChangedEventHandler);
            manualFan2RadioButtonOff.CheckedChanged += new EventHandler(FanLevelChangedEventHandler);
            manualFan2RadioButtonMedium.CheckedChanged += new EventHandler(FanLevelChangedEventHandler);
            manualFan2RadioButtonHigh.CheckedChanged += new EventHandler(FanLevelChangedEventHandler);

            // ...Operation mode radio buttons...
            operationModeRadioButtonAutomatic.CheckedChanged += new EventHandler(ConfigurationRadioButtonAutomaticEventHandler);
            operationModeRadioButtonManual.CheckedChanged += new EventHandler(ConfigurationRadioButtonManualEventHandler);
            
            frequencyTextBox.TextChanged += new EventHandler(ConsistencyModeTextBoxesChangedEventHandler);
            powerApplyButton.Click += new EventHandler(PowerApplyButtonClickedEventHandler);

            eppTrackBar.Scroll += new EventHandler(EppTrackBarScrollEventHandler);

            // Empty out pre-populated temperature label text fields.
            // (There are so many to allow support for lots of CPU cores, which many systems will not have.)
            temperatureLabel1.Text = string.Empty;
            temperatureLabel2.Text = string.Empty;

            // Disable some options depending on fan control capability.
            if (!_core.IsAutomaticFanControlDisableSupported)
            {
                operationModeRadioButtonManual.Enabled = false;
            }
            if (!_core.IsSpecificFanControlSupported)
            {
                operationModeRadioButtonManual.Enabled = false;
            }

            // Initial update of the tray icon (required for it to appear for display).
            UpdateTrayIcon(false);

            //初始化电源管理相关的UI元素。
            UpdatePowerForm();
            // Update form with default state values.
            UpdateForm();
            _state.UpdateThermalSetting();
            // Apply manual fan control configuration from registry.
            ApplyManualModeConfiguration();
            
            // Apply operation mode configuration from registry.
            ApplyConfiguration();

            // Start threads to do background work.
            _core.StartBackgroundThread();
            StartTrayIconThread();

            _initializationComplete = true;
        }

        /// <summary>
        /// Apply configuration settings loaded from the registry.
        /// </summary>
        private void ApplyConfiguration()
        {
            // Consistency mode settings.
            int? lowerTemperatureThreshold = _configurationStore.GetIntOption(ConfigurationOption.ConsistencyModeLowerTemperatureThreshold);
            int? upperTemperatureThreshold = _configurationStore.GetIntOption(ConfigurationOption.ConsistencyModeUpperTemperatureThreshold);
            int? rpmThreshold = _configurationStore.GetIntOption(ConfigurationOption.ConsistencyModeRpmThreshold);
            

            // Read previous operation mode from configuration.
            bool modeSet = false;
            if (Enum.TryParse(_configurationStore.GetStringOption(ConfigurationOption.OperationMode), out OperationMode operationMode))
            {
                switch (operationMode)
                {
                    case OperationMode.Automatic:
                        operationModeRadioButtonAutomatic.Checked = true;
                        modeSet = true;
                        break;
                    case OperationMode.Manual:
                        if (operationModeRadioButtonManual.Enabled)
                        {
                            operationModeRadioButtonManual.Checked = true;
                            modeSet = true;
                        }
                        break;
                    case OperationMode.Consistency:
                        
                        break;
                }
            }
            if (!modeSet)
            {
                // Default to automatic mode.
                operationModeRadioButtonAutomatic.Checked = true;
            }
        }

        /// <summary>
        /// Apply manual mode configuration from the registry.
        /// </summary>
        private void ApplyManualModeConfiguration()
        {
            if (operationModeRadioButtonManual.Checked)
            {
                // Apply saved manual mode configuration.
                if (_configurationStore.GetIntOption(ConfigurationOption.ManualModeEcFanControlEnabled) == 0)
                {
                    ecFanControlRadioButtonOff.Checked = true;

                    if (Enum.TryParse(_configurationStore.GetStringOption(ConfigurationOption.ManualModeFan1Level), out FanLevel fan1Level))
                    {
                        switch (fan1Level)
                        {
                            case FanLevel.Off:
                                manualFan1RadioButtonOff.Checked = true;
                                break;
                            case FanLevel.Medium:
                                manualFan1RadioButtonMedium.Checked = true;
                                break;
                            case FanLevel.High:
                                manualFan1RadioButtonHigh.Checked = true;
                                break;
                        }
                    }

                    if (Enum.TryParse(_configurationStore.GetStringOption(ConfigurationOption.ManualModeFan2Level), out FanLevel fan2Level))
                    {
                        switch (fan2Level)
                        {
                            case FanLevel.Off:
                                manualFan2RadioButtonOff.Checked = true;
                                break;
                            case FanLevel.Medium:
                                manualFan2RadioButtonMedium.Checked = true;
                                break;
                            case FanLevel.High:
                                manualFan2RadioButtonHigh.Checked = true;
                                break;
                        }
                    }
                }
            }
        }

        private void UpdatePowerForm()
        {
            if (CpuPowerApi.GetGuidByState(CpuPowerApi.GUID_PROCESSOR_PERFEPP, out uint epp) == 0)
            {
                eppTrackBar.Value = (int)epp;
                eppLabel.Text = string.Format("EPP: {0}", epp);
            }

            if (CpuPowerApi.GetGuidByState(CpuPowerApi.GUID_PROCESSOR_FREQUENCYMAX, out uint frequencyMax) == 0)
            {
                frequencyTextBox.Text = frequencyMax.ToString();
            }
            Console.WriteLine("Finished updating power form.");
        }

        /// <summary>
        /// Clear the manual operation mode configuration (when we switch to a different mode, it should be reset).
        /// </summary>
        private void ClearManualControlConfiguration()
        {
            _configurationStore.SetOption(ConfigurationOption.ManualModeEcFanControlEnabled, null);
            _configurationStore.SetOption(ConfigurationOption.ManualModeFan1Level, null);
            _configurationStore.SetOption(ConfigurationOption.ManualModeFan2Level, null);
        }

        /// <summary>
        /// Update the form based on the current state.
        /// </summary>
        public void UpdateForm()
        {
            // This method does not write to the state, but we should still make sure that the state is not changing
            // during the update.
            _state.WaitOne();

            // Fan RPM.
            fan1RpmLabel.Text = string.Format("Fan 1 RPM: {0}", _state.Fan1Rpm != null ? _state.Fan1Rpm : "(Error)");

            if (_state.Fan2Present)
            {
                fan2RpmLabel.Text = string.Format("Fan 2 RPM: {0}", _state.Fan2Rpm != null ? _state.Fan2Rpm : "(Error)");
                fan2RpmLabel.Enabled = true;

                if (_core.IsIndividualFanControlSupported)
                {
                    manualFan2GroupBox.Enabled = true;
                }
            }
            else
            {
                fan2RpmLabel.Text = string.Format("Fan 2 not present");
                fan2RpmLabel.Enabled = false;
            }

            // Temperatures.
            int labelIndex = 0;
            foreach (TemperatureComponent component in _state.Temperatures.Keys)
            {
                foreach (string key in _state.Temperatures[component].Keys)
                {
                    string temperature = _state.Temperatures[component][key] != 0 ? _state.Temperatures[component][key].ToString() : "--";

                    string labelValue;
                    if (_state.MinimumTemperatures[component].ContainsKey(key) && _state.MaximumTemperatures[component].ContainsKey(key))
                    {
                        labelValue = string.Format("{0}: {1} ({2}-{3})", key, temperature, _state.MinimumTemperatures[component][key], _state.MaximumTemperatures[component][key]);
                    }
                    else
                    {
                        labelValue = string.Format("{0}: {1}", key, temperature);
                    }

                    switch (labelIndex)
                    {
                        case 0: temperatureLabel1.Text = labelValue; break;
                        case 1: temperatureLabel2.Text = labelValue; break;
                    }

                    labelIndex++;
                }
            }

            // 系统监测数据 - CPU和GPU频率已经是GHz单位，内存显示为 已用/总内存 (GB)
            cpuFrequencyLabel.Text = _state.CpuFrequency.HasValue 
                ? string.Format("CPU 频率: {0:F1} GHz", _state.CpuFrequency.Value / 1000.0) 
                : "CPU 频率: --";

            gpuFrequencyLabel.Text = _state.GpuFrequency.HasValue 
                ? string.Format("GPU 频率: {0:F1} GHz", _state.GpuFrequency.Value / 1000.0) 
                : "GPU 频率: --";

            // 内存显示：已用内存/总内存 (GB) 使用率百分比
            if (_state.UsedMemoryMB.HasValue && _state.TotalMemoryMB.HasValue)
            {
                float usedGB = _state.UsedMemoryMB.Value / 1024.0f;
                float totalGB = _state.TotalMemoryMB.Value / 1024.0f;
                float usagePercent = (usedGB / totalGB) * 100;
                memoryLabel.Text = string.Format("内存: {0:F1}/{1:F1} GB ({2:F1}%)", usedGB, totalGB, usagePercent);
            }
            else
            {
                memoryLabel.Text = "内存: --";
            }

            // EC fan control enabled?
            if (_state.OperationMode != OperationMode.Manual)
            {
                if (_state.EcFanControlEnabled && !ecFanControlRadioButtonOn.Checked)
                {
                    ecFanControlRadioButtonOn.Checked = true;
                }
                else if (!_state.EcFanControlEnabled && !ecFanControlRadioButtonOff.Checked)
                {
                    ecFanControlRadioButtonOff.Checked = true;
                }
            }

            // Consistency mode status.
            //consistencyModeStatusLabel.Text = _state.ConsistencyModeStatus;

            // Thermal setting.
            if (_core.RequestedThermalSetting == null)
            {
                switch (_state.ThermalSetting)
                {
                    case ThermalSetting.Optimized:
                        SetThermalSettingAvaiability(true);
                        thermalSettingRadioButtonOptimized.Checked = true;
                        break;
                    case ThermalSetting.Cool:
                        SetThermalSettingAvaiability(true);
                        thermalSettingRadioButtonCool.Checked = true;
                        break;
                    case ThermalSetting.Quiet:
                        SetThermalSettingAvaiability(true);
                        thermalSettingRadioButtonQuiet.Checked = true;
                        break;
                    case ThermalSetting.Performance:
                        SetThermalSettingAvaiability(true);
                        thermalSettingRadioButtonPerformance.Checked = true;
                        break;
                    case ThermalSetting.Error:
                        SetThermalSettingAvaiability(false);
                        break;
                }
            }

            // Restart background thread button removed.

            // Tray icon hover text.
            if (_state.Fan2Present)
            {
                trayIcon.Text = string.Format("Dell Fan Management\n{0}\n{1}", fan1RpmLabel.Text, fan2RpmLabel.Text);
            }
            else
            {
                trayIcon.Text = string.Format("Dell Fan Management\n{0}", fan1RpmLabel.Text);
            }

            UpdateTrayIcon(false);

            // Error message.
            if (_state.Error != null)
            {
                MessageBox.Show(_state.Error, "Error in background thread", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _state.Error = null; // ...The one place where the state is actually updated.
            }

            // Power profiles management.
            if (_state.ActivePowerProfile != null)
            {
                if (_state.ActivePowerProfile != _registeredPowerProfile)
                {
                    if (_registeredPowerProfile == null)
                    {
                        Log.Write(string.Format("The active power profile is {0}", _state.ActivePowerProfile));
                    }
                    else
                    {
                        Log.Write(string.Format("Power profile changed from {0} to {1}", _registeredPowerProfile, _state.ActivePowerProfile));

                        // Check to see if we should change the thermal setting.
                        ThermalSetting? thermalSettingOverride = _configurationStore.GetThermalSettingOverride((Guid)_state.ActivePowerProfile);
                        if (thermalSettingOverride != null)
                        {
                            _core.RequestThermalSetting((ThermalSetting)thermalSettingOverride);
                            Log.Write(string.Format("Thermal setting override: {0}", thermalSettingOverride));
                        }

                        // Check to see if we should change the power mode.
                        Guid? powerMode = _configurationStore.GetPowerModeOverride((Guid)_state.ActivePowerProfile);
                        if (powerMode != null)
                        {
                            Utility.PowerSetActiveOverlayScheme((Guid)powerMode); // NULL check above.
                            Log.Write(string.Format("Power mode overrode: {0}", powerMode));
                        }

                        // Check to see if we should change the NVIDIAGPU P-state.
                        int? nvPstate = _configurationStore.GetNvPstateOverride((Guid)_state.ActivePowerProfile);
                        if (nvPstate != null)
                        {
                            string nvInspectorPath = _configurationStore.GetStringOption(ConfigurationOption.NVPStateApplicationPath);
                            if (nvInspectorPath != null)
                            {
                                Utility.SetNvidiaGpuPstate(nvInspectorPath, (int)nvPstate); // NULL check above.
                                Log.Write(string.Format("NVIDIA P-state override: {0}", nvPstate));
                            }
                        }
                    }
                    _registeredPowerProfile = _state.ActivePowerProfile;
                }
            }

            //Console.WriteLine("Finished updating form.");

            _state.Release();
        }

        /// <summary>
        /// Called when "Automatic" configuration radio button is clicked.
        /// </summary>
        private void ConfigurationRadioButtonAutomaticEventHandler(Object sender, EventArgs e)
        {
            _core.SetAutomaticMode();
            _configurationStore.SetOption(ConfigurationOption.OperationMode, OperationMode.Automatic);
            ClearManualControlConfiguration();

            SetFanControlsAvailability(false);
            SetConsistencyModeControlsAvailability(false);
            SetEcFanControlsAvailability(false);

            UpdateTrayIcon(false);
        }

        /// <summary>
        /// Called when "Manual" configuration radio button is clicked.
        /// </summary>
        private void ConfigurationRadioButtonManualEventHandler(Object sender, EventArgs e)
        {
            ecFanControlRadioButtonOn.Checked = true;
            _core.SetManualMode();
            _configurationStore.SetOption(ConfigurationOption.OperationMode, OperationMode.Manual);

            SetFanControlsAvailability(false);
            SetConsistencyModeControlsAvailability(false);
            SetEcFanControlsAvailability(true);

            UpdateTrayIcon(false);
        }

        /// <summary>
        /// Enable or disable the manual fan control controls.
        /// </summary>
        /// <param name="enabled">Indicates whether to enable or disable the controls</param>
        private void SetFanControlsAvailability(bool enabled)
        {
            manualGroupBox.Enabled = enabled;

            if (!enabled)
            {
                manualFan1RadioButtonOff.Checked = false;
                manualFan1RadioButtonMedium.Checked = false;
                manualFan1RadioButtonHigh.Checked = false;
                manualFan2RadioButtonOff.Checked = false;
                manualFan2RadioButtonMedium.Checked = false;
                manualFan2RadioButtonHigh.Checked = false;
            }
            else
            {
                // Disable manual fan control fields if needed.
                if (!_core.IsIndividualFanControlSupported)
                {
                    manualFan2GroupBox.Enabled = false;
                }
                if (!_state.Fan2Present)
                {
                    manualFan2GroupBox.Enabled = false;
                    manualFan2RadioButtonOff.Checked = false;
                    manualFan2RadioButtonMedium.Checked = false;
                    manualFan2RadioButtonHigh.Checked = false;
                }
            }
        }

        /// <summary>
        /// Enable or disbale the consistency mode configuration controls.
        /// </summary>
        /// <param name="enabled">Indicates whether to enable or disable the controls</param>
        private void SetConsistencyModeControlsAvailability(bool enabled)
        {
            //consistencyModeGroupBox.Enabled = enabled;
        }

        /// <summary>
        /// Enable or disable the EC fan control on/off controls.
        /// </summary>
        /// <param name="enabled">Indicates whether to enable or disable the controls</param>
        private void SetEcFanControlsAvailability(bool enabled)
        {
            ecFanControlRadioButtonOn.Enabled = enabled;
            ecFanControlRadioButtonOff.Enabled = enabled;
        }

        /// <summary>
        /// Enable or disable the thermal setting controls.
        /// </summary>
        /// <param name="enabled">Indicates whether to enable or disable the controls</param>
        private void SetThermalSettingAvaiability(bool enabled)
        {
            thermalSettingGroupBox.Enabled = enabled;

            if (!enabled)
            {
                thermalSettingRadioButtonOptimized.Checked = false;
                thermalSettingRadioButtonCool.Checked = false;
                thermalSettingRadioButtonQuiet.Checked = false;
                thermalSettingRadioButtonPerformance.Checked = false;
            }
        }

        /// <summary>
        /// Called when the form is closed.
        /// </summary>
        private void FormClosedEventHandler(Object sender, FormClosedEventArgs e)
        {
            _formClosed = true;

            _state.WaitOne();
            _state.BackgroundThreadRunning = false; // Request termination of background thread.
            _state.FormClosed = true;
            _state.Release();
        }

        /// <summary>
        /// Called when the "Restart BG Thread" button is clicked.  Just starts the background thread.
        /// </summary>
        private void RestartBackgroundThreadButtonClickedEventHandler(Object sender, EventArgs e)
        {
            _core.StartBackgroundThread();
        }

        /// <summary>
        /// Called when any of the "thermal setting" radio buttons are clicked.
        /// </summary>
        private void ThermalSettingChangedEventHandler(Object sender, EventArgs e)
        {
            if (thermalSettingRadioButtonOptimized.Checked)
            {
                _core.RequestThermalSetting(ThermalSetting.Optimized);
            }
            else if (thermalSettingRadioButtonCool.Checked)
            {
                _core.RequestThermalSetting(ThermalSetting.Cool);
            }
            else if (thermalSettingRadioButtonQuiet.Checked)
            {
                _core.RequestThermalSetting(ThermalSetting.Quiet);
            }
            else if (thermalSettingRadioButtonPerformance.Checked)
            {
                _core.RequestThermalSetting(ThermalSetting.Performance);
            }
        }

        /// <summary>
        /// Called when the EC fan control on/off radio buttons are clicked.
        /// </summary>
        private void EcFanControlSettingChangedEventHandler(Object sender, EventArgs e)
        {
            if (ecFanControlRadioButtonOn.Checked)
            {
                _core.RequestEcFanControl(true);
                SetFanControlsAvailability(false);
                if (operationModeRadioButtonManual.Checked && _initializationComplete)
                {
                    _configurationStore.SetOption(ConfigurationOption.ManualModeEcFanControlEnabled, 1);
                    _configurationStore.SetOption(ConfigurationOption.ManualModeFan1Level, null);
                    _configurationStore.SetOption(ConfigurationOption.ManualModeFan2Level, null);
                }
            }
            else if (ecFanControlRadioButtonOff.Checked)
            {
                _core.RequestEcFanControl(false);
                if (operationModeRadioButtonManual.Checked)
                {
                    SetFanControlsAvailability(true);
                    _configurationStore.SetOption(ConfigurationOption.ManualModeEcFanControlEnabled, 0);
                }
            }
        }

        /// <summary>
        /// Called when one of the manual fan control level radio buttons is clicked.
        /// </summary>
        private void FanLevelChangedEventHandler(Object sender, EventArgs e)
        {
            // Fan 1.
            FanLevel? fan1LevelRequested = null;
            if (manualFan1RadioButtonOff.Checked)
            {
                fan1LevelRequested = FanLevel.Off;
                if (!_core.IsIndividualFanControlSupported)
                {
                    manualFan2RadioButtonOff.Checked = true;
                }
            }
            else if (manualFan1RadioButtonMedium.Checked)
            {
                fan1LevelRequested = FanLevel.Medium;
                if (!_core.IsIndividualFanControlSupported)
                {
                    manualFan2RadioButtonMedium.Checked = true;
                }
            }
            else if (manualFan1RadioButtonHigh.Checked)
            {
                fan1LevelRequested = FanLevel.High;
                if (!_core.IsIndividualFanControlSupported)
                {
                    manualFan2RadioButtonHigh.Checked = true;
                }
            }

            if (fan1LevelRequested != null)
            {
                _core.RequestFan1Level(fan1LevelRequested);
                _configurationStore.SetOption(ConfigurationOption.ManualModeFan1Level, fan1LevelRequested);
            }

            // Fan 2.
            FanLevel? fan2LevelRequested = null;
            if (manualFan2RadioButtonOff.Checked)
            {
                fan2LevelRequested = FanLevel.Off;
            }
            else if (manualFan2RadioButtonMedium.Checked)
            {
                fan2LevelRequested = FanLevel.Medium;
            }
            else if (manualFan2RadioButtonHigh.Checked)
            {
                fan2LevelRequested = FanLevel.High;
            }

            if (fan2LevelRequested != null && _core.IsIndividualFanControlSupported)
            {
                _core.RequestFan2Level(fan2LevelRequested);
                _configurationStore.SetOption(ConfigurationOption.ManualModeFan2Level, fan2LevelRequested);
            }
        }

        /// <summary>
        /// Called when the consistency mode configuration text boxes are modified.
        /// </summary>
        private void ConsistencyModeTextBoxesChangedEventHandler(Object sender, EventArgs e)
        {
            if (Regex.IsMatch(frequencyTextBox.Text, "[^0-9]"))
            {
                frequencyTextBox.Text = Regex.Replace(frequencyTextBox.Text, "[^0-9]", "");
            }

        }

        /// <summary>
        /// Called when the consistency mode "Apply changes" button is clicked.
        /// </summary>
        private void ConsistencyApplyChangesButtonClickedEventHandler(Object sender, EventArgs e)
        {
            //WriteConsistencyModeConfiguration();
        }

        /// <summary>
        /// Called when the tray icon is clicked. Only shows the context menu, does not restore the window.
        /// </summary>
        private void TrayIconOnClickEventHandler(object sender, EventArgs e)
        {
            // 只显示上下文菜单，不恢复窗口
            // 这样用户必须通过"Dell控制面板"菜单项来恢复窗口
        }

        /// <summary>
        /// Called when the window is resized. If the window is minimized and the "tray icon" is visible, then the
        /// window is hidden in the taskbar.
        /// </summary>
        private void OnResizeEventHandler(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized && trayIcon.Visible)
            {
                ShowInTaskbar = false;
                Visible = false;
            }
        }

        /// <summary>
        /// Load system tray icons.
        /// </summary>
        private void LoadTrayIcons()
        {
            int globalIndex = 0;

            foreach (string color in new string[] { "Grey", "Blue", "Red" })
            {
                for (int index = 1; index <= 16; index++)
                {
                    _trayIcons[globalIndex++] = new Icon(string.Format(@"Resources\Fan-{0}-{1}.ico", color, index));
                }
            }
        }

        /// <summary>
        /// Update the system tray icon.
        /// </summary>
        /// <param name="advance">Whether or not to advance a frame</param>
        private void UpdateTrayIcon(bool advance)
        {
            // Tray icon is always visible
            trayIcon.Visible = true;

            int offset = _core.TrayIconColor switch
            {
                TrayIconColor.Gray => 0,
                TrayIconColor.Blue => 16,
                TrayIconColor.Red => 32,
                _ => 0
            };

            if (advance)
            {
                _trayIconIndex = (_trayIconIndex + 1) % (_trayIcons.Length / 3);
            }

            Icon newIcon = _trayIcons[_trayIconIndex + offset];
            if (trayIcon.Icon != newIcon)
            {
                trayIcon.Icon = newIcon;
            }
        }

        /// <summary>
        /// Update the system tray icon (advance one frame).
        /// </summary>
        private void UpdateTrayIcon()
        {
            UpdateTrayIcon(true);
        }

        /// <summary>
        /// Kicks off the thread that handles the tray icon animation.
        /// </summary>
        private void StartTrayIconThread()
        {
            new Thread(new ThreadStart(TrayIconThread)).Start();
        }

        /// <summary>
        /// Update the tray icon, changing speed with the fan RPM.
        /// </summary>
        private void TrayIconThread()
        {
            try
            {
                MethodInvoker updateInvoker = new(UpdateTrayIcon);

                while (!_formClosed)
                {
                    int waitTime = 1000; // One second.

                    // Always animate
                    uint? averageRpm;
                    if (_state.Fan2Present)
                    {
                        averageRpm = (_state.Fan1Rpm + _state.Fan2Rpm) / 2;
                    }
                    else
                    {
                        averageRpm = _state.Fan1Rpm;
                    }

                    if (averageRpm > 250 && averageRpm < 10000)
                    {
                        try
                        {
                            BeginInvoke(updateInvoker);
                        }
                        catch (Exception)
                        {
                            // If the window handle is not here (not open yet, or closing), there could be an error.
                            // Silently ignore.
                        }

                        // Higher RPM = lower wait time = faster animation.
                        waitTime = 250000 / (int)averageRpm;
                    }

                    Thread.Sleep(Math.Min(waitTime, 1000));
                }
            }
            catch (Exception exception)
            {
                Log.Write(exception);
            }
        }

        /// <summary>
        /// Shows a disclaimer message to the user.
        /// </summary>
        private static void ShowDisclaimer()
        {
            MessageBox.Show("Note: While every has been made to make this program safe to use, it does interact with the embedded controller and system BIOS using undocumented methods and may have adverse effects on your system.  Use at your own risk.  If you experience odd behavior, a full system shutdown should restore everything back to the original state.  This program is not created by or affiliated with Dell Inc. or Dell Technologies Inc.", "Dell Fan Management – Disclaimer");
        }

        private void EppTrackBarScrollEventHandler(object sender, EventArgs e)
        {
            uint result= CpuPowerApi.SetGuidByState(CpuPowerApi.GUID_PROCESSOR_PERFEPP, (uint)eppTrackBar.Value);
            eppLabel.Text = string.Format("EPP: {0}", eppTrackBar.Value);
        }

        private void PowerApplyButtonClickedEventHandler(object sender, EventArgs e)
        {
            CpuPowerApi.SetGuidByState(CpuPowerApi.GUID_PROCESSOR_PERFEPP, (uint)eppTrackBar.Value);
            CpuPowerApi.SetGuidByState(CpuPowerApi.GUID_PROCESSOR_FREQUENCYMAX, uint.Parse(frequencyTextBox.Text));
        }

        /// <summary>
        /// 托盘菜单 - 显示主界面
        /// </summary>
        private void TrayMenuItemShowClickEventHandler(object sender, EventArgs e)
        {
            Visible = true;
            ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
        }

        /// <summary>
        /// 托盘菜单 - 退出程序
        /// </summary>
        private void TrayMenuItemExitClickEventHandler(object sender, EventArgs e)
        {
            // 调用退出处理方法
            ExitApplication();
        }
        
        /// <summary>
        /// 重写WndProc以拦截关闭消息，将其转为最小化
        /// </summary>
        /// <param name="m">Windows消息</param>
        protected override void WndProc(ref Message m)
        {
            const int WM_CLOSE = 0x0010;
            
            if (m.Msg == WM_CLOSE)
            {
                // 最小化到托盘而不是关闭
                WindowState = FormWindowState.Minimized;
                ShowInTaskbar = false;
                Visible = false;
                return;
            }
            
            base.WndProc(ref m);
        }
        
        /// <summary>
        /// 当窗体关闭时的事件处理程序
        /// </summary>
        private void ClosedEventHandler(Object sender, FormClosedEventArgs e)
        {
            ExitApplication();
        }
        
        /// <summary>
        /// 退出应用程序的处理方法
        /// </summary>
        private void ExitApplication()
        {
            _formClosed = true;

            _state.WaitOne();
            _state.BackgroundThreadRunning = false; // Request termination of background thread.
            _state.FormClosed = true;
            _state.Release();
            
            // 确保所有子进程都被终止
            try
            {
                // 获取当前进程的所有子进程
                Process currentProcess = Process.GetCurrentProcess();
                Process[] processes = Process.GetProcesses();
                
                foreach (Process process in processes)
                {
                    // 检查是否是当前进程的子进程
                    if (process.MainModule != null && process.MainModule.FileName == currentProcess.MainModule.FileName)
                    {
                        // 如果是同一个进程的实例，终止它
                        if (process.Id != currentProcess.Id)
                        {
                            try
                            {
                                process.Kill();
                                process.WaitForExit(5000); // 等待最多5秒
                            }
                            catch (Exception)
                            {
                                // 忽略终止失败的进程
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write($"Error terminating child processes: {ex.Message}");
            }
            
            // 退出应用程序
            Application.Exit();
        }
    }
}
