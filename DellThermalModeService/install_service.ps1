# DellThermalModeService Installation Script
# Run as Administrator

param(
    [Parameter()]
    [ValidateSet("Optimized", "Cool", "Quiet", "Performance")]
    [string]$Mode = "Quiet",

    [Parameter()]
    [string]$ServicePath = $null
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

# Determine service executable path
if (-not $ServicePath) {
    $ServicePath = Join-Path $PSScriptRoot "DellThermalModeService.exe"
}

if (-not (Test-Path $ServicePath)) {
    Write-Error "Service executable not found: $ServicePath"
    exit 1
}

$ServicePath = (Resolve-Path $ServicePath).Path
Write-Host "Service executable: $ServicePath"
Write-Host "Thermal mode: $Mode ($modeValue)"

# Check admin privileges
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "This script must be run as Administrator."
    exit 1
}

# Stop and remove existing service
$existingService = Get-Service -Name "DellThermalModeSvc" -ErrorAction SilentlyContinue
if ($existingService) {
    Write-Host "Stopping existing service..."
    Stop-Service -Name "DellThermalModeSvc" -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2

    Write-Host "Removing existing service..."
    sc.exe delete "DellThermalModeSvc" | Out-Null
    Start-Sleep -Seconds 2
}

# Create service
Write-Host "Creating service..."
$desc = "Sets Dell thermal mode (Quiet/Performance/etc.) at system boot"
sc.exe create "DellThermalModeSvc" `
    binPath= "$ServicePath" `
    start= auto `
    displayname= "Dell Thermal Mode Service" `
    type= own | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to create service"
    exit 1
}

# Set description
sc.exe description "DellThermalModeSvc" "$desc" | Out-Null

# Create registry config key
$regPath = "HKLM:\SYSTEM\CurrentControlSet\Services\DellThermalModeSvc\Config"
if (-not (Test-Path $regPath)) {
    New-Item -Path $regPath -Force | Out-Null
}
Set-ItemProperty -Path $regPath -Name "ThermalMode" -Value $modeValue -Type DWord

Write-Host "Registry config set: ThermalMode = $modeValue"

# Start service
Write-Host "Starting service..."
Start-Service -Name "DellThermalModeSvc"
Start-Sleep -Seconds 2

$service = Get-Service -Name "DellThermalModeSvc"
Write-Host "Service status: $($service.Status)"

# Check log
$logPath = "C:\ProgramData\DellThermalModeSvc\service.log"
if (Test-Path $logPath) {
    Write-Host "`nRecent log entries:"
    Get-Content $logPath -Tail 10
}

Write-Host "`nInstallation complete!"
Write-Host "The service will automatically set thermal mode to '$Mode' at every boot."
