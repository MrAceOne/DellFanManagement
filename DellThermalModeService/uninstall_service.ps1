# DellThermalModeService Uninstallation Script
# Run as Administrator

$ErrorActionPreference = "Stop"

# Check admin privileges
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "This script must be run as Administrator."
    exit 1
}

$serviceName = "DellThermalModeSvc"

# Check if service exists
$existingService = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
if (-not $existingService) {
    Write-Host "Service '$serviceName' does not exist."
    exit 0
}

# Stop service
Write-Host "Stopping service..."
Stop-Service -Name $serviceName -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

# Delete service
Write-Host "Removing service..."
sc.exe delete $serviceName | Out-Null

if ($LASTEXITCODE -eq 0) {
    Write-Host "Service removed successfully."
} else {
    Write-Warning "sc.exe delete returned exit code $LASTEXITCODE"
}

# Clean up registry
$regPath = "HKLM:\SYSTEM\CurrentControlSet\Services\DellThermalModeSvc"
if (Test-Path $regPath) {
    Remove-Item -Path $regPath -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Registry entries cleaned up."
}

# Note about log files
$logDir = "C:\ProgramData\DellThermalModeSvc"
if (Test-Path $logDir) {
    Write-Host "`nLog files remain at: $logDir"
    Write-Host "You may delete this directory manually if desired."
}

Write-Host "`nUninstallation complete."
