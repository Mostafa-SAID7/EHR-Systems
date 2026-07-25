#!/usr/bin/env bash
# =============================================================================
# deploy.sh — Deploy EHR Platform to a Kubernetes environment
# Usage: ./devops/scripts/deploy.sh <dev|staging|prod> [--tag <image-tag>]
# =============================================================================
set -euo pipefail

ENVIRONMENT="${1:-dev}"
TAG="${TAG:-latest}"

shift 1 || true
while [[ $# -gt 0 ]]; do
  case "$1" in
    --tag) TAG="$2"; shift 2 ;;
    *) echo "Unknown option: $1"; exit 1 ;;
  esac
done

if [[ ! "$ENVIRONMENT" =~ ^(dev|staging|prod)$ ]]; then
  echo "❌ Environment must be dev, staging, or prod"
  exit 1
fi

echo "🚀 Deploying EHR Platform → ${ENVIRONMENT} (image tag: ${TAG})"

OVERLAY="devops/kubernetes/overlays/${ENVIRONMENT}"

# Set image tags in the overlay
(
  cd "$OVERLAY"
  for img in api-gateway identity-service patient-service clinical-service \
              appointment-service notification-service audit-service \
              billing-service prescription-service analytics-service frontend; do
    kustomize edit set image "ehr/${img}=ehr/${img}:${TAG}"
  done
)

# Apply
kubectl apply -k "$OVERLAY"

# Wait for critical deployments
CRITICAL_DEPLOYMENTS=(api-gateway identity-service patient-service)
NS="ehr-platform"
[[ "$ENVIRONMENT" == "staging" ]] && NS="ehr-platform-staging"

echo "⏳ Waiting for rollouts..."
for dep in "${CRITICAL_DEPLOYMENTS[@]}"; do
  kubectl -n "$NS" rollout status "deployment/${dep}" --timeout=10m
done

echo "✅ Deployment to ${ENVIRONMENT} complete"

# Quick health check
GATEWAY_HOST=$(kubectl -n "$NS" get svc api-gateway -o jsonpath='{.status.loadBalancer.ingress[0].hostname}' 2>/dev/null || echo "")
if [[ -n "$GATEWAY_HOST" ]]; then
  echo "🩺 Health check → http://${GATEWAY_HOST}/health"
  curl -sf "http://${GATEWAY_HOST}/health" && echo "✅ Gateway healthy" || echo "⚠️  Gateway health check failed"
fi
