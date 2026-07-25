#!/usr/bin/env bash
# =============================================================================
# health-check.sh — Check health of all EHR services
# Usage: ./devops/scripts/health-check.sh [--base-url <url>] [--env <local|k8s>]
# =============================================================================
set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost}"
ENV="${1:-local}"
PASS=0
FAIL=0

check() {
  local name="$1"
  local url="$2"
  
  if curl -sf --max-time 5 "$url" > /dev/null 2>&1; then
    echo "  ✅  ${name}"
    ((PASS++))
  else
    echo "  ❌  ${name} → ${url}"
    ((FAIL++))
  fi
}

echo "🩺 EHR Platform Health Check (${ENV})"
echo "─────────────────────────────────────────"

if [[ "$ENV" == "local" ]]; then
  check "Identity Service"     "http://localhost:5001/health"
  check "Patient Service"      "http://localhost:5002/health"
  check "Clinical Service"     "http://localhost:5003/health"
  check "Appointment Service"  "http://localhost:5004/health"
  check "Notification Service" "http://localhost:5005/health"
  check "Audit Service"        "http://localhost:5006/health"
  check "Billing Service"      "http://localhost:5007/health"
  check "Prescription Service" "http://localhost:5008/health"
  check "Analytics Service"    "http://localhost:5009/health"
  check "API Gateway"          "http://localhost:5000/health"
  check "Frontend"             "http://localhost:4200"
  check "Kafka UI"             "http://localhost:8090"
  check "RabbitMQ UI"          "http://localhost:15672"
  check "Kibana"               "http://localhost:5601"
else
  # Kubernetes — check via kubectl port-forward or ingress
  NS="ehr-platform"
  for dep in api-gateway identity-service patient-service clinical-service \
              appointment-service audit-service billing-service \
              prescription-service analytics-service; do
    READY=$(kubectl -n "$NS" get deployment "$dep" \
      -o jsonpath='{.status.readyReplicas}' 2>/dev/null || echo "0")
    DESIRED=$(kubectl -n "$NS" get deployment "$dep" \
      -o jsonpath='{.spec.replicas}' 2>/dev/null || echo "?")
    
    if [[ "$READY" == "$DESIRED" && "$READY" != "0" ]]; then
      echo "  ✅  ${dep} (${READY}/${DESIRED} pods)"
      ((PASS++))
    else
      echo "  ❌  ${dep} (${READY}/${DESIRED} pods ready)"
      ((FAIL++))
    fi
  done
fi

echo "─────────────────────────────────────────"
echo "Results: ✅ ${PASS} up  |  ❌ ${FAIL} down"

[[ $FAIL -eq 0 ]] && exit 0 || exit 1
