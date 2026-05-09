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
        /// Pre-loaded icons to use in the system tray (only current color theme, 16 icons).
        /// </summary>
        private readonly Icon[] _trayIcons;

        /// <summary>
        /// Next tray icon animation to be displayed.
        /// </summary>
        private int _trayIconIndex;

        /// <summary>
        /// Current tray icon color theme.
        /// </summary>
        private TrayIconColor _currentTrayIconColor;

        /// <summary>
        /// Timer for tray icon animation (replaces background thread).
        /// </summary>
        private System.Windows.Forms.Timer _trayIconTimer;

        /// <summary>
        /// Indicates that the program is closing, so background threads should stop.
        /// </summary>
        private bool _formClosed;

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
            InitializeComponent();

            // Initialize objects.
            _configurationStore = new();
            _state = new State(_configurationStore);
            _core = new Core(_state, this);
            _formClosed = false;

            _trayIcons = new Icon[16];
            _trayIconIndex = 0;
            _currentTrayIconColor = TrayIconColor.Gray;
            LoadTrayIcons(_currentTrayIconColor);

            // Disclaimer.
            if (_configurationStore.GetIntOption(ConfigurationOption.DisclaimerShown) != 1)
            {
                ShowDisclaimer();
                _configurationStore.SetOption(ConfigurationOption.DisclaimerShown, 1);
            }

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

            // ...Fan mode radio buttons...
            fanModeRadioButtonAutomatic.CheckedChanged += new EventHandler(FanModeSettingChangedEventHandler);
            fanModeRadioButtonManual.CheckedChanged += new EventHandler(FanModeSettingChangedEventHandler);

            eppTrackBar.Scroll += new EventHandler(EppTrackBarScrollEventHandler);

            // Empty out pre-populated temperature label text fields.
            temperatureLabel1.Text = string.Empty;
            temperatureLabel2.Text = string.Empty;

            // Fan mode is always available.
            fanModeGroupBox.Enabled = true;

            // Initial update of the tray icon (required for it to appear for display).
            UpdateTrayIcon(false);

            // Initialize power management related UI elements.
            UpdatePowerForm();
            // Update form with default state values.
            UpdateForm();
            _state.UpdateThermalSetting();

            // Apply EC fan control configuration from registry.
            ApplyEcFanControlConfiguration();

            // Initialize thermal setting UI once on startup.
            InitializeThermalSettingUi();

            // Start background work.
            _core.StartBackgroundThread();
            StartTrayIconTimer();

            // Apply acrylic glass effect after the window handle is created.
            if (IsHandleCreated)
            {
                ApplyAcrylicEffect();
            }
            else
            {
                HandleCreated += (s, e) => ApplyAcrylicEffect();
            }
        }

        /// <summary>
        /// Apply acrylic blur-behind effect to the form.
        /// </summary>
        private void ApplyAcrylicEffect()
        {
            // 0xC8FFFFFF = semi-transparent white (200 alpha)
            DwmHelper.EnableAcrylic(Handle, unchecked((int)0xC8FFFFFF));
        }

        /// <summary>
        /// Apply fan mode configuration loaded from the registry.
        /// </summary>
        private void ApplyEcFanControlConfiguration()
        {
            // Temporarily remove event handlers to avoid triggering events during initialization
            fanModeRadioButtonAutomatic.CheckedChanged -= FanModeSettingChangedEventHandler;
            fanModeRadioButtonManual.CheckedChanged -= FanModeSettingChangedEventHandler;

            // 启动时始终默认使用自动模式
            fanModeRadioButtonAutomatic.Checked = true;
            _core.RequestFanMode(FanMode.Automatic);

            // Restore event handlers
            fanModeRadioButtonAutomatic.CheckedChanged += FanModeSettingChangedEventHandler;
            fanModeRadioButtonManual.CheckedChanged += FanModeSettingChangedEventHandler;
        }

        /// <summary>
        /// Initialize thermal setting UI once on startup based on current state.
        /// </summary>
        private void InitializeThermalSettingUi()
        {
            if (_state.FanMode == FanMode.Manual)
            {
                // 手动模式下禁用散热管理，不选中任何选项
                SetThermalSettingAvaiability(false);
                return;
            }

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

        private void UpdatePowerForm()
        {
            if (CpuPowerManager.GetGuidByState(CpuPowerManager.GUID_PROCESSOR_PERFEPP, out uint epp) == 0)
            {
                eppTrackBar.Value = (int)epp;
                eppLabel.Text = string.Format("EPP: {0}", epp);
            }

            if (CpuPowerManager.GetGuidByState(CpuPowerManager.GUID_PROCESSOR_FREQUENCYMAX, out uint frequencyMax) == 0)
            {
                frequencyTextBox.Text = frequencyMax.ToString();
            }
            Console.WriteLine("Finished updating power form.");
        }

        /// <summary>
        /// Update the form based on the current state.
        /// </summary>
        public void UpdateForm()
        {
            _state.WaitOne();

            // Fan RPM.
            fan1RpmLabel.Text = string.Format("Fan 1 RPM: {0}", _state.Fan1Rpm != null ? _state.Fan1Rpm : "(Error)");

            if (_state.Fan2Present)
            {
                fan2RpmLabel.Text = string.Format("Fan 2 RPM: {0}", _state.Fan2Rpm != null ? _state.Fan2Rpm : "(Error)");
                fan2RpmLabel.Enabled = true;
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

            // System monitor data
            cpuFrequencyLabel.Text = _state.CpuFrequency.HasValue
                ? string.Format("CPU 频率: {0:F1} GHz", _state.CpuFrequency.Value / 1000.0)
                : "CPU 频率: --";

            gpuFrequencyLabel.Text = _state.GpuFrequency.HasValue
                ? string.Format("GPU 频率: {0:F1} GHz", _state.GpuFrequency.Value / 1000.0)
                : "GPU 频率: --";

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
                _state.Error = null;
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

                        ThermalSetting? thermalSettingOverride = _configurationStore.GetThermalSettingOverride((Guid)_state.ActivePowerProfile);
                        if (thermalSettingOverride != null)
                        {
                            _core.RequestThermalSetting((ThermalSetting)thermalSettingOverride);
                            Log.Write(string.Format("Thermal setting override: {0}", thermalSettingOverride));
                        }

                        Guid? powerMode = _configurationStore.GetPowerModeOverride((Guid)_state.ActivePowerProfile);
                        if (powerMode != null)
                        {
                            Utility.PowerSetActiveOverlayScheme((Guid)powerMode);
                            Log.Write(string.Format("Power mode overrode: {0}", powerMode));
                        }

                        int? nvPstate = _configurationStore.GetNvPstateOverride((Guid)_state.ActivePowerProfile);
                        if (nvPstate != null)
                        {
                            string nvInspectorPath = _configurationStore.GetStringOption(ConfigurationOption.NVPStateApplicationPath);
                            if (nvInspectorPath != null)
                            {
                                Utility.SetNvidiaGpuPstate(nvInspectorPath, (int)nvPstate);
                                Log.Write(string.Format("NVIDIA P-state override: {0}", nvPstate));
                            }
                        }
                    }
                    _registeredPowerProfile = _state.ActivePowerProfile;
                }
            }

            _state.Release();
        }

        /// <summary>
        /// Enable or disable the thermal setting controls.
        /// </summary>
        /// <param name="enabled">Indicates whether to enable or disable the controls</param>
        private void SetThermalSettingAvaiability(bool enabled)
        {
            thermalSettingGroupBox.Enabled = enabled;
        }

        /// <summary>
        /// Called when the form is closed.
        /// </summary>
        private void FormClosedEventHandler(Object sender, FormClosedEventArgs e)
        {
            _formClosed = true;

            _state.WaitOne();
            _state.BackgroundThreadRunning = false;
            _state.FormClosed = true;
            _state.Release();
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
        /// Called when the fan mode radio buttons are clicked.
        /// </summary>
        private void FanModeSettingChangedEventHandler(Object sender, EventArgs e)
        {
            if (fanModeRadioButtonAutomatic.Checked)
            {
                _core.RequestFanMode(FanMode.Automatic);
                _configurationStore.SetOption(ConfigurationOption.EcFanControlEnabled, 1);
                // 自动模式：启用散热模式选择
                SetThermalSettingAvaiability(true);
                // 恢复用户之前选择的散热模式到UI
                RestoreThermalSettingSelection();
            }
            else if (fanModeRadioButtonManual.Checked)
            {
                _core.RequestFanMode(FanMode.Manual);
                _configurationStore.SetOption(ConfigurationOption.EcFanControlEnabled, 0);
                // 手动模式：禁用散热模式选择（变为灰色）
                SetThermalSettingAvaiability(false);
            }
        }

        /// <summary>
        /// 恢复用户之前选择的散热模式到UI
        /// </summary>
        private void RestoreThermalSettingSelection()
        {
            ThermalSetting? userSetting = _core.UserSelectedThermalSetting;
            if (!userSetting.HasValue)
            {
                return;
            }

            switch (userSetting.Value)
            {
                case ThermalSetting.Optimized:
                    thermalSettingRadioButtonOptimized.Checked = true;
                    break;
                case ThermalSetting.Cool:
                    thermalSettingRadioButtonCool.Checked = true;
                    break;
                case ThermalSetting.Quiet:
                    thermalSettingRadioButtonQuiet.Checked = true;
                    break;
                case ThermalSetting.Performance:
                    thermalSettingRadioButtonPerformance.Checked = true;
                    break;
            }
        }

        /// <summary>
        /// Called when the tray icon is clicked. Only shows the context menu, does not restore the window.
        /// </summary>
        private void TrayIconOnClickEventHandler(object sender, EventArgs e)
        {
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
        /// Load system tray icons for the specified color theme.
        /// Disposes old icons before loading new ones to free memory.
        /// </summary>
        /// <param name="color">Color theme to load</param>
        private void LoadTrayIcons(TrayIconColor color)
        {
            // Dispose old icons to free GDI+ resources.
            for (int i = 0; i < _trayIcons.Length; i++)
            {
                _trayIcons[i]?.Dispose();
                _trayIcons[i] = null;
            }

            string colorName = color switch
            {
                TrayIconColor.Gray => "Grey",
                TrayIconColor.Blue => "Blue",
                TrayIconColor.Red => "Red",
                _ => "Grey"
            };

            for (int index = 1; index <= 16; index++)
            {
                _trayIcons[index - 1] = new Icon(string.Format(@"Resources\Fan-{0}-{1}.ico", colorName, index));
            }

            _currentTrayIconColor = color;
            _trayIconIndex = 0;
        }

        /// <summary>
        /// Update the system tray icon.
        /// </summary>
        /// <param name="advance">Whether or not to advance a frame</param>
        private void UpdateTrayIcon(bool advance)
        {
            trayIcon.Visible = true;

            // If color theme changed, reload icons for the new theme.
            if (_core.TrayIconColor != _currentTrayIconColor)
            {
                LoadTrayIcons(_core.TrayIconColor);
            }

            if (advance)
            {
                _trayIconIndex = (_trayIconIndex + 1) % _trayIcons.Length;
            }

            Icon newIcon = _trayIcons[_trayIconIndex];
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
        /// Starts the timer that handles the tray icon animation.
        /// Replaces the background thread with a UI thread timer for lower resource usage.
        /// </summary>
        private void StartTrayIconTimer()
        {
            _trayIconTimer = new System.Windows.Forms.Timer();
            _trayIconTimer.Tick += TrayIconTimer_Tick;
            _trayIconTimer.Interval = 1000;
            _trayIconTimer.Start();
        }

        /// <summary>
        /// Timer tick handler for tray icon animation.
        /// Adjusts interval based on fan RPM.
        /// </summary>
        private void TrayIconTimer_Tick(object sender, EventArgs e)
        {
            if (_formClosed)
            {
                _trayIconTimer?.Stop();
                return;
            }

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
                UpdateTrayIcon();
                _trayIconTimer.Interval = Math.Min(250000 / (int)averageRpm, 1000);
            }
            else
            {
                _trayIconTimer.Interval = 1000;
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
            uint result = CpuPowerManager.SetGuidByState(CpuPowerManager.GUID_PROCESSOR_PERFEPP, (uint)eppTrackBar.Value);
            eppLabel.Text = string.Format("EPP: {0}", eppTrackBar.Value);
        }

        private void PowerApplyButtonClickedEventHandler(object sender, EventArgs e)
        {
            CpuPowerManager.SetGuidByState(CpuPowerManager.GUID_PROCESSOR_PERFEPP, (uint)eppTrackBar.Value);
            CpuPowerManager.SetGuidByState(CpuPowerManager.GUID_PROCESSOR_FREQUENCYMAX, uint.Parse(frequencyTextBox.Text));
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
            ExitApplication();
        }

        /// <summary>
        /// 重写WndProc以拦截关闭消息，将其转为最小化；并阻止背景绘制以显示Acrylic效果。
        /// </summary>
        /// <param name="m">Windows消息</param>
        protected override void WndProc(ref Message m)
        {
            const int WM_CLOSE = 0x0010;
            const int WM_ERASEBKGND = 0x0014;

            if (m.Msg == WM_CLOSE)
            {
                WindowState = FormWindowState.Minimized;
                ShowInTaskbar = false;
                Visible = false;
                return;
            }

            // 阻止 WinForms 绘制纯色背景，让 DWM Acrylic 效果透出来
            if (m.Msg == WM_ERASEBKGND)
            {
                m.Result = (IntPtr)1;
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
            _state.BackgroundThreadRunning = false;
            _state.FormClosed = true;
            _state.Release();

            try
            {
                Process currentProcess = Process.GetCurrentProcess();
                Process[] processes = Process.GetProcesses();

                foreach (Process process in processes)
                {
                    if (process.MainModule != null && process.MainModule.FileName == currentProcess.MainModule.FileName)
                    {
                        if (process.Id != currentProcess.Id)
                        {
                            try
                            {
                                process.Kill();
                                process.WaitForExit(5000);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write($"Error terminating child processes: {ex.Message}");
            }

            Application.Exit();
        }
    }
}
