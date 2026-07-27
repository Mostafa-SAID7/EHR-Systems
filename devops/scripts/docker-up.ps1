# =============================================================================
# Docker Stack Startup: Modular, Layered, Single Responsibility
# =============================================================================
# Layers:
# 1. Infrastructure (30s) - databases, cache, messaging
# 2. Monitoring (20s) - prometheus, grafana, loki, tempo, otel
# 3. Services (15s) - 10 microservices
#
# Total time: ~65 seconds (sequential layers)
# =============================================================================

param(
    [ValidateSet("all", "infrastructure", "monitoring", "services", "clean")]
    [string]$Layer = "all",
    
    [switch]$Wait = $false,
    [int]$Timeout = 120
)

$DockerDir = Split-Path -Parent $PSCommandPath | Split-Path -Parent | Join-Path -ChildPath "docker"
$EnvFile = "$DockerDir\.env"

# Load environment variables
if (Test-Path $EnvFile) {
    $env:COMPOSE_PROJECT_NAME = "ehr-platform"
    Write-Host "📋 Loaded .env from $EnvFile" -ForegroundColor Green
}

# Create shared network first
Write-Host "🔗 Creating shared network..." -ForegroundColor Cyan
docker network create ehr-network 2>$null | Out-Null

# ─────────────────────────────────────────────────────────────────────────
# Layer 1: Infrastructure (30 seconds)
# ─────────────────────────────────────────────────────────────────────────
function Start-Infrastructure {
    Write-Host "`n📦 Layer 1: Infrastructure (Databases, Cache, Messaging)" -ForegroundColor Cyan
    Write-Host "   Starting: PostgreSQL, MongoDB, MySQL, Redis, RabbitMQ, Kafka, Elasticsearch" -ForegroundColor Gray
    
    docker-compose -f "$DockerDir/1-infrastructure.yml" up -d
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Infrastructure layer started" -ForegroundColor Green
        if ($Wait) {
            Write-Host "   ⏳ Waiting for health checks..." -ForegroundColor Yellow
            Start-Sleep -Seconds 30
        }
    } else {
        Write-Host "❌ Failed to start infrastructure" -ForegroundColor Red
        exit 1
    }
}

# ─────────────────────────────────────────────────────────────────────────
# Layer 2: Monitoring (20 seconds)
# ─────────────────────────────────────────────────────────────────────────
function Start-Monitoring {
    Write-Host "`n📊 Layer 2: Monitoring (Prometheus, Grafana, Loki, Tempo, OTEL)" -ForegroundColor Cyan
    
    docker-compose -f "$DockerDir/2-monitoring.yml" up -d
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Monitoring layer started" -ForegroundColor Green
        if ($Wait) {
            Write-Host "   ⏳ Waiting for dashboards..." -ForegroundColor Yellow
            Start-Sleep -Seconds 20
        }
    } else {
        Write-Host "❌ Failed to start monitoring" -ForegroundColor Red
        exit 1
    }
}

# ─────────────────────────────────────────────────────────────────────────
# Layer 3: Services (15 seconds)
# ─────────────────────────────────────────────────────────────────────────
function Start-Services {
    Write-Host "`n🚀 Layer 3: Microservices (10 services on ports 5000-5009)" -ForegroundColor Cyan
    Write-Host "   Services: Gateway, Identity, Patient, Clinical, Appointment, Prescription, Billing, Audit, Notification, Analytics" -ForegroundColor Gray
    
    Write-Host "   🔨 Building service images (first run only)..." -ForegroundColor Yellow
    docker-compose -f "$DockerDir/3-services.yml" build --parallel 2>&1 | Select-String "Successfully|error" -First 5
    
    docker-compose -f "$DockerDir/3-services.yml" up -d
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Services layer started" -ForegroundColor Green
        if ($Wait) {
            Write-Host "   ⏳ Waiting for services to be ready..." -ForegroundColor Yellow
            Start-Sleep -Seconds 15
        }
    } else {
        Write-Host "❌ Failed to start services" -ForegroundColor Red
        exit 1
    }
}

# ─────────────────────────────────────────────────────────────────────────
# Main orchestration
# ─────────────────────────────────────────────────────────────────────────
$StartTime = Get-Date

switch ($Layer) {
    "all" {
        Start-Infrastructure
        Start-Monitoring
        Start-Services
    }
    "infrastructure" {
        Start-Infrastructure
    }
    "monitoring" {
        Start-Monitoring
    }
    "services" {
        Start-Services
    }
    "clean" {
        Write-Host "🧹 Cleaning up all containers..." -ForegroundColor Yellow
        docker-compose -f "$DockerDir/1-infrastructure.yml" down
        docker-compose -f "$DockerDir/2-monitoring.yml" down
        docker-compose -f "$DockerDir/3-services.yml" down
        docker network rm ehr-network 2>$null
        Write-Host "✅ Cleanup complete" -ForegroundColor Green
        exit 0
    }
}

# Summary
$Duration = (Get-Date) - $StartTime
Write-Host "`n" -ForegroundColor Gray
Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "🎉 EHR Platform Stack Started in $($Duration.TotalSeconds) seconds" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "`n📍 Quick Links:" -ForegroundColor Cyan
Write-Host "   API Gateway:      http://localhost:5000/swagger" -ForegroundColor Gray
Write-Host "   Grafana:          http://localhost:3001 (admin/admin)" -ForegroundColor Gray
Write-Host "   Prometheus:       http://localhost:9090" -ForegroundColor Gray
Write-Host "   Loki:             http://localhost:3100" -ForegroundColor Gray
Write-Host "   Tempo:            http://localhost:3200" -ForegroundColor Gray
Write-Host "   RabbitMQ:         http://localhost:15672 (guest/guest)" -ForegroundColor Gray
Write-Host "   MongoDB:          mongodb://ehr_admin:password@localhost:27017" -ForegroundColor Gray
Write-Host "`n📊 Docker Status:" -ForegroundColor Cyan
docker ps --format "table {{.Names}}\t{{.Image}}\t{{.Status}}\t{{.Ports}}" | Select-String "ehr-"
Write-Host "`n💡 Stop stack: docker-compose -f devops/docker/{1,2,3}-*.yml down" -ForegroundColor Yellow
