
namespace DellFanManagement.App
{
    partial class DellFanManagementGuiForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DellFanManagementGuiForm));
            fansGroupBox = new System.Windows.Forms.GroupBox();
            fan2RpmLabel = new System.Windows.Forms.Label();
            fan1RpmLabel = new System.Windows.Forms.Label();
            thermalSettingGroupBox = new System.Windows.Forms.GroupBox();
            thermalSettingRadioButtonPerformance = new System.Windows.Forms.RadioButton();
            thermalSettingRadioButtonQuiet = new System.Windows.Forms.RadioButton();
            thermalSettingRadioButtonCool = new System.Windows.Forms.RadioButton();
            thermalSettingRadioButtonOptimized = new System.Windows.Forms.RadioButton();
            temperatureGroupBox = new System.Windows.Forms.GroupBox();
            temperatureLabel2 = new System.Windows.Forms.Label();
            temperatureLabel1 = new System.Windows.Forms.Label();
            operationModeGroupBox = new System.Windows.Forms.GroupBox();
            operationModeRadioButtonConsistency = new System.Windows.Forms.RadioButton();
            operationModeRadioButtonManual = new System.Windows.Forms.RadioButton();
            operationModeRadioButtonAutomatic = new System.Windows.Forms.RadioButton();
            manualGroupBox = new System.Windows.Forms.GroupBox();
            manualFan2GroupBox = new System.Windows.Forms.GroupBox();
            manualFan2RadioButtonHigh = new System.Windows.Forms.RadioButton();
            manualFan2RadioButtonMedium = new System.Windows.Forms.RadioButton();
            manualFan2RadioButtonOff = new System.Windows.Forms.RadioButton();
            manualFan1GroupBox = new System.Windows.Forms.GroupBox();
            manualFan1RadioButtonHigh = new System.Windows.Forms.RadioButton();
            manualFan1RadioButtonMedium = new System.Windows.Forms.RadioButton();
            manualFan1RadioButtonOff = new System.Windows.Forms.RadioButton();
            consistencyModeGroupBox = new System.Windows.Forms.GroupBox();
            alertsCheckBox = new System.Windows.Forms.CheckBox();
            consistencyModeApplyChangesButton = new System.Windows.Forms.Button();
            consistencyModeRpmThresholdTextBox = new System.Windows.Forms.TextBox();
            consistencyModeRpmThresholdLabel = new System.Windows.Forms.Label();
            consistencyModeUpperTemperatureThresholdTextBox = new System.Windows.Forms.TextBox();
            consistencyModeUpperTemperatureThresholdLabel = new System.Windows.Forms.Label();
            consistencyModeLowerTemperatureThresholdTextBox = new System.Windows.Forms.TextBox();
            consistencyModeLowerTemperatureThresholdLabel = new System.Windows.Forms.Label();
            ecFanControlRadioButtonOn = new System.Windows.Forms.RadioButton();
            ecFanControlRadioButtonOff = new System.Windows.Forms.RadioButton();
            ecFanControlGroupBox = new System.Windows.Forms.GroupBox();
            restartBackgroundThreadButton = new System.Windows.Forms.Button();
            statusStrip = new System.Windows.Forms.StatusStrip();
            consistencyModeStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            trayIcon = new System.Windows.Forms.NotifyIcon(components);
            trayIconCheckBox = new System.Windows.Forms.CheckBox();
            animatedCheckBox = new System.Windows.Forms.CheckBox();
            fansGroupBox.SuspendLayout();
            thermalSettingGroupBox.SuspendLayout();
            temperatureGroupBox.SuspendLayout();
            operationModeGroupBox.SuspendLayout();
            manualGroupBox.SuspendLayout();
            manualFan2GroupBox.SuspendLayout();
            manualFan1GroupBox.SuspendLayout();
            consistencyModeGroupBox.SuspendLayout();
            ecFanControlGroupBox.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // fansGroupBox
            // 
            fansGroupBox.Controls.Add(fan2RpmLabel);
            fansGroupBox.Controls.Add(fan1RpmLabel);
            fansGroupBox.Location = new System.Drawing.Point(24, 24);
            fansGroupBox.Margin = new System.Windows.Forms.Padding(6);
            fansGroupBox.Name = "fansGroupBox";
            fansGroupBox.Padding = new System.Windows.Forms.Padding(6);
            fansGroupBox.Size = new System.Drawing.Size(446, 130);
            fansGroupBox.TabIndex = 1;
            fansGroupBox.TabStop = false;
            fansGroupBox.Text = "风扇:";
            // 
            // fan2RpmLabel
            // 
            fan2RpmLabel.AutoSize = true;
            fan2RpmLabel.Location = new System.Drawing.Point(12, 76);
            fan2RpmLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            fan2RpmLabel.Name = "fan2RpmLabel";
            fan2RpmLabel.Size = new System.Drawing.Size(330, 31);
            fan2RpmLabel.TabIndex = 2;
            fan2RpmLabel.Text = "Fan 2 RPM: (Not measured)";
            // 
            // fan1RpmLabel
            // 
            fan1RpmLabel.AutoSize = true;
            fan1RpmLabel.Location = new System.Drawing.Point(12, 38);
            fan1RpmLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            fan1RpmLabel.Name = "fan1RpmLabel";
            fan1RpmLabel.Size = new System.Drawing.Size(330, 31);
            fan1RpmLabel.TabIndex = 1;
            fan1RpmLabel.Text = "Fan 1 RPM: (Not measured)";
            // 
            // thermalSettingGroupBox
            // 
            thermalSettingGroupBox.Controls.Add(thermalSettingRadioButtonPerformance);
            thermalSettingGroupBox.Controls.Add(thermalSettingRadioButtonQuiet);
            thermalSettingGroupBox.Controls.Add(thermalSettingRadioButtonCool);
            thermalSettingGroupBox.Controls.Add(thermalSettingRadioButtonOptimized);
            thermalSettingGroupBox.Location = new System.Drawing.Point(700, 234);
            thermalSettingGroupBox.Margin = new System.Windows.Forms.Padding(6);
            thermalSettingGroupBox.Name = "thermalSettingGroupBox";
            thermalSettingGroupBox.Padding = new System.Windows.Forms.Padding(6);
            thermalSettingGroupBox.Size = new System.Drawing.Size(222, 248);
            thermalSettingGroupBox.TabIndex = 6;
            thermalSettingGroupBox.TabStop = false;
            thermalSettingGroupBox.Text = "散热管理:";
            // 
            // thermalSettingRadioButtonPerformance
            // 
            thermalSettingRadioButtonPerformance.AutoSize = true;
            thermalSettingRadioButtonPerformance.Location = new System.Drawing.Point(12, 194);
            thermalSettingRadioButtonPerformance.Margin = new System.Windows.Forms.Padding(6);
            thermalSettingRadioButtonPerformance.Name = "thermalSettingRadioButtonPerformance";
            thermalSettingRadioButtonPerformance.Size = new System.Drawing.Size(93, 35);
            thermalSettingRadioButtonPerformance.TabIndex = 3;
            thermalSettingRadioButtonPerformance.TabStop = true;
            thermalSettingRadioButtonPerformance.Text = "极速";
            thermalSettingRadioButtonPerformance.UseVisualStyleBackColor = true;
            // 
            // thermalSettingRadioButtonQuiet
            // 
            thermalSettingRadioButtonQuiet.AutoSize = true;
            thermalSettingRadioButtonQuiet.Location = new System.Drawing.Point(12, 144);
            thermalSettingRadioButtonQuiet.Margin = new System.Windows.Forms.Padding(6);
            thermalSettingRadioButtonQuiet.Name = "thermalSettingRadioButtonQuiet";
            thermalSettingRadioButtonQuiet.Size = new System.Drawing.Size(93, 35);
            thermalSettingRadioButtonQuiet.TabIndex = 2;
            thermalSettingRadioButtonQuiet.TabStop = true;
            thermalSettingRadioButtonQuiet.Text = "静音";
            thermalSettingRadioButtonQuiet.UseVisualStyleBackColor = true;
            // 
            // thermalSettingRadioButtonCool
            // 
            thermalSettingRadioButtonCool.AutoSize = true;
            thermalSettingRadioButtonCool.Location = new System.Drawing.Point(12, 94);
            thermalSettingRadioButtonCool.Margin = new System.Windows.Forms.Padding(6);
            thermalSettingRadioButtonCool.Name = "thermalSettingRadioButtonCool";
            thermalSettingRadioButtonCool.Size = new System.Drawing.Size(93, 35);
            thermalSettingRadioButtonCool.TabIndex = 1;
            thermalSettingRadioButtonCool.TabStop = true;
            thermalSettingRadioButtonCool.Text = "酷凉";
            thermalSettingRadioButtonCool.UseVisualStyleBackColor = true;
            // 
            // thermalSettingRadioButtonOptimized
            // 
            thermalSettingRadioButtonOptimized.AutoSize = true;
            thermalSettingRadioButtonOptimized.Location = new System.Drawing.Point(12, 44);
            thermalSettingRadioButtonOptimized.Margin = new System.Windows.Forms.Padding(6);
            thermalSettingRadioButtonOptimized.Name = "thermalSettingRadioButtonOptimized";
            thermalSettingRadioButtonOptimized.Size = new System.Drawing.Size(93, 35);
            thermalSettingRadioButtonOptimized.TabIndex = 0;
            thermalSettingRadioButtonOptimized.TabStop = true;
            thermalSettingRadioButtonOptimized.Text = "优化";
            thermalSettingRadioButtonOptimized.UseVisualStyleBackColor = true;
            // 
            // temperatureGroupBox
            // 
            temperatureGroupBox.Controls.Add(temperatureLabel2);
            temperatureGroupBox.Controls.Add(temperatureLabel1);
            temperatureGroupBox.Location = new System.Drawing.Point(24, 166);
            temperatureGroupBox.Margin = new System.Windows.Forms.Padding(6);
            temperatureGroupBox.Name = "temperatureGroupBox";
            temperatureGroupBox.Padding = new System.Windows.Forms.Padding(6);
            temperatureGroupBox.Size = new System.Drawing.Size(664, 398);
            temperatureGroupBox.TabIndex = 4;
            temperatureGroupBox.TabStop = false;
            temperatureGroupBox.Text = "温度:";
            // 
            // temperatureLabel2
            // 
            temperatureLabel2.AutoSize = true;
            temperatureLabel2.Location = new System.Drawing.Point(12, 84);
            temperatureLabel2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            temperatureLabel2.Name = "temperatureLabel2";
            temperatureLabel2.Size = new System.Drawing.Size(209, 31);
            temperatureLabel2.TabIndex = 1;
            temperatureLabel2.Text = "GPU: 50 (50-100)";
            // 
            // temperatureLabel1
            // 
            temperatureLabel1.AutoSize = true;
            temperatureLabel1.Location = new System.Drawing.Point(12, 46);
            temperatureLabel1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            temperatureLabel1.Name = "temperatureLabel1";
            temperatureLabel1.Size = new System.Drawing.Size(207, 31);
            temperatureLabel1.TabIndex = 0;
            temperatureLabel1.Text = "CPU: 50 (50-100)";
            // 
            // operationModeGroupBox
            // 
            operationModeGroupBox.Controls.Add(operationModeRadioButtonConsistency);
            operationModeGroupBox.Controls.Add(operationModeRadioButtonManual);
            operationModeGroupBox.Controls.Add(operationModeRadioButtonAutomatic);
            operationModeGroupBox.Location = new System.Drawing.Point(700, 24);
            operationModeGroupBox.Margin = new System.Windows.Forms.Padding(6);
            operationModeGroupBox.Name = "operationModeGroupBox";
            operationModeGroupBox.Padding = new System.Windows.Forms.Padding(6);
            operationModeGroupBox.Size = new System.Drawing.Size(222, 198);
            operationModeGroupBox.TabIndex = 5;
            operationModeGroupBox.TabStop = false;
            operationModeGroupBox.Text = "操作模式:";
            // 
            // operationModeRadioButtonConsistency
            // 
            operationModeRadioButtonConsistency.AutoSize = true;
            operationModeRadioButtonConsistency.Location = new System.Drawing.Point(12, 144);
            operationModeRadioButtonConsistency.Margin = new System.Windows.Forms.Padding(6);
            operationModeRadioButtonConsistency.Name = "operationModeRadioButtonConsistency";
            operationModeRadioButtonConsistency.Size = new System.Drawing.Size(117, 35);
            operationModeRadioButtonConsistency.TabIndex = 2;
            operationModeRadioButtonConsistency.TabStop = true;
            operationModeRadioButtonConsistency.Text = "一致性";
            operationModeRadioButtonConsistency.UseVisualStyleBackColor = true;
            // 
            // operationModeRadioButtonManual
            // 
            operationModeRadioButtonManual.AutoSize = true;
            operationModeRadioButtonManual.Location = new System.Drawing.Point(12, 94);
            operationModeRadioButtonManual.Margin = new System.Windows.Forms.Padding(6);
            operationModeRadioButtonManual.Name = "operationModeRadioButtonManual";
            operationModeRadioButtonManual.Size = new System.Drawing.Size(93, 35);
            operationModeRadioButtonManual.TabIndex = 1;
            operationModeRadioButtonManual.TabStop = true;
            operationModeRadioButtonManual.Text = "手动";
            operationModeRadioButtonManual.UseVisualStyleBackColor = true;
            // 
            // operationModeRadioButtonAutomatic
            // 
            operationModeRadioButtonAutomatic.AutoSize = true;
            operationModeRadioButtonAutomatic.Location = new System.Drawing.Point(12, 44);
            operationModeRadioButtonAutomatic.Margin = new System.Windows.Forms.Padding(6);
            operationModeRadioButtonAutomatic.Name = "operationModeRadioButtonAutomatic";
            operationModeRadioButtonAutomatic.Size = new System.Drawing.Size(93, 35);
            operationModeRadioButtonAutomatic.TabIndex = 0;
            operationModeRadioButtonAutomatic.TabStop = true;
            operationModeRadioButtonAutomatic.Text = "自动";
            operationModeRadioButtonAutomatic.UseVisualStyleBackColor = true;
            operationModeRadioButtonAutomatic.CheckedChanged += operationModeRadioButtonAutomatic_CheckedChanged;
            // 
            // manualGroupBox
            // 
            manualGroupBox.Controls.Add(manualFan2GroupBox);
            manualGroupBox.Controls.Add(manualFan1GroupBox);
            manualGroupBox.Location = new System.Drawing.Point(934, 24);
            manualGroupBox.Margin = new System.Windows.Forms.Padding(6);
            manualGroupBox.Name = "manualGroupBox";
            manualGroupBox.Padding = new System.Windows.Forms.Padding(6);
            manualGroupBox.Size = new System.Drawing.Size(388, 252);
            manualGroupBox.TabIndex = 9;
            manualGroupBox.TabStop = false;
            manualGroupBox.Text = "手动控制:";
            // 
            // manualFan2GroupBox
            // 
            manualFan2GroupBox.Controls.Add(manualFan2RadioButtonHigh);
            manualFan2GroupBox.Controls.Add(manualFan2RadioButtonMedium);
            manualFan2GroupBox.Controls.Add(manualFan2RadioButtonOff);
            manualFan2GroupBox.Location = new System.Drawing.Point(200, 42);
            manualFan2GroupBox.Margin = new System.Windows.Forms.Padding(6);
            manualFan2GroupBox.Name = "manualFan2GroupBox";
            manualFan2GroupBox.Padding = new System.Windows.Forms.Padding(6);
            manualFan2GroupBox.Size = new System.Drawing.Size(176, 198);
            manualFan2GroupBox.TabIndex = 1;
            manualFan2GroupBox.TabStop = false;
            manualFan2GroupBox.Text = "风扇 2:";
            manualFan2GroupBox.Enter += manualFan2GroupBox_Enter;
            // 
            // manualFan2RadioButtonHigh
            // 
            manualFan2RadioButtonHigh.AutoSize = true;
            manualFan2RadioButtonHigh.Location = new System.Drawing.Point(12, 144);
            manualFan2RadioButtonHigh.Margin = new System.Windows.Forms.Padding(6);
            manualFan2RadioButtonHigh.Name = "manualFan2RadioButtonHigh";
            manualFan2RadioButtonHigh.Size = new System.Drawing.Size(69, 35);
            manualFan2RadioButtonHigh.TabIndex = 5;
            manualFan2RadioButtonHigh.TabStop = true;
            manualFan2RadioButtonHigh.Text = "高";
            manualFan2RadioButtonHigh.UseVisualStyleBackColor = true;
            // 
            // manualFan2RadioButtonMedium
            // 
            manualFan2RadioButtonMedium.AutoSize = true;
            manualFan2RadioButtonMedium.Location = new System.Drawing.Point(12, 94);
            manualFan2RadioButtonMedium.Margin = new System.Windows.Forms.Padding(6);
            manualFan2RadioButtonMedium.Name = "manualFan2RadioButtonMedium";
            manualFan2RadioButtonMedium.Size = new System.Drawing.Size(69, 35);
            manualFan2RadioButtonMedium.TabIndex = 5;
            manualFan2RadioButtonMedium.TabStop = true;
            manualFan2RadioButtonMedium.Text = "中";
            manualFan2RadioButtonMedium.UseVisualStyleBackColor = true;
            // 
            // manualFan2RadioButtonOff
            // 
            manualFan2RadioButtonOff.AutoSize = true;
            manualFan2RadioButtonOff.Location = new System.Drawing.Point(12, 44);
            manualFan2RadioButtonOff.Margin = new System.Windows.Forms.Padding(6);
            manualFan2RadioButtonOff.Name = "manualFan2RadioButtonOff";
            manualFan2RadioButtonOff.Size = new System.Drawing.Size(69, 35);
            manualFan2RadioButtonOff.TabIndex = 5;
            manualFan2RadioButtonOff.TabStop = true;
            manualFan2RadioButtonOff.Text = "关";
            manualFan2RadioButtonOff.UseVisualStyleBackColor = true;
            // 
            // manualFan1GroupBox
            // 
            manualFan1GroupBox.Controls.Add(manualFan1RadioButtonHigh);
            manualFan1GroupBox.Controls.Add(manualFan1RadioButtonMedium);
            manualFan1GroupBox.Controls.Add(manualFan1RadioButtonOff);
            manualFan1GroupBox.Location = new System.Drawing.Point(12, 42);
            manualFan1GroupBox.Margin = new System.Windows.Forms.Padding(6);
            manualFan1GroupBox.Name = "manualFan1GroupBox";
            manualFan1GroupBox.Padding = new System.Windows.Forms.Padding(6);
            manualFan1GroupBox.Size = new System.Drawing.Size(176, 198);
            manualFan1GroupBox.TabIndex = 0;
            manualFan1GroupBox.TabStop = false;
            manualFan1GroupBox.Text = "风扇 1:";
            // 
            // manualFan1RadioButtonHigh
            // 
            manualFan1RadioButtonHigh.AutoSize = true;
            manualFan1RadioButtonHigh.Location = new System.Drawing.Point(12, 144);
            manualFan1RadioButtonHigh.Margin = new System.Windows.Forms.Padding(6);
            manualFan1RadioButtonHigh.Name = "manualFan1RadioButtonHigh";
            manualFan1RadioButtonHigh.Size = new System.Drawing.Size(69, 35);
            manualFan1RadioButtonHigh.TabIndex = 2;
            manualFan1RadioButtonHigh.TabStop = true;
            manualFan1RadioButtonHigh.Text = "高";
            manualFan1RadioButtonHigh.UseVisualStyleBackColor = true;
            // 
            // manualFan1RadioButtonMedium
            // 
            manualFan1RadioButtonMedium.AutoSize = true;
            manualFan1RadioButtonMedium.Location = new System.Drawing.Point(12, 94);
            manualFan1RadioButtonMedium.Margin = new System.Windows.Forms.Padding(6);
            manualFan1RadioButtonMedium.Name = "manualFan1RadioButtonMedium";
            manualFan1RadioButtonMedium.Size = new System.Drawing.Size(69, 35);
            manualFan1RadioButtonMedium.TabIndex = 1;
            manualFan1RadioButtonMedium.TabStop = true;
            manualFan1RadioButtonMedium.Text = "中";
            manualFan1RadioButtonMedium.UseVisualStyleBackColor = true;
            // 
            // manualFan1RadioButtonOff
            // 
            manualFan1RadioButtonOff.AutoSize = true;
            manualFan1RadioButtonOff.Location = new System.Drawing.Point(12, 44);
            manualFan1RadioButtonOff.Margin = new System.Windows.Forms.Padding(6);
            manualFan1RadioButtonOff.Name = "manualFan1RadioButtonOff";
            manualFan1RadioButtonOff.Size = new System.Drawing.Size(69, 35);
            manualFan1RadioButtonOff.TabIndex = 0;
            manualFan1RadioButtonOff.TabStop = true;
            manualFan1RadioButtonOff.Text = "关";
            manualFan1RadioButtonOff.UseVisualStyleBackColor = true;
            // 
            // consistencyModeGroupBox
            // 
            consistencyModeGroupBox.Controls.Add(alertsCheckBox);
            consistencyModeGroupBox.Controls.Add(consistencyModeApplyChangesButton);
            consistencyModeGroupBox.Controls.Add(consistencyModeRpmThresholdTextBox);
            consistencyModeGroupBox.Controls.Add(consistencyModeRpmThresholdLabel);
            consistencyModeGroupBox.Controls.Add(consistencyModeUpperTemperatureThresholdTextBox);
            consistencyModeGroupBox.Controls.Add(consistencyModeUpperTemperatureThresholdLabel);
            consistencyModeGroupBox.Controls.Add(consistencyModeLowerTemperatureThresholdTextBox);
            consistencyModeGroupBox.Controls.Add(consistencyModeLowerTemperatureThresholdLabel);
            consistencyModeGroupBox.Location = new System.Drawing.Point(934, 288);
            consistencyModeGroupBox.Margin = new System.Windows.Forms.Padding(6);
            consistencyModeGroupBox.Name = "consistencyModeGroupBox";
            consistencyModeGroupBox.Padding = new System.Windows.Forms.Padding(6);
            consistencyModeGroupBox.Size = new System.Drawing.Size(388, 276);
            consistencyModeGroupBox.TabIndex = 10;
            consistencyModeGroupBox.TabStop = false;
            consistencyModeGroupBox.Text = "一致性选项:";
            // 
            // alertsCheckBox
            // 
            alertsCheckBox.AutoSize = true;
            alertsCheckBox.Enabled = false;
            alertsCheckBox.Location = new System.Drawing.Point(274, 160);
            alertsCheckBox.Margin = new System.Windows.Forms.Padding(6);
            alertsCheckBox.Name = "alertsCheckBox";
            alertsCheckBox.Size = new System.Drawing.Size(94, 35);
            alertsCheckBox.TabIndex = 13;
            alertsCheckBox.Text = "警告";
            alertsCheckBox.UseVisualStyleBackColor = true;
            alertsCheckBox.CheckedChanged += alertsCheckBox_CheckedChanged;
            // 
            // consistencyModeApplyChangesButton
            // 
            consistencyModeApplyChangesButton.Location = new System.Drawing.Point(10, 214);
            consistencyModeApplyChangesButton.Margin = new System.Windows.Forms.Padding(6);
            consistencyModeApplyChangesButton.Name = "consistencyModeApplyChangesButton";
            consistencyModeApplyChangesButton.Size = new System.Drawing.Size(368, 50);
            consistencyModeApplyChangesButton.TabIndex = 14;
            consistencyModeApplyChangesButton.Text = "应用";
            consistencyModeApplyChangesButton.UseVisualStyleBackColor = true;
            // 
            // consistencyModeRpmThresholdTextBox
            // 
            consistencyModeRpmThresholdTextBox.Location = new System.Drawing.Point(142, 156);
            consistencyModeRpmThresholdTextBox.Margin = new System.Windows.Forms.Padding(6);
            consistencyModeRpmThresholdTextBox.MaxLength = 4;
            consistencyModeRpmThresholdTextBox.Name = "consistencyModeRpmThresholdTextBox";
            consistencyModeRpmThresholdTextBox.Size = new System.Drawing.Size(106, 38);
            consistencyModeRpmThresholdTextBox.TabIndex = 5;
            consistencyModeRpmThresholdTextBox.Text = "2400";
            // 
            // consistencyModeRpmThresholdLabel
            // 
            consistencyModeRpmThresholdLabel.AutoSize = true;
            consistencyModeRpmThresholdLabel.Location = new System.Drawing.Point(14, 162);
            consistencyModeRpmThresholdLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            consistencyModeRpmThresholdLabel.Name = "consistencyModeRpmThresholdLabel";
            consistencyModeRpmThresholdLabel.Size = new System.Drawing.Size(116, 31);
            consistencyModeRpmThresholdLabel.TabIndex = 4;
            consistencyModeRpmThresholdLabel.Text = "转速阈值:";
            // 
            // consistencyModeUpperTemperatureThresholdTextBox
            // 
            consistencyModeUpperTemperatureThresholdTextBox.Location = new System.Drawing.Point(212, 98);
            consistencyModeUpperTemperatureThresholdTextBox.Margin = new System.Windows.Forms.Padding(6);
            consistencyModeUpperTemperatureThresholdTextBox.MaxLength = 2;
            consistencyModeUpperTemperatureThresholdTextBox.Name = "consistencyModeUpperTemperatureThresholdTextBox";
            consistencyModeUpperTemperatureThresholdTextBox.Size = new System.Drawing.Size(160, 38);
            consistencyModeUpperTemperatureThresholdTextBox.TabIndex = 3;
            consistencyModeUpperTemperatureThresholdTextBox.Text = "85";
            // 
            // consistencyModeUpperTemperatureThresholdLabel
            // 
            consistencyModeUpperTemperatureThresholdLabel.AutoSize = true;
            consistencyModeUpperTemperatureThresholdLabel.Location = new System.Drawing.Point(12, 104);
            consistencyModeUpperTemperatureThresholdLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            consistencyModeUpperTemperatureThresholdLabel.Name = "consistencyModeUpperTemperatureThresholdLabel";
            consistencyModeUpperTemperatureThresholdLabel.Size = new System.Drawing.Size(116, 31);
            consistencyModeUpperTemperatureThresholdLabel.TabIndex = 2;
            consistencyModeUpperTemperatureThresholdLabel.Text = "温度上限:";
            // 
            // consistencyModeLowerTemperatureThresholdTextBox
            // 
            consistencyModeLowerTemperatureThresholdTextBox.Location = new System.Drawing.Point(212, 40);
            consistencyModeLowerTemperatureThresholdTextBox.Margin = new System.Windows.Forms.Padding(6);
            consistencyModeLowerTemperatureThresholdTextBox.MaxLength = 2;
            consistencyModeLowerTemperatureThresholdTextBox.Name = "consistencyModeLowerTemperatureThresholdTextBox";
            consistencyModeLowerTemperatureThresholdTextBox.Size = new System.Drawing.Size(160, 38);
            consistencyModeLowerTemperatureThresholdTextBox.TabIndex = 1;
            consistencyModeLowerTemperatureThresholdTextBox.Text = "65";
            consistencyModeLowerTemperatureThresholdTextBox.TextChanged += consistencyModeLowerTemperatureThresholdTextBox_TextChanged;
            // 
            // consistencyModeLowerTemperatureThresholdLabel
            // 
            consistencyModeLowerTemperatureThresholdLabel.AutoSize = true;
            consistencyModeLowerTemperatureThresholdLabel.Location = new System.Drawing.Point(12, 46);
            consistencyModeLowerTemperatureThresholdLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            consistencyModeLowerTemperatureThresholdLabel.Name = "consistencyModeLowerTemperatureThresholdLabel";
            consistencyModeLowerTemperatureThresholdLabel.Size = new System.Drawing.Size(116, 31);
            consistencyModeLowerTemperatureThresholdLabel.TabIndex = 0;
            consistencyModeLowerTemperatureThresholdLabel.Text = "温度下限:";
            consistencyModeLowerTemperatureThresholdLabel.Click += consistencyModeLowerTemperatureThresholdLabel_Click;
            // 
            // ecFanControlRadioButtonOn
            // 
            ecFanControlRadioButtonOn.AutoSize = true;
            ecFanControlRadioButtonOn.Location = new System.Drawing.Point(12, 44);
            ecFanControlRadioButtonOn.Margin = new System.Windows.Forms.Padding(6);
            ecFanControlRadioButtonOn.Name = "ecFanControlRadioButtonOn";
            ecFanControlRadioButtonOn.Size = new System.Drawing.Size(69, 35);
            ecFanControlRadioButtonOn.TabIndex = 2;
            ecFanControlRadioButtonOn.TabStop = true;
            ecFanControlRadioButtonOn.Text = "开";
            ecFanControlRadioButtonOn.UseVisualStyleBackColor = true;
            // 
            // ecFanControlRadioButtonOff
            // 
            ecFanControlRadioButtonOff.AutoSize = true;
            ecFanControlRadioButtonOff.Location = new System.Drawing.Point(106, 44);
            ecFanControlRadioButtonOff.Margin = new System.Windows.Forms.Padding(6);
            ecFanControlRadioButtonOff.Name = "ecFanControlRadioButtonOff";
            ecFanControlRadioButtonOff.Size = new System.Drawing.Size(69, 35);
            ecFanControlRadioButtonOff.TabIndex = 3;
            ecFanControlRadioButtonOff.TabStop = true;
            ecFanControlRadioButtonOff.Text = "关";
            ecFanControlRadioButtonOff.UseVisualStyleBackColor = true;
            // 
            // ecFanControlGroupBox
            // 
            ecFanControlGroupBox.Controls.Add(ecFanControlRadioButtonOn);
            ecFanControlGroupBox.Controls.Add(ecFanControlRadioButtonOff);
            ecFanControlGroupBox.Location = new System.Drawing.Point(482, 24);
            ecFanControlGroupBox.Margin = new System.Windows.Forms.Padding(6);
            ecFanControlGroupBox.Name = "ecFanControlGroupBox";
            ecFanControlGroupBox.Padding = new System.Windows.Forms.Padding(6);
            ecFanControlGroupBox.Size = new System.Drawing.Size(206, 96);
            ecFanControlGroupBox.TabIndex = 2;
            ecFanControlGroupBox.TabStop = false;
            ecFanControlGroupBox.Text = "EC 风扇控制:";
            // 
            // restartBackgroundThreadButton
            // 
            restartBackgroundThreadButton.Font = new System.Drawing.Font("Segoe UI", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            restartBackgroundThreadButton.Location = new System.Drawing.Point(480, 132);
            restartBackgroundThreadButton.Margin = new System.Windows.Forms.Padding(6);
            restartBackgroundThreadButton.Name = "restartBackgroundThreadButton";
            restartBackgroundThreadButton.Size = new System.Drawing.Size(210, 42);
            restartBackgroundThreadButton.TabIndex = 3;
            restartBackgroundThreadButton.Text = "重置线程";
            restartBackgroundThreadButton.UseVisualStyleBackColor = true;
            // 
            // statusStrip
            // 
            statusStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { consistencyModeStatusLabel });
            statusStrip.Location = new System.Drawing.Point(0, 589);
            statusStrip.Name = "statusStrip";
            statusStrip.Padding = new System.Windows.Forms.Padding(2, 0, 28, 0);
            statusStrip.Size = new System.Drawing.Size(1345, 41);
            statusStrip.SizingGrip = false;
            statusStrip.TabIndex = 13;
            // 
            // consistencyModeStatusLabel
            // 
            consistencyModeStatusLabel.Name = "consistencyModeStatusLabel";
            consistencyModeStatusLabel.Size = new System.Drawing.Size(237, 31);
            consistencyModeStatusLabel.Text = "Fan speed is locked";
            // 
            // trayIcon
            // 
            trayIcon.Text = "Dell Fan Management";
            // 
            // trayIconCheckBox
            // 
            trayIconCheckBox.AutoSize = true;
            trayIconCheckBox.Checked = true;
            trayIconCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            trayIconCheckBox.Location = new System.Drawing.Point(712, 492);
            trayIconCheckBox.Margin = new System.Windows.Forms.Padding(6);
            trayIconCheckBox.Name = "trayIconCheckBox";
            trayIconCheckBox.Size = new System.Drawing.Size(142, 35);
            trayIconCheckBox.TabIndex = 7;
            trayIconCheckBox.Text = "托盘图标";
            trayIconCheckBox.UseVisualStyleBackColor = true;
            // 
            // animatedCheckBox
            // 
            animatedCheckBox.AutoSize = true;
            animatedCheckBox.Checked = true;
            animatedCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            animatedCheckBox.Location = new System.Drawing.Point(712, 532);
            animatedCheckBox.Margin = new System.Windows.Forms.Padding(6);
            animatedCheckBox.Name = "animatedCheckBox";
            animatedCheckBox.Size = new System.Drawing.Size(94, 35);
            animatedCheckBox.TabIndex = 8;
            animatedCheckBox.Text = "动画";
            animatedCheckBox.UseVisualStyleBackColor = true;
            // 
            // DellFanManagementGuiForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(1345, 630);
            Controls.Add(animatedCheckBox);
            Controls.Add(trayIconCheckBox);
            Controls.Add(statusStrip);
            Controls.Add(restartBackgroundThreadButton);
            Controls.Add(ecFanControlGroupBox);
            Controls.Add(consistencyModeGroupBox);
            Controls.Add(manualGroupBox);
            Controls.Add(operationModeGroupBox);
            Controls.Add(temperatureGroupBox);
            Controls.Add(thermalSettingGroupBox);
            Controls.Add(fansGroupBox);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(6);
            MaximizeBox = false;
            Name = "DellFanManagementGuiForm";
            Text = "Dell Fan Management";
            fansGroupBox.ResumeLayout(false);
            fansGroupBox.PerformLayout();
            thermalSettingGroupBox.ResumeLayout(false);
            thermalSettingGroupBox.PerformLayout();
            temperatureGroupBox.ResumeLayout(false);
            temperatureGroupBox.PerformLayout();
            operationModeGroupBox.ResumeLayout(false);
            operationModeGroupBox.PerformLayout();
            manualGroupBox.ResumeLayout(false);
            manualFan2GroupBox.ResumeLayout(false);
            manualFan2GroupBox.PerformLayout();
            manualFan1GroupBox.ResumeLayout(false);
            manualFan1GroupBox.PerformLayout();
            consistencyModeGroupBox.ResumeLayout(false);
            consistencyModeGroupBox.PerformLayout();
            ecFanControlGroupBox.ResumeLayout(false);
            ecFanControlGroupBox.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox fansGroupBox;
        private System.Windows.Forms.Label fan1RpmLabel;
        private System.Windows.Forms.Label fan2RpmLabel;
        private System.Windows.Forms.GroupBox thermalSettingGroupBox;
        private System.Windows.Forms.RadioButton thermalSettingRadioButtonPerformance;
        private System.Windows.Forms.RadioButton thermalSettingRadioButtonQuiet;
        private System.Windows.Forms.RadioButton thermalSettingRadioButtonCool;
        private System.Windows.Forms.RadioButton thermalSettingRadioButtonOptimized;
        private System.Windows.Forms.GroupBox temperatureGroupBox;
        private System.Windows.Forms.Label temperatureLabel2;
        private System.Windows.Forms.Label temperatureLabel1;
        private System.Windows.Forms.GroupBox operationModeGroupBox;
        private System.Windows.Forms.RadioButton operationModeRadioButtonConsistency;
        private System.Windows.Forms.RadioButton operationModeRadioButtonManual;
        private System.Windows.Forms.RadioButton operationModeRadioButtonAutomatic;
        private System.Windows.Forms.GroupBox manualGroupBox;
        private System.Windows.Forms.GroupBox manualFan2GroupBox;
        private System.Windows.Forms.RadioButton manualFan2RadioButtonHigh;
        private System.Windows.Forms.RadioButton manualFan2RadioButtonMedium;
        private System.Windows.Forms.RadioButton manualFan2RadioButtonOff;
        private System.Windows.Forms.GroupBox manualFan1GroupBox;
        private System.Windows.Forms.RadioButton manualFan1RadioButtonHigh;
        private System.Windows.Forms.RadioButton manualFan1RadioButtonMedium;
        private System.Windows.Forms.RadioButton manualFan1RadioButtonOff;
        private System.Windows.Forms.GroupBox consistencyModeGroupBox;
        private System.Windows.Forms.Button consistencyModeApplyChangesButton;
        private System.Windows.Forms.TextBox consistencyModeRpmThresholdTextBox;
        private System.Windows.Forms.Label consistencyModeRpmThresholdLabel;
        private System.Windows.Forms.TextBox consistencyModeUpperTemperatureThresholdTextBox;
        private System.Windows.Forms.Label consistencyModeUpperTemperatureThresholdLabel;
        private System.Windows.Forms.TextBox consistencyModeLowerTemperatureThresholdTextBox;
        private System.Windows.Forms.Label consistencyModeLowerTemperatureThresholdLabel;
        private System.Windows.Forms.RadioButton ecFanControlRadioButtonOn;
        private System.Windows.Forms.RadioButton ecFanControlRadioButtonOff;
        private System.Windows.Forms.GroupBox ecFanControlGroupBox;
        private System.Windows.Forms.Button restartBackgroundThreadButton;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel consistencyModeStatusLabel;
        private System.Windows.Forms.NotifyIcon trayIcon;
        private System.Windows.Forms.CheckBox trayIconCheckBox;
        private System.Windows.Forms.CheckBox alertsCheckBox;
        private System.Windows.Forms.CheckBox animatedCheckBox;
    }
}

