#!/usr/bin/env bash
# =============================================================================
# migrate.sh — Run EF Core database migrations for all EHR services
# Usage: ./devops/scripts/migrate.sh [--service <name>] [--env <dev|staging|prod>]
# For local development, run against the Docker Compose stack.
# =============================================================================
set -euo pipefail

SERVICE=""
ENV="local"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --service) SERVICE="$2"; shift 2 ;;
    --env)     ENV="$2"; shift 2 ;;
    *) echo "Unknown option: $1"; exit 1 ;;
  esac
done

SERVICES=(
  EHRPlatform.Services.Identity
  EHRPlatform.Services.Patient
  EHRPlatform.Services.Clinical
  EHRPlatform.Services.Appointment
  EHRPlatform.Services.Audit
  EHRPlatform.Services.Billing
  EHRPlatform.Services.Prescription
  EHRPlatform.Services.Analytics
)

run_migration() {
  local svc="$1"
  echo "→ Migrating ${svc}..."

  if [[ "$ENV" == "local" ]]; then
    # Run against local Docker Compose databases
    (cd backend && dotnet ef database update \
      --project "src/${svc}/${svc}.csproj" \
      --startup-project "src/${svc}/${svc}.csproj" \
      --no-build 2>&1) && echo "  ✅ ${svc} done" || echo "  ❌ ${svc} failed"
  else
    # In Kubernetes — run as a Job
    kubectl run "migrate-$(echo "${svc}" | tr '[:upper:]' '[:lower:]' | tr '.' '-')" \
      --image="ehr/$(echo "${svc}" | tr '[:upper:]' '[:lower:]' | sed 's/ehrplatform\.services\.//')" \
      --restart=Never \
      --namespace=ehr-platform \
      --command -- dotnet "${svc}.dll" migrate
  fi
}

if [[ -n "$SERVICE" ]]; then
  run_migration "$SERVICE"
else
  for svc in "${SERVICES[@]}"; do
    run_migration "$svc"
  done
fi

echo "✅ Migrations complete"
