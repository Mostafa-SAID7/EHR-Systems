# =============================================================================
# Docker Stack Shutdown: Clean, layered stop
# =============================================================================

param(
    [ValidateSet("all", "services", "monitoring", "infrastructure")]
    [string]$Layer = "all",
    
    [switch]$Volumes = $false,
    [switch]$Images = $false
)

$DockerDir = Split-Path -Parent $PSCommandPath | Split-Path -Parent | Join-Path -ChildPath "docker"

$ComposeArgs = "-f '$DockerDir/1-infrastructure.yml' -f '$DockerDir/2-monitoring.yml' -f '$DockerDir/3-services.yml'"

# Build down command
$DownCmd = "docker-compose $ComposeArgs down"
if ($Volumes) { $DownCmd += " -v" }
if ($Images) { $DownCmd += " --rmi all" }

Write-Host "🛑 Stopping EHR Platform Stack..." -ForegroundColor Yellow

# Stop in reverse order (services → monitoring → infrastructure)
if ($Layer -in "all", "services") {
    Write-Host "   Stopping services..." -ForegroundColor Cyan
    docker-compose -f "$DockerDir/3-services.yml" down $(if ($Volumes) { "-v" }) $(if ($Images) { "--rmi all" })
}

if ($Layer -in "all", "monitoring") {
    Write-Host "   Stopping monitoring..." -ForegroundColor Cyan
    docker-compose -f "$DockerDir/2-monitoring.yml" down $(if ($Volumes) { "-v" }) $(if ($Images) { "--rmi all" })
}

if ($Layer -in "all", "infrastructure") {
    Write-Host "   Stopping infrastructure..." -ForegroundColor Cyan
    docker-compose -f "$DockerDir/1-infrastructure.yml" down $(if ($Volumes) { "-v" }) $(if ($Images) { "--rmi all" })
}

if ($Layer -eq "all") {
    Write-Host "   Removing network..." -ForegroundColor Cyan
    docker network rm ehr-network 2>$null
}

Write-Host "✅ Stack stopped" -ForegroundColor Green
