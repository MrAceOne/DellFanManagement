using System;
using System.Collections.Generic;
using DellFanManagement.DellSmbiozBzhLib;

namespace ECFanLevelProbe
{
    /// <summary>
    /// 自动探测 Dell EC 支持的风扇级别数量
    /// 通过 BZH SMM I/O 驱动逐级设置风扇级别并读取 RPM，判断 EC 实际支持多少级
    /// </summary>
    class Program
    {
        /// <summary>
        /// 风扇转速稳定等待时间（毫秒）
        /// </summary>
        const int StabilizationDelayMs = 5000;

        /// <summary>
        /// 最大探测级别
        /// </summary>
        const int MaxProbeLevel = 255;

        /// <summary>
        /// 连续相同 RPM 的次数阈值，超过此值认为已达到最大级别
        /// </summary>
        const int SameRpmThreshold = 3;

        static void Main(string[] args)
        {
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║     Dell EC 风扇级别探测器 v1.0             ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝\n");

            // 初始化 BZH 驱动
            Console.WriteLine("[1] 正在加载 BZH SMM I/O 驱动...");
            if (!DellSmbiosBzh.Initialize())
            {
                Console.WriteLine("    错误：无法加载 bzh_dell_smm_io_x64.sys 驱动。");
                Console.WriteLine("    请确保：");
                Console.WriteLine("    - bzh_dell_smm_io_x64.sys 文件存在于当前目录");
                Console.WriteLine("    - Windows 已配置为允许加载无微软交叉签名的驱动");
                Console.WriteLine("    - 以管理员权限运行本程序\n");
                PromptExit();
                return;
            }
            Console.WriteLine("    驱动加载成功！\n");

            // 禁用自动风扇控制
            Console.WriteLine("[2] 正在禁用自动风扇控制...");
            bool ecDisabled = DellSmbiosBzh.DisableAutomaticFanControl();
            if (!ecDisabled)
            {
                Console.WriteLine("    警告：禁用自动风扇控制失败，尝试备用方法...");
                ecDisabled = DellSmbiosBzh.DisableAutomaticFanControl(alternate: true);
            }

            if (ecDisabled)
            {
                Console.WriteLine("    自动风扇控制已禁用。\n");
            }
            else
            {
                Console.WriteLine("    警告：无法禁用自动风扇控制，探测结果可能不准确！\n");
            }

            // 检测风扇数量
            Console.WriteLine("[3] 正在检测风扇数量...");
            List<int> activeFans = new List<int>();
            for (int i = 0; i <= 6; i++)
            {
                var rpm = DellSmbiosBzh.GetFanRpm((BzhFanIndex)i);
                if (rpm.HasValue)
                {
                    activeFans.Add(i);
                    Console.WriteLine($"    风扇 {i}: {rpm} RPM");
                }
            }

            if (activeFans.Count == 0)
            {
                Console.WriteLine("    未检测到任何风扇！\n");
                RestoreAndExit();
                return;
            }
            Console.WriteLine($"    共检测到 {activeFans.Count} 个风扇。\n");

            // 对每个风扇进行级别探测
            Console.WriteLine("[4] 开始逐级探测风扇级别...\n");
            Console.WriteLine("    注意：探测过程中风扇转速会发生变化，请勿关机。\n");

            foreach (int fanIndex in activeFans)
            {
                ProbeFanLevels(fanIndex);
            }

            // 恢复自动风扇控制
            RestoreAndExit();
        }

        /// <summary>
        /// 探测指定风扇支持的级别
        /// </summary>
        static void ProbeFanLevels(int fanIndex)
        {
            Console.WriteLine($"    ┌─────────────────────────────────────────┐");
            Console.WriteLine($"    │  探测风扇 {fanIndex}                            │");
            Console.WriteLine($"    └─────────────────────────────────────────┘");

            var currentFan = (BzhFanIndex)fanIndex;
            List<LevelRpmResult> results = new List<LevelRpmResult>();
            uint? lastDistinctRpm = null;
            int sameRpmCount = 0;
            int maxEffectiveLevel = -1;

            for (int level = 0; level <= MaxProbeLevel; level++)
            {
                // 设置风扇级别
                bool success = DellSmbiosBzh.SetFanLevel(currentFan, (BzhFanLevel)level);
                if (!success)
                {
                    Console.WriteLine($"    级别 {level,2}: 设置失败（EC 拒绝），跳过");
                    continue;
                }

                // 等待风扇转速稳定
                System.Threading.Thread.Sleep(StabilizationDelayMs);

                // 读取当前 RPM
                uint? currentRpm = DellSmbiosBzh.GetFanRpm(currentFan);
                if (!currentRpm.HasValue)
                {
                    Console.WriteLine($"    级别 {level,2}: 读取 RPM 失败");
                    break;
                }

                // 记录结果
                results.Add(new LevelRpmResult { Level = level, Rpm = currentRpm.Value });

                // 判断 RPM 是否与上一个有效级别不同
                if (lastDistinctRpm.HasValue && currentRpm.Value == lastDistinctRpm.Value)
                {
                    sameRpmCount++;
                    Console.WriteLine($"    级别 {level,2}: {currentRpm,5} RPM  (= 与上一级相同, 连续相同: {sameRpmCount})");
                }
                else
                {
                    sameRpmCount = 0;
                    lastDistinctRpm = currentRpm;
                    maxEffectiveLevel = level;
                    Console.WriteLine($"    级别 {level,2}: {currentRpm,5} RPM  ✓ (转速变化)");
                }

                // 如果连续多次 RPM 相同，认为已达到最大级别
                if (sameRpmCount >= SameRpmThreshold)
                {
                    Console.WriteLine($"    → 连续 {SameRpmThreshold} 级 RPM 相同，判定已达到最大有效级别。");
                    break;
                }
            }

            // 输出汇总
            Console.WriteLine();
            Console.WriteLine($"    风扇 {fanIndex} 探测结果：");
            Console.WriteLine($"    ─────────────────────────────────────────");
            Console.WriteLine($"    最大有效级别: {maxEffectiveLevel}");
            Console.WriteLine($"    有效级别数:   {maxEffectiveLevel + 1} (0 ~ {maxEffectiveLevel})");
            Console.WriteLine();

            // 输出转速对照表
            Console.WriteLine($"    级别  │  RPM    │  变化");
            Console.WriteLine($"    ──────┼─────────┼─────────");
            uint? prevRpm = null;
            foreach (var r in results)
            {
                string change = "";
                if (prevRpm.HasValue)
                {
                    int diff = (int)r.Rpm - (int)prevRpm;
                    if (diff > 0) change = $"+{diff}";
                    else if (diff < 0) change = diff.ToString();
                    else change = "=";
                }
                else
                {
                    change = "-";
                }
                Console.WriteLine($"    {r.Level,4}  │ {r.Rpm,5}   │ {change}");
                prevRpm = r.Rpm;
            }
            Console.WriteLine();
        }

        /// <summary>
        /// 恢复自动风扇控制并退出
        /// </summary>
        static void RestoreAndExit()
        {
            Console.WriteLine("[5] 正在恢复自动风扇控制...");
            bool restored = DellSmbiosBzh.EnableAutomaticFanControl();
            if (!restored)
            {
                restored = DellSmbiosBzh.EnableAutomaticFanControl(alternate: true);
            }

            if (restored)
            {
                Console.WriteLine("    自动风扇控制已恢复。\n");
            }
            else
            {
                Console.WriteLine("    警告：恢复自动风扇控制失败！请重启电脑以恢复。\n");
            }

            DellSmbiosBzh.Shutdown();
            Console.WriteLine("    BZH 驱动已卸载。\n");

            PromptExit();
        }

        static void PromptExit()
        {
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }

        /// <summary>
        /// 记录每个级别对应的 RPM
        /// </summary>
        class LevelRpmResult
        {
            public int Level { get; set; }
            public uint Rpm { get; set; }
        }
    }
}
