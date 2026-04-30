# DellThermalMode Startup Unregistration Script
# Run as Administrator

$ErrorActionPreference = "Stop"

# Check admin privileges
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "This script must be run as Administrator."
    exit 1
}

# ============================================================================
# Remove Task Scheduler task
# ============================================================================
$taskName = "DellThermalModeAtBoot"
$task = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue
if ($task) {
    Write-Host "Removing Task Scheduler task '$taskName'..."
    Unregister-ScheduledTask -TaskName $taskName -Confirm:$false
    Write-Host "Task removed."
} else {
    Write-Host "Task '$taskName' not found."
}

# ============================================================================
# Remove Registry Run key (if exists)
# ============================================================================
$regPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"
$regName = "DellThermalMode"
$regValue = Get-ItemProperty -Path $regPath -Name $regName -ErrorAction SilentlyContinue
if ($regValue) {
    Write-Host "Removing Registry Run key..."
    Remove-ItemProperty -Path $regPath -Name $regName -Force
    Write-Host "Registry entry removed."
}

# ============================================================================
# Note about log files
# ============================================================================
$logDir = "C:\ProgramData\DellThermalMode"
if (Test-Path $logDir) {
    Write-Host "`nLog files remain at: $logDir"
    Write-Host "You may delete this directory manually if desired."
}

Write-Host "`nUnregistration complete."
