using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace DellFanManagement.Service
{
    class Program
    {
        private const string ServiceName = "DellFanService";
        private const string ServiceDisplayName = "Dell Fan Management Service";
        private const string ServiceDescription = "Dell笔记本风扇控制服务，支持手动/优化/酷凉/静音/极速模式";

        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            // 解析参数
            var parsed = ParseArgs(args);

            if (parsed.ShowHelp)
            {
                PrintUsage();
                return;
            }

            if (parsed.Mode.HasValue)
            {
                await SetModeCommand(parsed.Mode.Value);
                return;
            }

            if (parsed.ShowStatus)
            {
                StatusCommand();
                return;
            }

            if (parsed.IsConfig)
            {
                ConfigCommand(parsed);
                return;
            }

            if (parsed.IsInstall)
            {
                InstallService();
                return;
            }

            if (parsed.IsUninstall)
            {
                UninstallService();
                return;
            }

            if (parsed.IsStart)
            {
                StartService();
                return;
            }

            if (parsed.IsStop)
            {
                StopService();
                return;
            }

            if (parsed.IsRestart)
            {
                RestartService();
                return;
            }

            if (parsed.IsRun)
            {
                await RunConsoleMode(args);
                return;
            }

            // 未识别的参数组合
            Console.WriteLine("无效的参数组合，请使用 --help 查看帮助");
            Environment.ExitCode = 1;
        }

        /// <summary>
        /// 解析命令行参数
        /// </summary>
        private static ParsedArgs ParseArgs(string[] args)
        {
            var result = new ParsedArgs();

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i].ToLowerInvariant();
                string? nextArg = i + 1 < args.Length ? args[i + 1] : null;

                switch (arg)
                {
                    case "-m":
                    case "--mode":
                        if (nextArg != null && int.TryParse(nextArg, out int mode) && mode >= 0 && mode <= 4)
                        {
                            result.Mode = mode;
                            i++;
                        }
                        else
                        {
                            Console.WriteLine("错误: -m/--mode 需要指定 0-4 的模式值");
                            Environment.ExitCode = 1;
                        }
                        break;

                    case "-s":
                    case "--status":
                        result.ShowStatus = true;
                        break;

                    case "-c":
                    case "--config":
                        result.IsConfig = true;
                        break;

                    case "--install":
                        result.IsInstall = true;
                        break;

                    case "--uninstall":
                        result.IsUninstall = true;
                        break;

                    case "--start":
                        result.IsStart = true;
                        break;

                    case "--stop":
                        result.IsStop = true;
                        break;

                    case "--restart":
                        result.IsRestart = true;
                        break;

                    case "--run":
                        result.IsRun = true;
                        break;

                    case "-h":
                    case "--help":
                    case "/?":
                        result.ShowHelp = true;
                        break;

                    // 配置项参数（仅在 --config 模式下有效）
                    case "--cpu-threshold":
                        if (nextArg != null && int.TryParse(nextArg, out int cpuThreshold))
                        {
                            result.CpuThreshold = cpuThreshold;
                            i++;
                        }
                        break;

                    case "--gpu-threshold":
                        if (nextArg != null && int.TryParse(nextArg, out int gpuThreshold))
                        {
                            result.GpuThreshold = gpuThreshold;
                            i++;
                        }
                        break;

                    case "--check-interval":
                        if (nextArg != null && int.TryParse(nextArg, out int interval))
                        {
                            result.CheckInterval = interval;
                            i++;
                        }
                        break;

                    case "--enable-turbo-protection":
                        if (nextArg != null && bool.TryParse(nextArg, out bool turboProtection))
                        {
                            result.EnableTurboProtection = turboProtection;
                            i++;
                        }
                        break;

                    case "--trigger-cpu":
                        if (nextArg != null && int.TryParse(nextArg, out int triggerCpu))
                        {
                            result.TriggerCpu = triggerCpu;
                            i++;
                        }
                        break;

                    case "--trigger-gpu":
                        if (nextArg != null && int.TryParse(nextArg, out int triggerGpu))
                        {
                            result.TriggerGpu = triggerGpu;
                            i++;
                        }
                        break;

                    case "--trigger-duration":
                        if (nextArg != null && int.TryParse(nextArg, out int triggerDuration))
                        {
                            result.TriggerDuration = triggerDuration;
                            i++;
                        }
                        break;

                    case "--recovery-cpu":
                        if (nextArg != null && int.TryParse(nextArg, out int recoveryCpu))
                        {
                            result.RecoveryCpu = recoveryCpu;
                            i++;
                        }
                        break;

                    case "--recovery-gpu":
                        if (nextArg != null && int.TryParse(nextArg, out int recoveryGpu))
                        {
                            result.RecoveryGpu = recoveryGpu;
                            i++;
                        }
                        break;

                    case "--recovery-duration":
                        if (nextArg != null && int.TryParse(nextArg, out int recoveryDuration))
                        {
                            result.RecoveryDuration = recoveryDuration;
                            i++;
                        }
                        break;

                    case "--fan1-level":
                        if (nextArg != null && int.TryParse(nextArg, out int fan1Level) && fan1Level >= 0 && fan1Level <= 2)
                        {
                            result.Fan1Level = fan1Level;
                            i++;
                        }
                        break;

                    case "--fan2-level":
                        if (nextArg != null && int.TryParse(nextArg, out int fan2Level) && fan2Level >= 0 && fan2Level <= 2)
                        {
                            result.Fan2Level = fan2Level;
                            i++;
                        }
                        break;

                    case "--temperature-control":
                        if (nextArg != null && bool.TryParse(nextArg, out bool tempControl))
                        {
                            result.TemperatureControl = tempControl;
                            i++;
                        }
                        break;

                    case "--reset":
                        result.IsReset = true;
                        break;

                    default:
                        if (arg.StartsWith("-"))
                        {
                            Console.WriteLine($"未知参数: {arg}");
                        }
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// 设置运行模式
        /// </summary>
        private static async Task SetModeCommand(int modeValue)
        {
            FanMode mode = (FanMode)modeValue;
            string modeName = mode switch
            {
                FanMode.Manual => "手动控制",
                FanMode.Optimized => "优化散热",
                FanMode.Cool => "酷凉模式",
                FanMode.Quiet => "静音模式",
                FanMode.Performance => "极速模式",
                _ => "未知"
            };

            if (IsServiceRunning())
            {
                Configuration config = new Configuration();
                config.Load();
                config.CurrentMode = mode;
                config.Save();
                Console.WriteLine($"已发送模式切换请求: {modeName} (模式 {modeValue})");
                Console.WriteLine("服务将在下次检查周期应用新模式");
            }
            else
            {
                Console.WriteLine($"服务未运行，以前台模式启动: {modeName}");
                Configuration config = new Configuration();
                config.Load();
                config.CurrentMode = mode;
                config.Save();
                await RunConsoleMode(new[] { "--run" });
            }
        }

        /// <summary>
        /// 配置命令
        /// </summary>
        private static void ConfigCommand(ParsedArgs parsed)
        {
            Configuration config = new Configuration();
            config.Load();

            bool changed = false;

            if (parsed.IsReset)
            {
                config = new Configuration();
                Console.WriteLine("配置已重置为默认值");
                changed = true;
            }
            else
            {
                if (parsed.CpuThreshold.HasValue)
                {
                    config.CpuTemperatureThreshold = parsed.CpuThreshold.Value;
                    Console.WriteLine($"CPU温度阈值已设置为: {parsed.CpuThreshold.Value}°C");
                    changed = true;
                }

                if (parsed.GpuThreshold.HasValue)
                {
                    config.GpuTemperatureThreshold = parsed.GpuThreshold.Value;
                    Console.WriteLine($"GPU温度阈值已设置为: {parsed.GpuThreshold.Value}°C");
                    changed = true;
                }

                if (parsed.CheckInterval.HasValue)
                {
                    config.CheckIntervalSeconds = parsed.CheckInterval.Value;
                    Console.WriteLine($"检查间隔已设置为: {parsed.CheckInterval.Value}秒");
                    changed = true;
                }

                if (parsed.EnableTurboProtection.HasValue)
                {
                    config.EnableTurboBoostProtection = parsed.EnableTurboProtection.Value;
                    Console.WriteLine($"睿频保护已设置为: {parsed.EnableTurboProtection.Value}");
                    changed = true;
                }

                if (parsed.TriggerCpu.HasValue)
                {
                    config.TriggerCpuTemp = parsed.TriggerCpu.Value;
                    Console.WriteLine($"高温触发CPU温度已设置为: {parsed.TriggerCpu.Value}°C");
                    changed = true;
                }

                if (parsed.TriggerGpu.HasValue)
                {
                    config.TriggerGpuTemp = parsed.TriggerGpu.Value;
                    Console.WriteLine($"高温触发GPU温度已设置为: {parsed.TriggerGpu.Value}°C");
                    changed = true;
                }

                if (parsed.TriggerDuration.HasValue)
                {
                    config.TriggerDurationSeconds = parsed.TriggerDuration.Value;
                    Console.WriteLine($"高温触发持续时间已设置为: {parsed.TriggerDuration.Value}秒");
                    changed = true;
                }

                if (parsed.RecoveryCpu.HasValue)
                {
                    config.RecoveryCpuTemp = parsed.RecoveryCpu.Value;
                    Console.WriteLine($"恢复睿频CPU温度已设置为: {parsed.RecoveryCpu.Value}°C");
                    changed = true;
                }

                if (parsed.RecoveryGpu.HasValue)
                {
                    config.RecoveryGpuTemp = parsed.RecoveryGpu.Value;
                    Console.WriteLine($"恢复睿频GPU温度已设置为: {parsed.RecoveryGpu.Value}°C");
                    changed = true;
                }

                if (parsed.RecoveryDuration.HasValue)
                {
                    config.RecoveryDurationSeconds = parsed.RecoveryDuration.Value;
                    Console.WriteLine($"恢复持续时间已设置为: {parsed.RecoveryDuration.Value}秒");
                    changed = true;
                }

                if (parsed.Fan1Level.HasValue)
                {
                    config.Fan1ManualLevel = parsed.Fan1Level.Value;
                    Console.WriteLine($"风扇1手动档位已设置为: {parsed.Fan1Level.Value}");
                    changed = true;
                }

                if (parsed.Fan2Level.HasValue)
                {
                    config.Fan2ManualLevel = parsed.Fan2Level.Value;
                    Console.WriteLine($"风扇2手动档位已设置为: {parsed.Fan2Level.Value}");
                    changed = true;
                }

                if (parsed.TemperatureControl.HasValue)
                {
                    config.EnableTemperatureControl = parsed.TemperatureControl.Value;
                    Console.WriteLine($"温度控制已设置为: {parsed.TemperatureControl.Value}");
                    changed = true;
                }
            }

            if (changed)
            {
                config.Save();
                Console.WriteLine("配置已保存到注册表");
            }
            else
            {
                Console.WriteLine("未进行任何更改");
                Console.WriteLine();
                Console.WriteLine("可用配置项:");
                Console.WriteLine("  --cpu-threshold <值>      CPU温度阈值（默认45°C）");
                Console.WriteLine("  --gpu-threshold <值>      GPU温度阈值（默认45°C）");
                Console.WriteLine("  --check-interval <秒>     温度检查间隔（默认5秒）");
                Console.WriteLine("  --enable-turbo-protection <true/false>  启用高温睿频保护（默认true）");
                Console.WriteLine("  --trigger-cpu <值>        高温触发CPU温度（默认95°C）");
                Console.WriteLine("  --trigger-gpu <值>        高温触发GPU温度（默认80°C）");
                Console.WriteLine("  --trigger-duration <秒>   高温触发持续时间（默认10秒）");
                Console.WriteLine("  --recovery-cpu <值>       恢复睿频CPU温度（默认80°C）");
                Console.WriteLine("  --recovery-gpu <值>       恢复睿频GPU温度（默认70°C）");
                Console.WriteLine("  --recovery-duration <秒>  恢复持续时间（默认20秒）");
                Console.WriteLine("  --fan1-level <0-2>        风扇1手动档位（0=关,1=中,2=高）");
                Console.WriteLine("  --fan2-level <0-2>        风扇2手动档位（0=关,1=中,2=高）");
                Console.WriteLine("  --temperature-control <true/false>  启用温度控制");
                Console.WriteLine("  --reset                   重置为默认配置");
            }
        }

        /// <summary>
        /// 查看当前状态
        /// </summary>
        private static void StatusCommand()
        {
            Configuration config = new Configuration();
            config.Load();

            Console.WriteLine("=== DellFanService 状态 ===");
            Console.WriteLine();

            try
            {
                using ServiceController sc = new ServiceController(ServiceName);
                Console.WriteLine($"服务状态: {sc.Status}");
            }
            catch
            {
                Console.WriteLine("服务状态: 未安装");
            }

            Console.WriteLine();
            config.Print();
        }

        /// <summary>
        /// 安装Windows服务
        /// </summary>
        private static void InstallService()
        {
            try
            {
                string exePath = Process.GetCurrentProcess().MainModule?.FileName ??
                    AppContext.BaseDirectory + "DellFanService.exe";

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "sc.exe",
                        Arguments = $"create {ServiceName} binPath= \"{exePath} --run\" start= auto displayName= \"{ServiceDisplayName}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Verb = "runas"
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    var descProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "sc.exe",
                            Arguments = $"description {ServiceName} \"{ServiceDescription}\"",
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    descProcess.Start();
                    descProcess.WaitForExit();

                    Console.WriteLine("服务安装成功!");
                    Console.WriteLine($"服务名称: {ServiceName}");
                    Console.WriteLine($"可执行文件: {exePath}");
                    Console.WriteLine();
                    Console.WriteLine("请使用以下命令启动服务:");
                    Console.WriteLine("  dellfan --start");
                }
                else
                {
                    Console.WriteLine("服务安装失败:");
                    if (!string.IsNullOrEmpty(error))
                        Console.WriteLine(error);
                    if (!string.IsNullOrEmpty(output))
                        Console.WriteLine(output);
                    Environment.ExitCode = 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"安装服务时出错: {ex.Message}");
                Environment.ExitCode = 1;
            }
        }

        /// <summary>
        /// 卸载Windows服务
        /// </summary>
        private static void UninstallService()
        {
            try
            {
                StopService();

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "sc.exe",
                        Arguments = $"delete {ServiceName}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Verb = "runas"
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Console.WriteLine("服务卸载成功!");
                }
                else
                {
                    Console.WriteLine("服务卸载失败:");
                    if (!string.IsNullOrEmpty(error))
                        Console.WriteLine(error);
                    if (!string.IsNullOrEmpty(output))
                        Console.WriteLine(output);
                    Environment.ExitCode = 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"卸载服务时出错: {ex.Message}");
                Environment.ExitCode = 1;
            }
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        private static void StartService()
        {
            try
            {
                using ServiceController sc = new ServiceController(ServiceName);
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    Console.WriteLine("服务已经在运行中");
                    return;
                }

                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                Console.WriteLine("服务启动成功!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"启动服务失败: {ex.Message}");
                Console.WriteLine("请确保已使用管理员权限运行");
                Environment.ExitCode = 1;
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        private static void StopService()
        {
            try
            {
                using ServiceController sc = new ServiceController(ServiceName);
                if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    Console.WriteLine("服务已经停止");
                    return;
                }

                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                Console.WriteLine("服务停止成功!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"停止服务失败: {ex.Message}");
                Environment.ExitCode = 1;
            }
        }

        /// <summary>
        /// 重启服务
        /// </summary>
        private static void RestartService()
        {
            StopService();
            Thread.Sleep(1000);
            StartService();
        }

        /// <summary>
        /// 检查服务是否正在运行
        /// </summary>
        private static bool IsServiceRunning()
        {
            try
            {
                using ServiceController sc = new ServiceController(ServiceName);
                return sc.Status == ServiceControllerStatus.Running;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 控制台前台运行模式
        /// </summary>
        private static async Task RunConsoleMode(string[] args)
        {
            Console.WriteLine("=== DellFanService 控制台模式 ===");
            Console.WriteLine("按 Ctrl+C 停止服务");
            Console.WriteLine();

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
                Console.WriteLine();
                Console.WriteLine("正在停止...");
            };

            var host = Host.CreateDefaultBuilder(args)
                .UseWindowsService(options =>
                {
                    options.ServiceName = ServiceName;
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .ConfigureServices(services =>
                {
                    services.AddHostedService<ServiceWorker>();
                })
                .Build();

            try
            {
                await host.RunAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                // 正常退出
            }

            Console.WriteLine("服务已停止");
        }

        /// <summary>
        /// 打印使用帮助
        /// </summary>
        private static void PrintUsage()
        {
            Console.WriteLine("DellFanService - Dell笔记本风扇控制服务");
            Console.WriteLine();
            Console.WriteLine("用法: DellFanService [选项]");
            Console.WriteLine();
            Console.WriteLine("模式切换:");
            Console.WriteLine("  -m, --mode <0-4>          切换风扇模式");
            Console.WriteLine("                              0=手动控制, 1=优化散热, 2=酷凉");
            Console.WriteLine("                              3=静音, 4=极速");
            Console.WriteLine();
            Console.WriteLine("配置参数（配合 -c/--config 使用）:");
            Console.WriteLine("  -c, --config              进入配置模式");
            Console.WriteLine("      --cpu-threshold <值>      CPU温度阈值（默认45°C）");
            Console.WriteLine("      --gpu-threshold <值>      GPU温度阈值（默认45°C）");
            Console.WriteLine("      --check-interval <秒>     温度检查间隔（默认5秒）");
            Console.WriteLine("      --enable-turbo-protection <true/false>  启用高温睿频保护（默认true）");
            Console.WriteLine("      --trigger-cpu <值>        高温触发CPU温度（默认95°C）");
            Console.WriteLine("      --trigger-gpu <值>        高温触发GPU温度（默认80°C）");
            Console.WriteLine("      --trigger-duration <秒>   高温触发持续时间（默认10秒）");
            Console.WriteLine("      --recovery-cpu <值>       恢复睿频CPU温度（默认80°C）");
            Console.WriteLine("      --recovery-gpu <值>       恢复睿频GPU温度（默认70°C）");
            Console.WriteLine("      --recovery-duration <秒>  恢复持续时间（默认20秒）");
            Console.WriteLine("      --fan1-level <0-2>        风扇1手动档位（0=关,1=中,2=高）");
            Console.WriteLine("      --fan2-level <0-2>        风扇2手动档位（0=关,1=中,2=高）");
            Console.WriteLine("      --temperature-control <true/false>  启用温度控制");
            Console.WriteLine("      --reset                   重置为默认配置");
            Console.WriteLine();
            Console.WriteLine("状态查看:");
            Console.WriteLine("  -s, --status              查看当前配置和服务状态");
            Console.WriteLine();
            Console.WriteLine("服务管理（需要管理员权限）:");
            Console.WriteLine("  --install                 安装为Windows服务");
            Console.WriteLine("  --uninstall               卸载Windows服务");
            Console.WriteLine("  --start                   启动服务");
            Console.WriteLine("  --stop                    停止服务");
            Console.WriteLine("  --restart                 重启服务");
            Console.WriteLine("  --run                     前台运行（控制台模式，按Ctrl+C停止）");
            Console.WriteLine();
            Console.WriteLine("其他:");
            Console.WriteLine("  -h, --help                显示此帮助信息");
            Console.WriteLine();
            Console.WriteLine("示例:");
            Console.WriteLine("  DellFanService -m 1                    切换到优化散热模式");
            Console.WriteLine("  DellFanService -m 0                    切换到手动控制模式");
            Console.WriteLine("  DellFanService -c --cpu-threshold 50 --fan1-level 1");
            Console.WriteLine("  DellFanService --install               安装服务");
            Console.WriteLine("  DellFanService --start                 启动服务");
            Console.WriteLine("  DellFanService --run                   前台运行");
        }

        /// <summary>
        /// 解析后的参数
        /// </summary>
        private class ParsedArgs
        {
            public int? Mode { get; set; }
            public bool ShowStatus { get; set; }
            public bool IsConfig { get; set; }
            public bool IsInstall { get; set; }
            public bool IsUninstall { get; set; }
            public bool IsStart { get; set; }
            public bool IsStop { get; set; }
            public bool IsRestart { get; set; }
            public bool IsRun { get; set; }
            public bool ShowHelp { get; set; }
            public bool IsReset { get; set; }

            // 配置值
            public int? CpuThreshold { get; set; }
            public int? GpuThreshold { get; set; }
            public int? CheckInterval { get; set; }
            public bool? EnableTurboProtection { get; set; }
            public int? TriggerCpu { get; set; }
            public int? TriggerGpu { get; set; }
            public int? TriggerDuration { get; set; }
            public int? RecoveryCpu { get; set; }
            public int? RecoveryGpu { get; set; }
            public int? RecoveryDuration { get; set; }
            public int? Fan1Level { get; set; }
            public int? Fan2Level { get; set; }
            public bool? TemperatureControl { get; set; }
        }
    }
}
