using System;
using System.Collections.Generic;
using DellFanManagement.DellSmbiosSmiLib;
using DellFanManagement.DellSmbiosSmiLib.DellSmi;

namespace SMIFanLevelProbe
{
    /// <summary>
    /// 自动探测 Dell SMI 接口支持的风扇级别数量
    /// 通过 WMI/ACPI 接口逐级设置风扇级别并读取当前设置和风扇转速，判断 SMI 实际支持多少级
    /// </summary>
    class Program
    {
        /// <summary>
        /// 风扇级别稳定等待时间（毫秒）
        /// </summary>
        const int StabilizationDelayMs = 5000;

        static void Main(string[] args)
        {
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║     Dell SMI 风扇级别探测器 v1.0            ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝\n");

            // 先读取初始风扇转速
            Console.WriteLine("[1] 正在读取初始风扇转速...");
            uint? initialFan1Rpm = DellSmbiosSmi.GetTokenCurrentValue(Token.Fan1Rpm);
            uint? initialFan2Rpm = DellSmbiosSmi.GetTokenCurrentValue(Token.Fan2Rpm);
            if (initialFan1Rpm.HasValue)
            {
                Console.WriteLine($"    风扇 1: {initialFan1Rpm} RPM");
            }
            if (initialFan2Rpm.HasValue)
            {
                Console.WriteLine($"    风扇 2: {initialFan2Rpm} RPM");
            }
            if (!initialFan1Rpm.HasValue && !initialFan2Rpm.HasValue)
            {
                Console.WriteLine("    无法通过 SMI Token 读取风扇转速，将仅显示 Token 设置状态。");
            }
            Console.WriteLine();

            // 禁用自动风扇控制
            Console.WriteLine("[2] 正在禁用自动风扇控制...");
            bool autoDisabled = DellSmbiosSmi.DisableAutomaticFanControl();
            if (!autoDisabled)
            {
                Console.WriteLine("    警告：禁用自动风扇控制失败，可能无法精确控制风扇。\n");
            }
            else
            {
                Console.WriteLine("    自动风扇控制已禁用。\n");
            }

            // 探测 SMI 支持的级别
            Console.WriteLine("[3] 开始逐级探测 SMI 风扇级别...\n");
            Console.WriteLine("    注意：探测过程中风扇转速会发生变化，请勿关机。\n");

            ProbeFanLevels();

            // 恢复自动风扇控制
            RestoreAndExit();
        }

        /// <summary>
        /// 读取风扇转速
        /// </summary>
        static (uint? fan1, uint? fan2) ReadFanRpm()
        {
            uint? fan1 = DellSmbiosSmi.GetTokenCurrentValue(Token.Fan1Rpm);
            uint? fan2 = DellSmbiosSmi.GetTokenCurrentValue(Token.Fan2Rpm);
            return (fan1, fan2);
        }

        /// <summary>
        /// 探测 SMI 风扇级别
        /// </summary>
        static void ProbeFanLevels()
        {
            // SMI 接口通过 Token 设置风扇级别
            // 按 SetFanLevel() 中的映射顺序测试，同时测试额外的 Token
            var testTokens = new[]
            {
                new { Token = Token.FanSpeedMediumHigh, Name = "Off (MediumHigh Token)" },
                new { Token = Token.FanSpeedMedium,     Name = "Low (Medium Token)" },
                new { Token = Token.FanSpeedHigh,       Name = "Medium (High Token)" },
                new { Token = Token.FanSpeedLow,        Name = "High (Low Token)" },
                new { Token = Token.FanSpeedMediumLow,  Name = "MediumLow (额外)" },
                new { Token = Token.FanSpeedAuto,       Name = "Auto (自动)" },
            };

            Console.WriteLine("    测试顺序：");
            foreach (var t in testTokens)
            {
                Console.WriteLine($"        {t.Name} ({t.Token})");
            }
            Console.WriteLine();

            // 记录每个 Token 的设置结果
            var results = new List<TokenResult>();

            foreach (var testItem in testTokens)
            {
                // 设置 Token
                bool success = DellSmbiosSmi.SetToken(testItem.Token);
                if (!success)
                {
                    Console.WriteLine($"    {testItem.Name,30}: 设置失败（EC 拒绝），跳过");
                    results.Add(new TokenResult { Token = testItem.Token, Name = testItem.Name, Success = false });
                    continue;
                }

                // 等待稳定
                System.Threading.Thread.Sleep(StabilizationDelayMs);

                // 读取当前设置
                uint? currentValue = DellSmbiosSmi.GetTokenCurrentValue(testItem.Token);

                // 读取风扇转速
                var (fan1Rpm, fan2Rpm) = ReadFanRpm();

                // 输出结果
                string rpmInfo = "";
                if (fan1Rpm.HasValue || fan2Rpm.HasValue)
                {
                    rpmInfo = $"风扇1: {fan1Rpm?.ToString() ?? "N/A"} RPM, 风扇2: {fan2Rpm?.ToString() ?? "N/A"} RPM";
                }

                Console.WriteLine($"    {testItem.Name,30}: 设置成功, Token值: {currentValue?.ToString() ?? "N/A"}, {rpmInfo}");

                results.Add(new TokenResult
                {
                    Token = testItem.Token,
                    Name = testItem.Name,
                    Success = true,
                    CurrentValue = currentValue,
                    Fan1Rpm = fan1Rpm,
                    Fan2Rpm = fan2Rpm
                });
            }

            // 输出汇总
            Console.WriteLine();
            Console.WriteLine("    SMI 探测结果：");
            Console.WriteLine("    ──────────────────────────────────────────────────────────────────────────────");
            int validCount = results.FindAll(r => r.Success).Count;
            Console.WriteLine($"    有效级别数: {validCount}");
            Console.WriteLine();

            // 输出结果表
            Console.WriteLine("    级别名称                       │ 状态   │ Token值 │ 风扇1 RPM │ 风扇2 RPM");
            Console.WriteLine("    ───────────────────────────────┼────────┼─────────┼───────────┼──────────");
            foreach (var r in results)
            {
                string status = r.Success ? "成功" : "失败";
                string value = r.CurrentValue.HasValue ? r.CurrentValue.Value.ToString() : "-";
                string fan1 = r.Fan1Rpm.HasValue ? r.Fan1Rpm.Value.ToString() : "N/A";
                string fan2 = r.Fan2Rpm.HasValue ? r.Fan2Rpm.Value.ToString() : "N/A";
                Console.WriteLine($"    {r.Name,30} │ {status,6} │ {value,7} │ {fan1,9} │ {fan2,8}");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// 恢复自动风扇控制并退出
        /// </summary>
        static void RestoreAndExit()
        {
            Console.WriteLine("[4] 正在恢复自动风扇控制...");
            bool restored = DellSmbiosSmi.EnableAutomaticFanControl();
            if (restored)
            {
                Console.WriteLine("    自动风扇控制已恢复。\n");
            }
            else
            {
                Console.WriteLine("    警告：恢复自动风扇控制失败！请重启电脑以恢复。\n");
            }

            PromptExit();
        }

        static void PromptExit()
        {
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }

        /// <summary>
        /// 记录每个 Token 的设置结果
        /// </summary>
        class TokenResult
        {
            public Token Token { get; set; }
            public string Name { get; set; }
            public bool Success { get; set; }
            public uint? CurrentValue { get; set; }
            public uint? Fan1Rpm { get; set; }
            public uint? Fan2Rpm { get; set; }
        }
    }
}
