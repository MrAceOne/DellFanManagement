using Microsoft.Win32;
using System;

namespace DellFanManagement.Service
{
    /// <summary>
    /// 配置管理，使用注册表存储设置
    /// 优先使用 HKEY_LOCAL_MACHINE（服务模式），回退到 HKEY_CURRENT_USER（命令行模式）
    /// </summary>
    public class Configuration
    {
        private const string RegistryKeyPath = @"SOFTWARE\DellFanService";

        // 当前运行模式
        public FanMode CurrentMode { get; set; } = FanMode.Optimized;

        // ========== 手动模式参数 ==========

        /// <summary>
        /// CPU温度阈值（默认45度），超过此值风扇1开启
        /// </summary>
        public int CpuTemperatureThreshold { get; set; } = 45;

        /// <summary>
        /// GPU温度阈值（默认45度），超过此值风扇2开启
        /// </summary>
        public int GpuTemperatureThreshold { get; set; } = 45;

        /// <summary>
        /// 温度检查间隔（秒，默认5秒）
        /// </summary>
        public int CheckIntervalSeconds { get; set; } = 5;

        /// <summary>
        /// 是否启用高温禁用睿频保护（默认true）
        /// </summary>
        public bool EnableTurboBoostProtection { get; set; } = true;

        /// <summary>
        /// 高温触发禁用睿频的CPU温度阈值（默认95度）
        /// </summary>
        public int TriggerCpuTemp { get; set; } = 95;

        /// <summary>
        /// 高温触发禁用睿频的GPU温度阈值（默认80度）
        /// </summary>
        public int TriggerGpuTemp { get; set; } = 80;

        /// <summary>
        /// 高温触发前需要持续满足高温条件的秒数（默认10秒）
        /// </summary>
        public int TriggerDurationSeconds { get; set; } = 10;

        /// <summary>
        /// 恢复睿频的CPU温度上限（默认80度）
        /// </summary>
        public int RecoveryCpuTemp { get; set; } = 80;

        /// <summary>
        /// 恢复睿频的GPU温度上限（默认70度）
        /// </summary>
        public int RecoveryGpuTemp { get; set; } = 70;

        /// <summary>
        /// 恢复睿频前需要持续满足低温条件的秒数（默认20秒）
        /// </summary>
        public int RecoveryDurationSeconds { get; set; } = 20;

        /// <summary>
        /// 风扇1手动档位（0=Off, 1=Medium, 2=High）
        /// </summary>
        public int Fan1ManualLevel { get; set; } = 1;

        /// <summary>
        /// 风扇2手动档位（0=Off, 1=Medium, 2=High）
        /// </summary>
        public int Fan2ManualLevel { get; set; } = 1;

        /// <summary>
        /// 是否启用基于温度的自动控制（手动模式下）
        /// </summary>
        public bool EnableTemperatureControl { get; set; } = true;

        /// <summary>
        /// 配置存储位置
        /// </summary>
        public RegistryLocation Location { get; private set; } = RegistryLocation.LocalMachine;

        /// <summary>
        /// 从注册表加载配置（优先 HKLM，回退 HKCU）
        /// </summary>
        public void Load()
        {
            // 先尝试从 HKLM 加载
            using RegistryKey? lmKey = Registry.LocalMachine.OpenSubKey(RegistryKeyPath);
            if (lmKey != null)
            {
                LoadFromKey(lmKey);
                Location = RegistryLocation.LocalMachine;
                return;
            }

            // 回退到 HKCU
            using RegistryKey? cuKey = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            if (cuKey != null)
            {
                LoadFromKey(cuKey);
                Location = RegistryLocation.CurrentUser;
            }
        }

        private void LoadFromKey(RegistryKey key)
        {
            CurrentMode = (FanMode)GetInt(key, "CurrentMode", (int)CurrentMode);
            CpuTemperatureThreshold = GetInt(key, "CpuTemperatureThreshold", CpuTemperatureThreshold);
            GpuTemperatureThreshold = GetInt(key, "GpuTemperatureThreshold", GpuTemperatureThreshold);
            CheckIntervalSeconds = GetInt(key, "CheckIntervalSeconds", CheckIntervalSeconds);
            EnableTurboBoostProtection = GetInt(key, "EnableTurboBoostProtection", EnableTurboBoostProtection ? 1 : 0) != 0;
            TriggerCpuTemp = GetInt(key, "TriggerCpuTemp", TriggerCpuTemp);
            TriggerGpuTemp = GetInt(key, "TriggerGpuTemp", TriggerGpuTemp);
            TriggerDurationSeconds = GetInt(key, "TriggerDurationSeconds", TriggerDurationSeconds);
            RecoveryCpuTemp = GetInt(key, "RecoveryCpuTemp", RecoveryCpuTemp);
            RecoveryGpuTemp = GetInt(key, "RecoveryGpuTemp", RecoveryGpuTemp);
            RecoveryDurationSeconds = GetInt(key, "RecoveryDurationSeconds", RecoveryDurationSeconds);
            Fan1ManualLevel = GetInt(key, "Fan1ManualLevel", Fan1ManualLevel);
            Fan2ManualLevel = GetInt(key, "Fan2ManualLevel", Fan2ManualLevel);
            EnableTemperatureControl = GetInt(key, "EnableTemperatureControl", EnableTemperatureControl ? 1 : 0) != 0;
        }

        /// <summary>
        /// 保存配置到注册表
        /// 优先尝试 HKLM（服务/全局配置），失败则回退到 HKCU（用户配置）
        /// </summary>
        public bool Save()
        {
            // 先尝试保存到 HKLM
            try
            {
                using RegistryKey key = Registry.LocalMachine.CreateSubKey(RegistryKeyPath);
                SaveToKey(key);
                Location = RegistryLocation.LocalMachine;
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                // 没有 HKLM 权限，回退到 HKCU
                try
                {
                    using RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
                    SaveToKey(key);
                    Location = RegistryLocation.CurrentUser;
                    Console.WriteLine("注意: 配置已保存到当前用户注册表（需要管理员权限才能保存到全局配置）");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"保存配置失败: {ex.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存配置失败: {ex.Message}");
                return false;
            }
        }

        private void SaveToKey(RegistryKey key)
        {
            key.SetValue("CurrentMode", (int)CurrentMode, RegistryValueKind.DWord);
            key.SetValue("CpuTemperatureThreshold", CpuTemperatureThreshold, RegistryValueKind.DWord);
            key.SetValue("GpuTemperatureThreshold", GpuTemperatureThreshold, RegistryValueKind.DWord);
            key.SetValue("CheckIntervalSeconds", CheckIntervalSeconds, RegistryValueKind.DWord);
            key.SetValue("EnableTurboBoostProtection", EnableTurboBoostProtection ? 1 : 0, RegistryValueKind.DWord);
            key.SetValue("TriggerCpuTemp", TriggerCpuTemp, RegistryValueKind.DWord);
            key.SetValue("TriggerGpuTemp", TriggerGpuTemp, RegistryValueKind.DWord);
            key.SetValue("TriggerDurationSeconds", TriggerDurationSeconds, RegistryValueKind.DWord);
            key.SetValue("RecoveryCpuTemp", RecoveryCpuTemp, RegistryValueKind.DWord);
            key.SetValue("RecoveryGpuTemp", RecoveryGpuTemp, RegistryValueKind.DWord);
            key.SetValue("RecoveryDurationSeconds", RecoveryDurationSeconds, RegistryValueKind.DWord);
            key.SetValue("Fan1ManualLevel", Fan1ManualLevel, RegistryValueKind.DWord);
            key.SetValue("Fan2ManualLevel", Fan2ManualLevel, RegistryValueKind.DWord);
            key.SetValue("EnableTemperatureControl", EnableTemperatureControl ? 1 : 0, RegistryValueKind.DWord);
        }

        private static int GetInt(RegistryKey key, string name, int defaultValue)
        {
            object? value = key.GetValue(name);
            if (value is int intValue)
                return intValue;
            if (value != null && int.TryParse(value.ToString(), out int parsed))
                return parsed;
            return defaultValue;
        }

        /// <summary>
        /// 打印当前配置
        /// </summary>
        public void Print()
        {
            Console.WriteLine("当前配置:");
            Console.WriteLine($"  存储位置: {Location}");
            Console.WriteLine($"  运行模式: {CurrentMode}");
            Console.WriteLine($"  CPU温度阈值: {CpuTemperatureThreshold}°C");
            Console.WriteLine($"  GPU温度阈值: {GpuTemperatureThreshold}°C");
            Console.WriteLine($"  检查间隔: {CheckIntervalSeconds}秒");
            Console.WriteLine($"  启用睿频保护: {EnableTurboBoostProtection}");
            Console.WriteLine($"  高温触发CPU温度: {TriggerCpuTemp}°C");
            Console.WriteLine($"  高温触发GPU温度: {TriggerGpuTemp}°C");
            Console.WriteLine($"  高温触发持续时间: {TriggerDurationSeconds}秒");
            Console.WriteLine($"  恢复睿频CPU温度: {RecoveryCpuTemp}°C");
            Console.WriteLine($"  恢复睿频GPU温度: {RecoveryGpuTemp}°C");
            Console.WriteLine($"  恢复持续时间: {RecoveryDurationSeconds}秒");
            Console.WriteLine($"  风扇1手动档位: {Fan1ManualLevel}");
            Console.WriteLine($"  风扇2手动档位: {Fan2ManualLevel}");
            Console.WriteLine($"  启用温度控制: {EnableTemperatureControl}");
        }
    }

    /// <summary>
    /// 注册表存储位置
    /// </summary>
    public enum RegistryLocation
    {
        LocalMachine,
        CurrentUser
    }
}
