# =============================================================================
# Docker Stack Status: Health check, diagnostics
# =============================================================================

Write-Host "🔍 EHR Platform Docker Stack Status" -ForegroundColor Cyan
Write-Host "═════════════════════════════════════════════════════════════" -ForegroundColor Gray

# Container status
Write-Host "`n📦 Container Status:" -ForegroundColor Cyan
$containers = docker ps -a --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | grep "ehr-"
if ($containers) {
    $containers
    $healthyCount = docker ps --format "table {{.Status}}" | grep "Up" | Measure-Object | Select-Object -ExpandProperty Count
    Write-Host "`n✅ $healthyCount containers running" -ForegroundColor Green
} else {
    Write-Host "❌ No EHR containers running" -ForegroundColor Red
}

# Network status
Write-Host "`n🔗 Network Status:" -ForegroundColor Cyan
$network = docker network inspect ehr-network 2>$null | ConvertFrom-Json
if ($network) {
    Write-Host "   Network: ehr-network (OK)" -ForegroundColor Green
    $containerCount = $network.Containers | Measure-Object | Select-Object -ExpandProperty Count
    Write-Host "   Connected containers: $containerCount" -ForegroundColor Gray
} else {
    Write-Host "   Network: ehr-network (not found)" -ForegroundColor Yellow
}

# Service endpoints
Write-Host "`n🌐 Service Endpoints:" -ForegroundColor Cyan
$endpoints = @(
    @{ Name = "API Gateway"; URL = "http://localhost:5000/health" },
    @{ Name = "Identity"; URL = "http://localhost:5001/health" },
    @{ Name = "Patient"; URL = "http://localhost:5002/health" },
    @{ Name = "Grafana"; URL = "http://localhost:3001" },
    @{ Name = "Prometheus"; URL = "http://localhost:9090" },
    @{ Name = "Loki"; URL = "http://localhost:3100" },
    @{ Name = "RabbitMQ"; URL = "http://localhost:15672" }
)

foreach ($endpoint in $endpoints) {
    $health = curl.exe -s -o /dev/null -w "%{http_code}" $endpoint.URL 2>$null
    $status = if ($health -in "200", "301", "302", "404") { "✅" } else { "❌" }
    Write-Host "   $status $($endpoint.Name): $($endpoint.URL) ($health)" -ForegroundColor Gray
}

# Resource usage
Write-Host "`n💾 Resource Usage:" -ForegroundColor Cyan
docker stats --no-stream --format "table {{.Container}}\t{{.MemUsage}}\t{{.CPUPerc}}" ehr-* 2>$null | Select-Object -First 11

Write-Host "`n" -ForegroundColor Gray
