# =============================================================================
# Kubernetes Deployment Script: Deploy EHR Platform to K8s
# Supports: dev, staging, production environments
# =============================================================================

param(
    [ValidateSet("dev", "staging", "prod")]
    [string]$Environment = "dev",
    
    [switch]$Wait = $false,
    [int]$Timeout = 300,
    [switch]$Dry = $false
)

$K8sDir = Split-Path -Parent $PSCommandPath

function Write-Status {
    param([string]$Message, [string]$Status = "INFO")
    $colors = @{
        "INFO" = "Cyan"
        "SUCCESS" = "Green"
        "ERROR" = "Red"
        "WARN" = "Yellow"
    }
    Write-Host "[$Status]" -ForegroundColor $colors[$Status] -NoNewline
    Write-Host " $Message"
}

# Validate kubectl
Write-Status "Checking kubectl..."
try {
    $kubeVersion = kubectl version --short 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "kubectl not found or cluster unreachable"
    }
    Write-Status "kubectl OK: $kubeVersion" "SUCCESS"
} catch {
    Write-Status "Error: kubectl not available. Install kubectl and configure kubeconfig." "ERROR"
    exit 1
}

# Validate cluster connectivity
Write-Status "Checking cluster connectivity..."
$nodes = kubectl get nodes -o json 2>$null | ConvertFrom-Json
if ($nodes.items.Count -eq 0) {
    Write-Status "Error: No Kubernetes nodes found. Ensure cluster is running." "ERROR"
    exit 1
}
Write-Status "Cluster has $($nodes.items.Count) node(s)" "SUCCESS"

# Prepare namespace
$namespace = if ($Environment -eq "prod") { "ehr-platform-prod" } elseif ($Environment -eq "staging") { "ehr-platform-staging" } else { "ehr-platform" }
Write-Status "Target namespace: $namespace"

# Build kustomize path
$kustomizePath = if ($Environment -eq "dev") {
    "$K8sDir/overlays/dev"
} elseif ($Environment -eq "staging") {
    "$K8sDir/overlays/staging"
} elseif ($Environment -eq "prod") {
    "$K8sDir/overlays/production"
} else {
    "$K8sDir"
}

Write-Status "Using configuration: $kustomizePath"

# Generate manifests
Write-Status "Generating manifests..."
$manifests = kubectl kustomize $kustomizePath 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Status "Error generating manifests: $manifests" "ERROR"
    exit 1
}

# Dry run
if ($Dry) {
    Write-Status "DRY RUN MODE - No changes will be applied" "WARN"
    Write-Host "`n=== GENERATED MANIFESTS ===" -ForegroundColor Cyan
    Write-Host $manifests
    Write-Host "=== END MANIFESTS ===" -ForegroundColor Cyan
    exit 0
}

# Apply manifests
Write-Status "Deploying to cluster..."
$manifests | kubectl apply -f -

if ($LASTEXITCODE -ne 0) {
    Write-Status "Error deploying manifests" "ERROR"
    exit 1
}

Write-Status "Deployment submitted" "SUCCESS"

# Wait for readiness
if ($Wait) {
    Write-Status "Waiting for deployment to be ready (timeout: ${Timeout}s)..."
    
    $startTime = Get-Date
    $allReady = $false
    
    while ((Get-Date) - $startTime -lt [TimeSpan]::FromSeconds($Timeout)) {
        # Check all deployments
        $deployments = kubectl get deployments -n $namespace -o json 2>$null | ConvertFrom-Json
        $allReady = $true
        
        foreach ($deploy in $deployments.items) {
            $name = $deploy.metadata.name
            $replicas = $deploy.spec.replicas
            $readyReplicas = $deploy.status.readyReplicas ?? 0
            
            if ($readyReplicas -lt $replicas) {
                Write-Status "  $name: $readyReplicas/$replicas ready"
                $allReady = $false
            } else {
                Write-Status "  $name: ✓ Ready ($readyReplicas/$replicas)" "SUCCESS"
            }
        }
        
        if ($allReady) {
            Write-Status "All deployments ready!" "SUCCESS"
            break
        }
        
        Start-Sleep -Seconds 5
    }
    
    if (-not $allReady) {
        Write-Status "Timeout waiting for deployments to be ready" "WARN"
    }
}

# Summary
Write-Status "Deployment complete!" "SUCCESS"
Write-Host "`n📊 Status Summary:" -ForegroundColor Cyan
kubectl get all -n $namespace --no-headers | Select-Object -First 20
Write-Host "`n🔗 Useful commands:" -ForegroundColor Cyan
Write-Host "  Watch deployment:       kubectl get pods -n $namespace -w"
Write-Host "  View logs:              kubectl logs -n $namespace -f deployment/api-gateway"
Write-Host "  Port forward:           kubectl port-forward -n $namespace svc/api-gateway 5000:80"
Write-Host "  Open Grafana:           kubectl port-forward -n $namespace svc/grafana 3000:3000"
