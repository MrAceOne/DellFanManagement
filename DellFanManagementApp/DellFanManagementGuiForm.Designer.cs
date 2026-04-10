
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
            fansGroupBox.Location = new System.Drawing.Point(12, 12);
            fansGroupBox.Name = "fansGroupBox";
            fansGroupBox.Size = new System.Drawing.Size(223, 64);
            fansGroupBox.TabIndex = 1;
            fansGroupBox.TabStop = false;
            fansGroupBox.Text = "风扇:";
            // 
            // fan2RpmLabel
            // 
            fan2RpmLabel.AutoSize = true;
            fan2RpmLabel.Location = new System.Drawing.Point(6, 38);
            fan2RpmLabel.Name = "fan2RpmLabel";
            fan2RpmLabel.Size = new System.Drawing.Size(169, 17);
            fan2RpmLabel.TabIndex = 2;
            fan2RpmLabel.Text = "Fan 2 RPM: (Not measured)";
            // 
            // fan1RpmLabel
            // 
            fan1RpmLabel.AutoSize = true;
            fan1RpmLabel.Location = new System.Drawing.Point(6, 19);
            fan1RpmLabel.Name = "fan1RpmLabel";
            fan1RpmLabel.Size = new System.Drawing.Size(169, 17);
            fan1RpmLabel.TabIndex = 1;
            fan1RpmLabel.Text = "Fan 1 RPM: (Not measured)";
            // 
            // thermalSettingGroupBox
            // 
            thermalSettingGroupBox.Controls.Add(thermalSettingRadioButtonPerformance);
            thermalSettingGroupBox.Controls.Add(thermalSettingRadioButtonQuiet);
            thermalSettingGroupBox.Controls.Add(thermalSettingRadioButtonCool);
            thermalSettingGroupBox.Controls.Add(thermalSettingRadioButtonOptimized);
            thermalSettingGroupBox.Location = new System.Drawing.Point(350, 117);
            thermalSettingGroupBox.Name = "thermalSettingGroupBox";
            thermalSettingGroupBox.Size = new System.Drawing.Size(111, 124);
            thermalSettingGroupBox.TabIndex = 6;
            thermalSettingGroupBox.TabStop = false;
            thermalSettingGroupBox.Text = "散热管理:";
            // 
            // thermalSettingRadioButtonPerformance
            // 
            thermalSettingRadioButtonPerformance.AutoSize = true;
            thermalSettingRadioButtonPerformance.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            thermalSettingRadioButtonPerformance.Location = new System.Drawing.Point(6, 97);
            thermalSettingRadioButtonPerformance.Name = "thermalSettingRadioButtonPerformance";
            thermalSettingRadioButtonPerformance.Size = new System.Drawing.Size(47, 20);
            thermalSettingRadioButtonPerformance.TabIndex = 3;
            thermalSettingRadioButtonPerformance.TabStop = true;
            thermalSettingRadioButtonPerformance.Text = "极速";
            thermalSettingRadioButtonPerformance.UseVisualStyleBackColor = true;
            // 
            // thermalSettingRadioButtonQuiet
            // 
            thermalSettingRadioButtonQuiet.AutoSize = true;
            thermalSettingRadioButtonQuiet.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            thermalSettingRadioButtonQuiet.Location = new System.Drawing.Point(6, 72);
            thermalSettingRadioButtonQuiet.Name = "thermalSettingRadioButtonQuiet";
            thermalSettingRadioButtonQuiet.Size = new System.Drawing.Size(47, 20);
            thermalSettingRadioButtonQuiet.TabIndex = 2;
            thermalSettingRadioButtonQuiet.TabStop = true;
            thermalSettingRadioButtonQuiet.Text = "静音";
            thermalSettingRadioButtonQuiet.UseVisualStyleBackColor = true;
            // 
            // thermalSettingRadioButtonCool
            // 
            thermalSettingRadioButtonCool.AutoSize = true;
            thermalSettingRadioButtonCool.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            thermalSettingRadioButtonCool.Location = new System.Drawing.Point(6, 47);
            thermalSettingRadioButtonCool.Name = "thermalSettingRadioButtonCool";
            thermalSettingRadioButtonCool.Size = new System.Drawing.Size(47, 20);
            thermalSettingRadioButtonCool.TabIndex = 1;
            thermalSettingRadioButtonCool.TabStop = true;
            thermalSettingRadioButtonCool.Text = "酷凉";
            thermalSettingRadioButtonCool.UseVisualStyleBackColor = true;
            // 
            // thermalSettingRadioButtonOptimized
            // 
            thermalSettingRadioButtonOptimized.AutoSize = true;
            thermalSettingRadioButtonOptimized.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            thermalSettingRadioButtonOptimized.Location = new System.Drawing.Point(6, 22);
            thermalSettingRadioButtonOptimized.Name = "thermalSettingRadioButtonOptimized";
            thermalSettingRadioButtonOptimized.Size = new System.Drawing.Size(47, 20);
            thermalSettingRadioButtonOptimized.TabIndex = 0;
            thermalSettingRadioButtonOptimized.TabStop = true;
            thermalSettingRadioButtonOptimized.Text = "优化";
            thermalSettingRadioButtonOptimized.UseVisualStyleBackColor = true;
            // 
            // temperatureGroupBox
            // 
            temperatureGroupBox.Controls.Add(temperatureLabel2);
            temperatureGroupBox.Controls.Add(temperatureLabel1);
            temperatureGroupBox.Location = new System.Drawing.Point(12, 83);
            temperatureGroupBox.Name = "temperatureGroupBox";
            temperatureGroupBox.Size = new System.Drawing.Size(223, 77);
            temperatureGroupBox.TabIndex = 4;
            temperatureGroupBox.TabStop = false;
            temperatureGroupBox.Text = "温度:";
            // 
            // temperatureLabel2
            // 
            temperatureLabel2.AutoSize = true;
            temperatureLabel2.Location = new System.Drawing.Point(6, 42);
            temperatureLabel2.Name = "temperatureLabel2";
            temperatureLabel2.Size = new System.Drawing.Size(106, 17);
            temperatureLabel2.TabIndex = 1;
            temperatureLabel2.Text = "GPU: 50 (50-100)";
            // 
            // temperatureLabel1
            // 
            temperatureLabel1.AutoSize = true;
            temperatureLabel1.Location = new System.Drawing.Point(6, 23);
            temperatureLabel1.Name = "temperatureLabel1";
            temperatureLabel1.Size = new System.Drawing.Size(105, 17);
            temperatureLabel1.TabIndex = 0;
            temperatureLabel1.Text = "CPU: 50 (50-100)";
            // 
            // operationModeGroupBox
            // 
            operationModeGroupBox.Controls.Add(operationModeRadioButtonConsistency);
            operationModeGroupBox.Controls.Add(operationModeRadioButtonManual);
            operationModeGroupBox.Controls.Add(operationModeRadioButtonAutomatic);
            operationModeGroupBox.Location = new System.Drawing.Point(350, 12);
            operationModeGroupBox.Name = "operationModeGroupBox";
            operationModeGroupBox.Size = new System.Drawing.Size(111, 99);
            operationModeGroupBox.TabIndex = 5;
            operationModeGroupBox.TabStop = false;
            operationModeGroupBox.Text = "操作模式:";
            // 
            // operationModeRadioButtonConsistency
            // 
            operationModeRadioButtonConsistency.AutoSize = true;
            operationModeRadioButtonConsistency.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            operationModeRadioButtonConsistency.Location = new System.Drawing.Point(6, 72);
            operationModeRadioButtonConsistency.Name = "operationModeRadioButtonConsistency";
            operationModeRadioButtonConsistency.Size = new System.Drawing.Size(58, 20);
            operationModeRadioButtonConsistency.TabIndex = 2;
            operationModeRadioButtonConsistency.TabStop = true;
            operationModeRadioButtonConsistency.Text = "一致性";
            operationModeRadioButtonConsistency.UseVisualStyleBackColor = true;
            // 
            // operationModeRadioButtonManual
            // 
            operationModeRadioButtonManual.AutoSize = true;
            operationModeRadioButtonManual.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            operationModeRadioButtonManual.Location = new System.Drawing.Point(6, 47);
            operationModeRadioButtonManual.Name = "operationModeRadioButtonManual";
            operationModeRadioButtonManual.Size = new System.Drawing.Size(47, 20);
            operationModeRadioButtonManual.TabIndex = 1;
            operationModeRadioButtonManual.TabStop = true;
            operationModeRadioButtonManual.Text = "手动";
            operationModeRadioButtonManual.UseVisualStyleBackColor = true;
            // 
            // operationModeRadioButtonAutomatic
            // 
            operationModeRadioButtonAutomatic.AutoSize = true;
            operationModeRadioButtonAutomatic.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            operationModeRadioButtonAutomatic.Location = new System.Drawing.Point(6, 22);
            operationModeRadioButtonAutomatic.Name = "operationModeRadioButtonAutomatic";
            operationModeRadioButtonAutomatic.Size = new System.Drawing.Size(47, 20);
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
            manualGroupBox.Location = new System.Drawing.Point(467, 12);
            manualGroupBox.Name = "manualGroupBox";
            manualGroupBox.Size = new System.Drawing.Size(194, 126);
            manualGroupBox.TabIndex = 9;
            manualGroupBox.TabStop = false;
            manualGroupBox.Text = "手动控制:";
            // 
            // manualFan2GroupBox
            // 
            manualFan2GroupBox.Controls.Add(manualFan2RadioButtonHigh);
            manualFan2GroupBox.Controls.Add(manualFan2RadioButtonMedium);
            manualFan2GroupBox.Controls.Add(manualFan2RadioButtonOff);
            manualFan2GroupBox.Location = new System.Drawing.Point(100, 21);
            manualFan2GroupBox.Name = "manualFan2GroupBox";
            manualFan2GroupBox.Size = new System.Drawing.Size(88, 99);
            manualFan2GroupBox.TabIndex = 1;
            manualFan2GroupBox.TabStop = false;
            manualFan2GroupBox.Text = "风扇 2:";
            manualFan2GroupBox.Enter += manualFan2GroupBox_Enter;
            // 
            // manualFan2RadioButtonHigh
            // 
            manualFan2RadioButtonHigh.AutoSize = true;
            manualFan2RadioButtonHigh.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            manualFan2RadioButtonHigh.Location = new System.Drawing.Point(6, 72);
            manualFan2RadioButtonHigh.Name = "manualFan2RadioButtonHigh";
            manualFan2RadioButtonHigh.Size = new System.Drawing.Size(36, 20);
            manualFan2RadioButtonHigh.TabIndex = 5;
            manualFan2RadioButtonHigh.TabStop = true;
            manualFan2RadioButtonHigh.Text = "高";
            manualFan2RadioButtonHigh.UseVisualStyleBackColor = true;
            // 
            // manualFan2RadioButtonMedium
            // 
            manualFan2RadioButtonMedium.AutoSize = true;
            manualFan2RadioButtonMedium.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            manualFan2RadioButtonMedium.Location = new System.Drawing.Point(6, 47);
            manualFan2RadioButtonMedium.Name = "manualFan2RadioButtonMedium";
            manualFan2RadioButtonMedium.Size = new System.Drawing.Size(36, 20);
            manualFan2RadioButtonMedium.TabIndex = 5;
            manualFan2RadioButtonMedium.TabStop = true;
            manualFan2RadioButtonMedium.Text = "中";
            manualFan2RadioButtonMedium.UseVisualStyleBackColor = true;
            // 
            // manualFan2RadioButtonOff
            // 
            manualFan2RadioButtonOff.AutoSize = true;
            manualFan2RadioButtonOff.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            manualFan2RadioButtonOff.Location = new System.Drawing.Point(6, 22);
            manualFan2RadioButtonOff.Name = "manualFan2RadioButtonOff";
            manualFan2RadioButtonOff.Size = new System.Drawing.Size(36, 20);
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
            manualFan1GroupBox.Location = new System.Drawing.Point(6, 21);
            manualFan1GroupBox.Name = "manualFan1GroupBox";
            manualFan1GroupBox.Size = new System.Drawing.Size(88, 99);
            manualFan1GroupBox.TabIndex = 0;
            manualFan1GroupBox.TabStop = false;
            manualFan1GroupBox.Text = "风扇 1:";
            // 
            // manualFan1RadioButtonHigh
            // 
            manualFan1RadioButtonHigh.AutoSize = true;
            manualFan1RadioButtonHigh.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            manualFan1RadioButtonHigh.Location = new System.Drawing.Point(6, 72);
            manualFan1RadioButtonHigh.Name = "manualFan1RadioButtonHigh";
            manualFan1RadioButtonHigh.Size = new System.Drawing.Size(36, 20);
            manualFan1RadioButtonHigh.TabIndex = 2;
            manualFan1RadioButtonHigh.TabStop = true;
            manualFan1RadioButtonHigh.Text = "高";
            manualFan1RadioButtonHigh.UseVisualStyleBackColor = true;
            // 
            // manualFan1RadioButtonMedium
            // 
            manualFan1RadioButtonMedium.AutoSize = true;
            manualFan1RadioButtonMedium.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            manualFan1RadioButtonMedium.Location = new System.Drawing.Point(6, 47);
            manualFan1RadioButtonMedium.Name = "manualFan1RadioButtonMedium";
            manualFan1RadioButtonMedium.Size = new System.Drawing.Size(36, 20);
            manualFan1RadioButtonMedium.TabIndex = 1;
            manualFan1RadioButtonMedium.TabStop = true;
            manualFan1RadioButtonMedium.Text = "中";
            manualFan1RadioButtonMedium.UseVisualStyleBackColor = true;
            // 
            // manualFan1RadioButtonOff
            // 
            manualFan1RadioButtonOff.AutoSize = true;
            manualFan1RadioButtonOff.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            manualFan1RadioButtonOff.Location = new System.Drawing.Point(6, 22);
            manualFan1RadioButtonOff.Name = "manualFan1RadioButtonOff";
            manualFan1RadioButtonOff.Size = new System.Drawing.Size(36, 20);
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
            consistencyModeGroupBox.Location = new System.Drawing.Point(467, 144);
            consistencyModeGroupBox.Name = "consistencyModeGroupBox";
            consistencyModeGroupBox.Size = new System.Drawing.Size(194, 138);
            consistencyModeGroupBox.TabIndex = 10;
            consistencyModeGroupBox.TabStop = false;
            consistencyModeGroupBox.Text = "一致性选项:";
            // 
            // alertsCheckBox
            // 
            alertsCheckBox.AutoSize = true;
            alertsCheckBox.Enabled = false;
            alertsCheckBox.Location = new System.Drawing.Point(137, 80);
            alertsCheckBox.Name = "alertsCheckBox";
            alertsCheckBox.Size = new System.Drawing.Size(51, 21);
            alertsCheckBox.TabIndex = 13;
            alertsCheckBox.Text = "警告";
            alertsCheckBox.UseVisualStyleBackColor = true;
            alertsCheckBox.CheckedChanged += alertsCheckBox_CheckedChanged;
            // 
            // consistencyModeApplyChangesButton
            // 
            consistencyModeApplyChangesButton.Location = new System.Drawing.Point(5, 107);
            consistencyModeApplyChangesButton.Name = "consistencyModeApplyChangesButton";
            consistencyModeApplyChangesButton.Size = new System.Drawing.Size(184, 25);
            consistencyModeApplyChangesButton.TabIndex = 14;
            consistencyModeApplyChangesButton.Text = "应用";
            consistencyModeApplyChangesButton.UseVisualStyleBackColor = true;
            // 
            // consistencyModeRpmThresholdTextBox
            // 
            consistencyModeRpmThresholdTextBox.Location = new System.Drawing.Point(71, 78);
            consistencyModeRpmThresholdTextBox.MaxLength = 4;
            consistencyModeRpmThresholdTextBox.Name = "consistencyModeRpmThresholdTextBox";
            consistencyModeRpmThresholdTextBox.Size = new System.Drawing.Size(55, 23);
            consistencyModeRpmThresholdTextBox.TabIndex = 5;
            consistencyModeRpmThresholdTextBox.Text = "2400";
            // 
            // consistencyModeRpmThresholdLabel
            // 
            consistencyModeRpmThresholdLabel.AutoSize = true;
            consistencyModeRpmThresholdLabel.Location = new System.Drawing.Point(7, 81);
            consistencyModeRpmThresholdLabel.Name = "consistencyModeRpmThresholdLabel";
            consistencyModeRpmThresholdLabel.Size = new System.Drawing.Size(59, 17);
            consistencyModeRpmThresholdLabel.TabIndex = 4;
            consistencyModeRpmThresholdLabel.Text = "转速阈值:";
            // 
            // consistencyModeUpperTemperatureThresholdTextBox
            // 
            consistencyModeUpperTemperatureThresholdTextBox.Location = new System.Drawing.Point(106, 49);
            consistencyModeUpperTemperatureThresholdTextBox.MaxLength = 2;
            consistencyModeUpperTemperatureThresholdTextBox.Name = "consistencyModeUpperTemperatureThresholdTextBox";
            consistencyModeUpperTemperatureThresholdTextBox.Size = new System.Drawing.Size(82, 23);
            consistencyModeUpperTemperatureThresholdTextBox.TabIndex = 3;
            consistencyModeUpperTemperatureThresholdTextBox.Text = "85";
            // 
            // consistencyModeUpperTemperatureThresholdLabel
            // 
            consistencyModeUpperTemperatureThresholdLabel.AutoSize = true;
            consistencyModeUpperTemperatureThresholdLabel.Location = new System.Drawing.Point(6, 52);
            consistencyModeUpperTemperatureThresholdLabel.Name = "consistencyModeUpperTemperatureThresholdLabel";
            consistencyModeUpperTemperatureThresholdLabel.Size = new System.Drawing.Size(59, 17);
            consistencyModeUpperTemperatureThresholdLabel.TabIndex = 2;
            consistencyModeUpperTemperatureThresholdLabel.Text = "温度上限:";
            // 
            // consistencyModeLowerTemperatureThresholdTextBox
            // 
            consistencyModeLowerTemperatureThresholdTextBox.Location = new System.Drawing.Point(106, 20);
            consistencyModeLowerTemperatureThresholdTextBox.MaxLength = 2;
            consistencyModeLowerTemperatureThresholdTextBox.Name = "consistencyModeLowerTemperatureThresholdTextBox";
            consistencyModeLowerTemperatureThresholdTextBox.Size = new System.Drawing.Size(82, 23);
            consistencyModeLowerTemperatureThresholdTextBox.TabIndex = 1;
            consistencyModeLowerTemperatureThresholdTextBox.Text = "65";
            consistencyModeLowerTemperatureThresholdTextBox.TextChanged += consistencyModeLowerTemperatureThresholdTextBox_TextChanged;
            // 
            // consistencyModeLowerTemperatureThresholdLabel
            // 
            consistencyModeLowerTemperatureThresholdLabel.AutoSize = true;
            consistencyModeLowerTemperatureThresholdLabel.Location = new System.Drawing.Point(6, 23);
            consistencyModeLowerTemperatureThresholdLabel.Name = "consistencyModeLowerTemperatureThresholdLabel";
            consistencyModeLowerTemperatureThresholdLabel.Size = new System.Drawing.Size(59, 17);
            consistencyModeLowerTemperatureThresholdLabel.TabIndex = 0;
            consistencyModeLowerTemperatureThresholdLabel.Text = "温度下限:";
            consistencyModeLowerTemperatureThresholdLabel.Click += consistencyModeLowerTemperatureThresholdLabel_Click;
            // 
            // ecFanControlRadioButtonOn
            // 
            ecFanControlRadioButtonOn.AutoSize = true;
            ecFanControlRadioButtonOn.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            ecFanControlRadioButtonOn.Location = new System.Drawing.Point(6, 22);
            ecFanControlRadioButtonOn.Name = "ecFanControlRadioButtonOn";
            ecFanControlRadioButtonOn.Size = new System.Drawing.Size(36, 20);
            ecFanControlRadioButtonOn.TabIndex = 2;
            ecFanControlRadioButtonOn.TabStop = true;
            ecFanControlRadioButtonOn.Text = "开";
            ecFanControlRadioButtonOn.UseVisualStyleBackColor = true;
            // 
            // ecFanControlRadioButtonOff
            // 
            ecFanControlRadioButtonOff.AutoSize = true;
            ecFanControlRadioButtonOff.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            ecFanControlRadioButtonOff.Location = new System.Drawing.Point(53, 22);
            ecFanControlRadioButtonOff.Name = "ecFanControlRadioButtonOff";
            ecFanControlRadioButtonOff.Size = new System.Drawing.Size(36, 20);
            ecFanControlRadioButtonOff.TabIndex = 3;
            ecFanControlRadioButtonOff.TabStop = true;
            ecFanControlRadioButtonOff.Text = "关";
            ecFanControlRadioButtonOff.UseVisualStyleBackColor = true;
            // 
            // ecFanControlGroupBox
            // 
            ecFanControlGroupBox.Controls.Add(ecFanControlRadioButtonOn);
            ecFanControlGroupBox.Controls.Add(ecFanControlRadioButtonOff);
            ecFanControlGroupBox.Location = new System.Drawing.Point(241, 12);
            ecFanControlGroupBox.Name = "ecFanControlGroupBox";
            ecFanControlGroupBox.Size = new System.Drawing.Size(103, 48);
            ecFanControlGroupBox.TabIndex = 2;
            ecFanControlGroupBox.TabStop = false;
            ecFanControlGroupBox.Text = "EC 风扇控制:";
            // 
            // restartBackgroundThreadButton
            // 
            restartBackgroundThreadButton.Font = new System.Drawing.Font("Segoe UI", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            restartBackgroundThreadButton.Location = new System.Drawing.Point(241, 66);
            restartBackgroundThreadButton.Name = "restartBackgroundThreadButton";
            restartBackgroundThreadButton.Size = new System.Drawing.Size(105, 21);
            restartBackgroundThreadButton.TabIndex = 3;
            restartBackgroundThreadButton.Text = "重置线程";
            restartBackgroundThreadButton.UseVisualStyleBackColor = true;
            // 
            // statusStrip
            // 
            statusStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { consistencyModeStatusLabel });
            statusStrip.Location = new System.Drawing.Point(0, 293);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new System.Drawing.Size(672, 22);
            statusStrip.SizingGrip = false;
            statusStrip.TabIndex = 13;
            // 
            // consistencyModeStatusLabel
            // 
            consistencyModeStatusLabel.Name = "consistencyModeStatusLabel";
            consistencyModeStatusLabel.Size = new System.Drawing.Size(124, 17);
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
            trayIconCheckBox.Location = new System.Drawing.Point(356, 246);
            trayIconCheckBox.Name = "trayIconCheckBox";
            trayIconCheckBox.Size = new System.Drawing.Size(75, 21);
            trayIconCheckBox.TabIndex = 7;
            trayIconCheckBox.Text = "托盘图标";
            trayIconCheckBox.UseVisualStyleBackColor = true;
            // 
            // animatedCheckBox
            // 
            animatedCheckBox.AutoSize = true;
            animatedCheckBox.Checked = true;
            animatedCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            animatedCheckBox.Location = new System.Drawing.Point(356, 266);
            animatedCheckBox.Name = "animatedCheckBox";
            animatedCheckBox.Size = new System.Drawing.Size(51, 21);
            animatedCheckBox.TabIndex = 8;
            animatedCheckBox.Text = "动画";
            animatedCheckBox.UseVisualStyleBackColor = true;
            // 
            // DellFanManagementGuiForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(672, 315);
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

