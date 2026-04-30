# DellThermalMode - 命令行工具

C++ 命令行工具，通过 WMI 调用 Dell BIOS 的 `BFn.DoBFn` 接口设置系统温度模式（静音/性能等）。

**不是 Windows Service**，只是一个普通命令行程序，可通过 Task Scheduler 在系统启动时自动执行一次。

## 编译

```bash
cd DellThermalModeService
mkdir build && cd build
cmake .. -DCMAKE_BUILD_TYPE=Release
cmake --build . --config Release
```

或使用 MSVC 直接编译：

```cmd
cl.exe /EHsc /O2 /W4 /DUNICODE /D_UNICODE /Fe:DellThermalMode.exe DellThermalModeService.cpp /link wbemuuid.lib
```

## 用法

### 直接运行

```powershell
# 设置静音模式（默认）
.\DellThermalMode.exe

# 设置指定模式
.\DellThermalMode.exe 1   # 均衡
.\DellThermalMode.exe 2   # 酷凉
.\DellThermalMode.exe 4   # 静音
.\DellThermalMode.exe 8   # 极速
```

### 注册为开机启动（推荐：Task Scheduler）

以管理员身份运行 PowerShell：

```powershell
# 注册为开机启动，默认静音模式
.\register_startup.ps1

# 注册为极速模式
.\register_startup.ps1 -Mode Performance
```

这会创建一个 **Task Scheduler 任务**：
- 触发时机：系统启动时（AtStartup）
- 运行身份：SYSTEM
- 执行一次后退出，不驻留内存

### 取消开机启动

```powershell
.\unregister_startup.ps1
```

## 温度模式值

| 值 | 枚举名 | 说明 |
|----|--------|------|
| 1 | Optimized | 均衡（默认） |
| 2 | Cool | 酷凉 |
| 4 | Quiet | 静音 |
| 8 | Performance | 极速 |

## 日志

执行日志写入：
```
C:\ProgramData\DellThermalMode\set_thermal_mode.log
```

## 工作原理

```
DellThermalMode.exe
  ↓ COM/WMI
ROOT\WMI\BFn.DoBFn
  ↓ Windows ACPI-WMI (wmiprov.dll / acpi.sys)
ACPI 设备 PNP0C14
  ↓ BIOS SMI Handler
设置 EC 温度模式
```

与 C# 项目中的 [`DellSmbiosSmi.SetThermalSetting()`](DellSmbiosSmiLib/DellSmbiosSmi.cs:58) 调用的是同一个底层接口。

## 与 C# 项目的对应关系

| C# (DellSmbiosSmiLib) | C++ (本工具) |
|-----------------------|-------------|
| `Class.Info = 17` | `smi.Class = 17` |
| `Selector.ThermalMode = 19` | `smi.Selector = 19` |
| `Input1 = 1` | `smi.Input1 = 1` |
| `Input2 = (uint)ThermalSetting.Quiet` | `smi.Input2 = 4` |
| `ManagementObject.InvokeMethod("DoBFn", ...)` | `IWbemServices::ExecMethod(..., L"DoBFn", ...)` |
