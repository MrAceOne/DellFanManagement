using System.Threading;

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
            systemMonitorGroupBox = new System.Windows.Forms.GroupBox();
            cpuFrequencyLabel = new System.Windows.Forms.Label();
            gpuFrequencyLabel = new System.Windows.Forms.Label();
            memoryLabel = new System.Windows.Forms.Label();
            statusStrip = new System.Windows.Forms.StatusStrip();
            trayIcon = new System.Windows.Forms.NotifyIcon(components);
            trayContextMenu = new System.Windows.Forms.ContextMenuStrip(components);
            trayMenuItemShow = new System.Windows.Forms.ToolStripMenuItem();
            trayMenuItemExit = new System.Windows.Forms.ToolStripMenuItem();
            fanControlGroupBox = new System.Windows.Forms.GroupBox();
            autoStartCheckBox = new System.Windows.Forms.CheckBox();
            turboBoostCheckBox = new System.Windows.Forms.CheckBox();
            manuButton = new System.Windows.Forms.RadioButton();
            autoButton = new System.Windows.Forms.RadioButton();
            fansGroupBox.SuspendLayout();
            thermalSettingGroupBox.SuspendLayout();
            systemMonitorGroupBox.SuspendLayout();
            trayContextMenu.SuspendLayout();
            fanControlGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // fansGroupBox
            // 
            fansGroupBox.Controls.Add(fan2RpmLabel);
            fansGroupBox.Controls.Add(fan1RpmLabel);
            fansGroupBox.Location = new System.Drawing.Point(12, 12);
            fansGroupBox.Name = "fansGroupBox";
            fansGroupBox.Size = new System.Drawing.Size(175, 74);
            fansGroupBox.TabIndex = 1;
            fansGroupBox.TabStop = false;
            fansGroupBox.Text = "风扇:";
            // 
            // fan2RpmLabel
            // 
            fan2RpmLabel.AutoSize = true;
            fan2RpmLabel.Location = new System.Drawing.Point(6, 47);
            fan2RpmLabel.Name = "fan2RpmLabel";
            fan2RpmLabel.Size = new System.Drawing.Size(87, 17);
            fan2RpmLabel.TabIndex = 2;
            fan2RpmLabel.Text = "Fan 2 RPM: --";
            // 
            // fan1RpmLabel
            // 
            fan1RpmLabel.AutoSize = true;
            fan1RpmLabel.Location = new System.Drawing.Point(6, 23);
            fan1RpmLabel.Name = "fan1RpmLabel";
            fan1RpmLabel.Size = new System.Drawing.Size(87, 17);
            fan1RpmLabel.TabIndex = 1;
            fan1RpmLabel.Text = "Fan 1 RPM: --";
            // 
            // thermalSettingGroupBox
            // 
            thermalSettingGroupBox.Controls.Add(thermalSettingRadioButtonPerformance);
            thermalSettingGroupBox.Controls.Add(thermalSettingRadioButtonQuiet);
            thermalSettingGroupBox.Controls.Add(thermalSettingRadioButtonCool);
            thermalSettingGroupBox.Controls.Add(thermalSettingRadioButtonOptimized);
            thermalSettingGroupBox.Location = new System.Drawing.Point(193, 92);
            thermalSettingGroupBox.Name = "thermalSettingGroupBox";
            thermalSettingGroupBox.Size = new System.Drawing.Size(129, 84);
            thermalSettingGroupBox.TabIndex = 6;
            thermalSettingGroupBox.TabStop = false;
            thermalSettingGroupBox.Text = "散热管理:";
            // 
            // thermalSettingRadioButtonPerformance
            // 
            thermalSettingRadioButtonPerformance.AutoSize = true;
            thermalSettingRadioButtonPerformance.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            thermalSettingRadioButtonPerformance.Location = new System.Drawing.Point(69, 48);
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
            thermalSettingRadioButtonQuiet.Location = new System.Drawing.Point(69, 22);
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
            // systemMonitorGroupBox
            // 
            systemMonitorGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            systemMonitorGroupBox.Controls.Add(cpuFrequencyLabel);
            systemMonitorGroupBox.Controls.Add(gpuFrequencyLabel);
            systemMonitorGroupBox.Controls.Add(memoryLabel);
            systemMonitorGroupBox.Location = new System.Drawing.Point(12, 92);
            systemMonitorGroupBox.Name = "systemMonitorGroupBox";
            systemMonitorGroupBox.Size = new System.Drawing.Size(175, 84);
            systemMonitorGroupBox.TabIndex = 15;
            systemMonitorGroupBox.TabStop = false;
            systemMonitorGroupBox.Text = "系统监测:";
            // 
            // cpuFrequencyLabel
            // 
            cpuFrequencyLabel.AutoSize = true;
            cpuFrequencyLabel.Location = new System.Drawing.Point(6, 23);
            cpuFrequencyLabel.Name = "cpuFrequencyLabel";
            cpuFrequencyLabel.Size = new System.Drawing.Size(49, 17);
            cpuFrequencyLabel.TabIndex = 0;
            cpuFrequencyLabel.Text = "CPU: --";
            // 
            // gpuFrequencyLabel
            // 
            gpuFrequencyLabel.AutoSize = true;
            gpuFrequencyLabel.Location = new System.Drawing.Point(6, 42);
            gpuFrequencyLabel.Name = "gpuFrequencyLabel";
            gpuFrequencyLabel.Size = new System.Drawing.Size(50, 17);
            gpuFrequencyLabel.TabIndex = 1;
            gpuFrequencyLabel.Text = "GPU: --";
            // 
            // memoryLabel
            // 
            memoryLabel.AutoSize = true;
            memoryLabel.Location = new System.Drawing.Point(6, 61);
            memoryLabel.Name = "memoryLabel";
            memoryLabel.Size = new System.Drawing.Size(49, 17);
            memoryLabel.TabIndex = 2;
            memoryLabel.Text = "内存: --";
            // 
            // statusStrip
            // 
            statusStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            statusStrip.Location = new System.Drawing.Point(0, 188);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new System.Drawing.Size(334, 22);
            statusStrip.TabIndex = 13;
            // 
            // trayIcon
            // 
            trayIcon.ContextMenuStrip = trayContextMenu;
            trayIcon.Text = "Dell Fan Management";
            trayIcon.Visible = true;
            // 
            // trayContextMenu
            // 
            trayContextMenu.ImageScalingSize = new System.Drawing.Size(32, 32);
            trayContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { trayMenuItemShow, trayMenuItemExit });
            trayContextMenu.Name = "trayContextMenu";
            trayContextMenu.Size = new System.Drawing.Size(147, 48);
            // 
            // trayMenuItemShow
            // 
            trayMenuItemShow.Name = "trayMenuItemShow";
            trayMenuItemShow.Size = new System.Drawing.Size(146, 22);
            trayMenuItemShow.Text = "Dell控制面板";
            // 
            // trayMenuItemExit
            // 
            trayMenuItemExit.Name = "trayMenuItemExit";
            trayMenuItemExit.Size = new System.Drawing.Size(146, 22);
            trayMenuItemExit.Text = "退出";
            // 
            // fanControlGroupBox
            // 
            fanControlGroupBox.Controls.Add(autoStartCheckBox);
            fanControlGroupBox.Controls.Add(turboBoostCheckBox);
            fanControlGroupBox.Controls.Add(manuButton);
            fanControlGroupBox.Controls.Add(autoButton);
            fanControlGroupBox.Location = new System.Drawing.Point(193, 12);
            fanControlGroupBox.Name = "fanControlGroupBox";
            fanControlGroupBox.Size = new System.Drawing.Size(129, 74);
            fanControlGroupBox.TabIndex = 16;
            fanControlGroupBox.TabStop = false;
            fanControlGroupBox.Text = "风扇控制:";
            // 
            // autoStartCheckBox
            // 
            autoStartCheckBox.AutoSize = true;
            autoStartCheckBox.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            autoStartCheckBox.Location = new System.Drawing.Point(52, 23);
            autoStartCheckBox.Name = "autoStartCheckBox";
            autoStartCheckBox.Size = new System.Drawing.Size(70, 20);
            autoStartCheckBox.TabIndex = 18;
            autoStartCheckBox.Text = "开机启动";
            autoStartCheckBox.UseVisualStyleBackColor = true;
            // 
            // turboBoostCheckBox
            // 
            turboBoostCheckBox.AutoSize = true;
            turboBoostCheckBox.Checked = true;
            turboBoostCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            turboBoostCheckBox.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            turboBoostCheckBox.Location = new System.Drawing.Point(52, 47);
            turboBoostCheckBox.Name = "turboBoostCheckBox";
            turboBoostCheckBox.Size = new System.Drawing.Size(70, 20);
            turboBoostCheckBox.TabIndex = 17;
            turboBoostCheckBox.Text = "睿频保护";
            turboBoostCheckBox.UseVisualStyleBackColor = true;
            // 
            // manuButton
            // 
            manuButton.AutoSize = true;
            manuButton.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            manuButton.Location = new System.Drawing.Point(6, 47);
            manuButton.Name = "manuButton";
            manuButton.Size = new System.Drawing.Size(47, 20);
            manuButton.TabIndex = 1;
            manuButton.TabStop = true;
            manuButton.Text = "手动";
            manuButton.UseVisualStyleBackColor = true;
            // 
            // autoButton
            // 
            autoButton.AutoSize = true;
            autoButton.Checked = true;
            autoButton.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            autoButton.Location = new System.Drawing.Point(6, 22);
            autoButton.Name = "autoButton";
            autoButton.Size = new System.Drawing.Size(47, 20);
            autoButton.TabIndex = 0;
            autoButton.TabStop = true;
            autoButton.Text = "自动";
            autoButton.UseVisualStyleBackColor = true;
            // 
            // DellFanManagementGuiForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.White;
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            ClientSize = new System.Drawing.Size(334, 210);
            Controls.Add(fanControlGroupBox);
            Controls.Add(systemMonitorGroupBox);
            Controls.Add(statusStrip);
            Controls.Add(thermalSettingGroupBox);
            Controls.Add(fansGroupBox);
            ForeColor = System.Drawing.SystemColors.ControlText;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MaximumSize = new System.Drawing.Size(350, 249);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(350, 249);
            Name = "DellFanManagementGuiForm";
            Text = "Dell 风扇管理";
            fansGroupBox.ResumeLayout(false);
            fansGroupBox.PerformLayout();
            thermalSettingGroupBox.ResumeLayout(false);
            thermalSettingGroupBox.PerformLayout();
            systemMonitorGroupBox.ResumeLayout(false);
            systemMonitorGroupBox.PerformLayout();
            trayContextMenu.ResumeLayout(false);
            fanControlGroupBox.ResumeLayout(false);
            fanControlGroupBox.PerformLayout();
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
        private System.Windows.Forms.GroupBox systemMonitorGroupBox;
        private System.Windows.Forms.Label cpuFrequencyLabel;
        private System.Windows.Forms.Label gpuFrequencyLabel;
        private System.Windows.Forms.Label memoryLabel;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.NotifyIcon trayIcon;
        private System.Windows.Forms.ContextMenuStrip trayContextMenu;
        private System.Windows.Forms.ToolStripMenuItem trayMenuItemShow;
        private System.Windows.Forms.ToolStripMenuItem trayMenuItemExit;
        private System.Windows.Forms.GroupBox fanControlGroupBox;
        private System.Windows.Forms.RadioButton manuButton;
        private System.Windows.Forms.RadioButton autoButton;
        private System.Windows.Forms.CheckBox turboBoostCheckBox;
        private System.Windows.Forms.CheckBox autoStartCheckBox;
    }
}
