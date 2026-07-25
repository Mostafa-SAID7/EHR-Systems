#!/usr/bin/env bash
# =============================================================================
# build.sh — Build all Docker images for the EHR Platform
# Usage: ./devops/scripts/build.sh [--push] [--tag <tag>] [--service <name>]
# Run from the repo root.
# =============================================================================
set -euo pipefail

REGISTRY="${REGISTRY:-ghcr.io/your-org}"
TAG="${TAG:-latest}"
PUSH=false
SERVICE=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    --push)    PUSH=true; shift ;;
    --tag)     TAG="$2";  shift 2 ;;
    --service) SERVICE="$2"; shift 2 ;;
    *) echo "Unknown option: $1"; exit 1 ;;
  esac
done

BACKEND_SERVICES=(
  EHRPlatform.Services.ApiGateway
  EHRPlatform.Services.Identity
  EHRPlatform.Services.Patient
  EHRPlatform.Services.Clinical
  EHRPlatform.Services.Appointment
  EHRPlatform.Services.Notification
  EHRPlatform.Services.Audit
  EHRPlatform.Services.Billing
  EHRPlatform.Services.Prescription
  EHRPlatform.Services.Analytics
)

build_backend() {
  local svc="$1"
  local image_name
  image_name="${REGISTRY}/$(echo "$svc" | tr '[:upper:]' '[:lower:]' | sed 's/ehrplatform\.services\.//')"
  
  echo "→ Building ${image_name}:${TAG}"
  docker build \
    --file devops/docker/services/Dockerfile.backend \
    --build-arg SERVICE="$svc" \
    --tag "${image_name}:${TAG}" \
    --label "org.opencontainers.image.revision=$(git rev-parse --short HEAD)" \
    --label "org.opencontainers.image.created=$(date -u +%Y-%m-%dT%H:%M:%SZ)" \
    .

  if $PUSH; then
    echo "→ Pushing ${image_name}:${TAG}"
    docker push "${image_name}:${TAG}"
  fi
}

build_frontend() {
  local image_name="${REGISTRY}/frontend"
  echo "→ Building ${image_name}:${TAG}"
  docker build \
    --file devops/docker/services/Dockerfile.frontend \
    --tag "${image_name}:${TAG}" \
    .
  
  if $PUSH; then
    docker push "${image_name}:${TAG}"
  fi
}

if [[ -n "$SERVICE" ]]; then
  if [[ "$SERVICE" == "frontend" ]]; then
    build_frontend
  else
    build_backend "$SERVICE"
  fi
else
  for svc in "${BACKEND_SERVICES[@]}"; do
    build_backend "$svc"
  done
  build_frontend
fi

echo "✅ Build complete (tag: ${TAG})"
