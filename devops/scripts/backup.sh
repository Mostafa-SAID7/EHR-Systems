#!/usr/bin/env bash
# =============================================================================
# backup.sh — Backup all EHR PostgreSQL databases to S3 / local
# Usage: ./devops/scripts/backup.sh [--target s3|local] [--bucket <name>]
# HIPAA: Backups are encrypted and retained for 7 years.
# =============================================================================
set -euo pipefail

TARGET="${TARGET:-local}"
BUCKET="${BUCKET:-ehr-platform-dev-db-backups}"
BACKUP_DIR="${BACKUP_DIR:-./backups}"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
ENCRYPTION_KEY="${BACKUP_ENCRYPTION_KEY:-}"

DATABASES=(
  "postgres-identity:5432:ehr_identity"
  "postgres-patient:5432:ehr_patient"
  "postgres-clinical:5432:ehr_clinical"
  "postgres-appointments:5432:ehr_appointments"
  "postgres-audit:5432:ehr_audit"
)

mkdir -p "$BACKUP_DIR"

backup_postgres() {
  local host="$1"
  local port="$2"
  local db="$3"
  local filename="${BACKUP_DIR}/${db}_${TIMESTAMP}.sql.gz"

  echo "→ Backing up ${db} from ${host}:${port}..."

  PGPASSWORD="${POSTGRES_PASSWORD:-postgres}" \
    pg_dump -h "$host" -p "$port" -U "${POSTGRES_USER:-postgres}" "$db" \
    | gzip > "$filename"

  # Encrypt if key is set (HIPAA: encryption at rest)
  if [[ -n "$ENCRYPTION_KEY" ]]; then
    openssl enc -aes-256-cbc -pbkdf2 -pass "pass:${ENCRYPTION_KEY}" \
      -in "$filename" -out "${filename}.enc"
    rm "$filename"
    filename="${filename}.enc"
  fi

  echo "  ✅ Saved to ${filename}"

  # Upload to S3
  if [[ "$TARGET" == "s3" ]]; then
    aws s3 cp "$filename" "s3://${BUCKET}/$(basename "$filename")" \
      --sse aws:kms
    echo "  ☁️  Uploaded to s3://${BUCKET}/$(basename "$filename")"
  fi
}

for entry in "${DATABASES[@]}"; do
  IFS=: read -r host port db <<< "$entry"
  backup_postgres "$host" "$port" "$db"
done

echo "✅ All backups complete (${TIMESTAMP})"

# Remove local files older than 7 days (S3 lifecycle handles long-term retention)
find "$BACKUP_DIR" -name "*.sql.gz*" -mtime +7 -delete 2>/dev/null || true
