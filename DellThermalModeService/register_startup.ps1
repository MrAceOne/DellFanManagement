# DellThermalMode Startup Registration Script
# Registers the command-line tool to run at system startup
# Run as Administrator

param(
    [Parameter()]
    [ValidateSet("Optimized", "Cool", "Quiet", "Performance")]
    [string]$Mode = "Quiet",

    [Parameter()]
    [string]$ExePath = $null
)

$ErrorActionPreference = "Stop"

# Map mode name to value
$modeMap = @{
    "Optimized"   = 1
    "Cool"        = 2
    "Quiet"       = 4
    "Performance" = 8
}
$modeValue = $modeMap[$Mode]

# Determine executable path
if (-not $ExePath) {
    $ExePath = Join-Path $PSScriptRoot "DellThermalMode.exe"
}

if (-not (Test-Path $ExePath)) {
    Write-Error "Executable not found: $ExePath"
    exit 1
}

$ExePath = (Resolve-Path $ExePath).Path
Write-Host "Executable: $ExePath"
Write-Host "Thermal mode: $Mode ($modeValue)"

# Check admin privileges
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "This script must be run as Administrator."
    exit 1
}

# ============================================================================
# Method 1: Task Scheduler (Recommended - runs at boot with SYSTEM privileges)
# ============================================================================
Write-Host "`nRegistering via Task Scheduler..."

$taskName = "DellThermalModeAtBoot"
$action = New-ScheduledTaskAction -Execute $ExePath -Argument "$modeValue"

# Trigger: At system startup
$trigger = New-ScheduledTaskTrigger -AtStartup

# Run with highest privileges as SYSTEM
$principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -LogonType ServiceAccount -RunLevel Highest

# Settings: allow start on battery, don't stop on idle
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable

# Remove existing task
Unregister-ScheduledTask -TaskName $taskName -Confirm:$false -ErrorAction SilentlyContinue

# Register new task
Register-ScheduledTask -TaskName $taskName `
    -Action $action `
    -Trigger $trigger `
    -Principal $principal `
    -Settings $settings `
    -Force | Out-Null

Write-Host "Task '$taskName' registered successfully."

# ============================================================================
# Method 2: Registry Run key (Alternative - runs at user logon)
# ============================================================================
# Uncomment below if you prefer registry method instead of Task Scheduler
<#
Write-Host "`nRegistering via Registry Run key..."
$regPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"
$regName = "DellThermalMode"
$regValue = ""$ExePath" $modeValue"
Set-ItemProperty -Path $regPath -Name $regName -Value $regValue
Write-Host "Registry entry added: $regPath\$regName"
#>

# ============================================================================
# Test run
# ============================================================================
Write-Host "`nTesting execution..."
$process = Start-Process -FilePath $ExePath -ArgumentList "$modeValue" -Wait -PassThru -WindowStyle Hidden
Write-Host "Exit code: $($process.ExitCode)"

# Check log
$logPath = "C:\ProgramData\DellThermalMode\set_thermal_mode.log"
if (Test-Path $logPath) {
    Write-Host "`nRecent log entries:"
    Get-Content $logPath -Tail 5
}

Write-Host "`nRegistration complete!"
Write-Host "The tool will run automatically at every system boot."
Write-Host "Mode: $Mode ($modeValue)"
