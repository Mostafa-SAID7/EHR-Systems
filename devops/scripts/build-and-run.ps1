# =============================================================================
# EHR Platform: Complete Build & Docker Startup Script
# Builds all 10 microservices locally and starts the full observability stack
# =============================================================================

param(
    [switch]$SkipBuild = $false,      # Skip building, just start containers
    [switch]$CleanBuild = $false,     # Clean build (remove obj/bin folders)
    [switch]$Monitoring = $true,      # Include monitoring stack (default: yes)
    [int]$ParallelBuilds = 4          # Parallel build jobs
)

$ErrorActionPreference = "Stop"
$WarningPreference = "SilentlyContinue"

function Write-Status {
    param([string]$Message)
    Write-Host "[OK] $Message"
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Write-Section {
    param([string]$Title)
    Write-Host "`n========================================`n-> $Title`n========================================`n"
}

# =============================================================================
# STEP 1: Verify Prerequisites
# =============================================================================
Write-Section "Verifying Prerequisites"

# Check Docker
try {
    $dockerVersion = docker version --format='{{.Server.Version}}' 2>$null
    Write-Status "Docker Engine $dockerVersion"
} catch {
    Write-Error-Custom "Docker is not running. Please start Docker Desktop."
    exit 1
}

# Check .NET SDK
try {
    $dotnetVersion = dotnet --version 2>$null
    Write-Status ".NET SDK $dotnetVersion"
} catch {
    Write-Error-Custom ".NET SDK not found. Install from https://dotnet.microsoft.com/download"
    exit 1
}

# Check Git
try {
    $gitVersion = git --version 2>$null
    Write-Status "Git $($gitVersion -replace 'git version ', '')"
} catch {
    Write-Error-Custom "Git not found"
    exit 1
}

# =============================================================================
# STEP 2: Clean Build (Optional)
# =============================================================================
if ($CleanBuild) {
    Write-Section "Cleaning Previous Builds"
    
    Get-ChildItem -Path "backend/src" -Recurse -Include "bin", "obj" | Remove-Item -Recurse -Force
    Write-Status "Cleaned bin and obj folders"
    
    # Clean Docker images
    docker image prune -f --filter "label=service=ehr" 2>$null
    Write-Status "Cleaned old Docker images"
}

# =============================================================================
# STEP 3: Build Backend Services
# =============================================================================
if (-not $SkipBuild) {
    Write-Section "Building 10 Microservices (Parallel: $ParallelBuilds)"
    
    $services = @(
        "EHRPlatform.Services.ApiGateway",
        "EHRPlatform.Services.Identity",
        "EHRPlatform.Services.Patient",
        "EHRPlatform.Services.Clinical",
        "EHRPlatform.Services.Appointment",
        "EHRPlatform.Services.Notification",
        "EHRPlatform.Services.Audit",
        "EHRPlatform.Services.Billing",
        "EHRPlatform.Services.Prescription",
        "EHRPlatform.Services.Analytics"
    )
    
    $serviceCount = $services.Count
    $completed = 0
    
    # Build each service
    foreach ($service in $services) {
        $completed++
        Write-Host "[$completed/$serviceCount] Building $service..."
        
        $projectPath = "backend/src/$service/$service.csproj"
        
        if (-not (Test-Path $projectPath)) {
            Write-Error-Custom "Project file not found: $projectPath"
            exit 1
        }
        
        dotnet build $projectPath -c Release | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Error-Custom "Failed to build $service"
            exit 1
        }
        
        Write-Status "$service compiled"
    }
    
    Write-Status "All 10 services built successfully"
}

# =============================================================================
# STEP 4: Build Docker Images (using docker-compose)
# =============================================================================
Write-Section "Building Docker Images"

Write-Host "Building backend services from Dockerfile..."
docker compose -f devops/docker/docker-compose.yml build

if ($LASTEXITCODE -ne 0) {
    Write-Error-Custom "Docker build failed"
    exit 1
}

Write-Status "All Docker images built"

# =============================================================================
# STEP 5: Start the Stack
# =============================================================================
Write-Section "Starting EHR Platform Stack"

if ($Monitoring) {
    Write-Host "Starting with monitoring (Prometheus, Grafana, Loki, Tempo, OTEL)..."
    docker compose -f devops/docker/docker-compose.yml --profile monitoring up -d
} else {
    Write-Host "Starting without monitoring..."
    docker compose -f devops/docker/docker-compose.yml up -d
}

if ($LASTEXITCODE -ne 0) {
    Write-Error-Custom "Failed to start containers"
    exit 1
}

Write-Status "Containers started"

# =============================================================================
# STEP 6: Wait for Services to Be Ready
# =============================================================================
Write-Section "Waiting for Services to Initialize (2-5 minutes)"

$maxWaitTime = 300  # 5 minutes
$elapsed = 0

while ($elapsed -lt $maxWaitTime) {
    $ps = docker compose -f devops/docker/docker-compose.yml --profile monitoring ps --format "json" 2>$null | ConvertFrom-Json
    
    if ($null -eq $ps) {
        Start-Sleep -Seconds 5
        $elapsed += 5
        continue
    }
    
    $healthy = $ps | Where-Object { $_.State -in @("running", "healthy") } | Measure-Object | Select-Object -ExpandProperty Count
    $total = $ps | Measure-Object | Select-Object -ExpandProperty Count
    
    Write-Host "Status: $healthy/$total services ready..." -NoNewline -ForegroundColor Cyan
    Write-Host "`r" -NoNewline
    
    if ($healthy -eq $total) {
        Write-Host "`n"
        Write-Status "All services are ready!"
        break
    }
    
    Start-Sleep -Seconds 5
    $elapsed += 5
}

if ($elapsed -ge $maxWaitTime) {
    Write-Host "`n"
    Write-Error-Custom "Timeout waiting for services. Check logs: docker compose logs"
    exit 1
}

# =============================================================================
# STEP 7: Display Dashboard URLs
# =============================================================================
Write-Section "Platform is Running"

Write-Host @"
==================================================
EHR PLATFORM - MICROSERVICES & OBSERVABILITY
==================================================

OBSERVABILITY DASHBOARDS:
  * Grafana (Dashboards):     http://localhost:3001 (admin/admin)
  * Prometheus (Metrics):     http://localhost:9090
  * Loki (Logs):              http://localhost:3100
  * Tempo (Traces):           http://localhost:3200
  * Kafka UI:                 http://localhost:8090
  * RabbitMQ Management:      http://localhost:15672 (ehr_user/ehr_password)
  * Kibana (Elasticsearch):   http://localhost:5601

MICROSERVICES (Port 5000-5009):
  * API Gateway:              http://localhost:5000/health
  * Identity Service:         http://localhost:5001/health
  * Patient Service:          http://localhost:5002/health
  * Clinical Service:         http://localhost:5003/health
  * Appointment Service:      http://localhost:5004/health
  * Notification Service:     http://localhost:5005/health
  * Audit Service:            http://localhost:5006/health
  * Billing Service:          http://localhost:5007/health
  * Prescription Service:     http://localhost:5008/health
  * Analytics Service:        http://localhost:5009/health

DATABASES:
  * PostgreSQL (5 instances): localhost:5432-5436
  * MongoDB:                  localhost:27017
  * MySQL (Billing):          localhost:3306
  * Redis:                    localhost:6379

MESSAGE QUEUES:
  * RabbitMQ:                 localhost:5672
  * Kafka:                    localhost:9092

USEFUL COMMANDS:
  # View logs
  docker compose -f devops/docker/docker-compose.yml --profile monitoring logs -f [service]

  # Check service status
  docker compose -f devops/docker/docker-compose.yml --profile monitoring ps

  # Stop all
  docker compose -f devops/docker/docker-compose.yml --profile monitoring down

  # Stop and clean
  docker compose -f devops/docker/docker-compose.yml --profile monitoring down -v

SUCCESS: All services running!
"@

# =============================================================================
# STEP 8: Verify Observability Pipeline
# =============================================================================
Write-Section "Verifying Observability Pipeline"

Start-Sleep -Seconds 5

# Check Prometheus
try {
    $promResult = Invoke-WebRequest -Uri "http://localhost:9090/api/v1/query?query=up" -TimeoutSec 5 -ErrorAction Stop
    Write-Status "Prometheus is receiving metrics"
} catch {
    Write-Error-Custom "Prometheus not responding (this may be OK, give it more time)"
}

# Check Grafana
try {
    $grafanaResult = Invoke-WebRequest -Uri "http://localhost:3001/api/health" -TimeoutSec 5 -ErrorAction Stop
    Write-Status "Grafana is ready"
} catch {
    Write-Error-Custom "Grafana not responding (this may be OK, give it more time)"
}

# Check OTEL Collector
try {
    $otelResult = docker logs ehr-otel-collector 2>&1 | Select-String "accepted_metric_points" | Select-Object -First 1
    if ($otelResult) {
        Write-Status "OTEL Collector is receiving telemetry"
    }
} catch {
    Write-Error-Custom "OTEL Collector not responding (this may be OK, give it more time)"
}

Write-Host "`n========================================`n"

Write-Host @"
NEXT STEPS:
  1. Open Grafana: http://localhost:3001
  2. Login: admin / admin
  3. Navigate to Dashboards > EHR Platform folder
  4. Select a dashboard (Infrastructure, API, Database, RabbitMQ, Business)
  5. Generate traffic to see metrics populate

GENERATE TRAFFIC:
  # Create a patient (generates distributed traces)
  curl -X POST http://localhost:5000/api/patients -H "Content-Type: application/json" -d '{\"firstName\":\"John\",\"lastName\":\"Doe\",\"dateOfBirth\":\"1990-01-01\"}'

  # View trace in Grafana > Explore > Tempo

NEED HELP:
  * Check logs:  docker compose logs -f [service]
  * Restart:     docker compose down; .\devops\scripts\build-and-run.ps1
  * Full reset:  docker compose down -v; .\devops\scripts\build-and-run.ps1 -CleanBuild
"@
