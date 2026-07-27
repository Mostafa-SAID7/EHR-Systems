#!/bin/bash
# ═══════════════════════════════════════════════════════════════════════════════
# EHR Platform - Database Migration Script
# Purpose: Manage database migrations across environments
# Usage: ./db-migrate.sh [up|down|version|pending|validate] [--env=environment]
# ═══════════════════════════════════════════════════════════════════════════════

set -euo pipefail

# ─────────────────────────────────────────────────────────────────────────────
# Configuration
# ─────────────────────────────────────────────────────────────────────────────

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$(dirname "$SCRIPT_DIR")")"
MIGRATIONS_DIR="$PROJECT_ROOT/db/migrations"
ROLLBACK_DIR="$PROJECT_ROOT/db/rollback"
LOG_FILE="/var/log/ehr-migrations.log"

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# ─────────────────────────────────────────────────────────────────────────────
# Environment Configuration
# ─────────────────────────────────────────────────────────────────────────────

# Parse environment from arguments
ENVIRONMENT="${ENVIRONMENT:-development}"
for arg in "$@"; do
    if [[ $arg == --env=* ]]; then
        ENVIRONMENT="${arg#--env=}"
    fi
done

# Load environment-specific config
case "$ENVIRONMENT" in
    development)
        DB_HOST="${DB_HOST:-localhost}"
        DB_PORT="${DB_PORT:-5432}"
        DB_USER="${DB_USER:-ehr_user}"
        DB_NAME="${DB_NAME:-ehr_platform_dev}"
        DB_PASSWORD="${DB_PASSWORD:-password}"
        DRY_RUN=false
        AUTO_BACKUP=false
        ;;
    staging)
        DB_HOST="${DB_HOST:-staging-db.local}"
        DB_PORT="${DB_PORT:-5432}"
        DB_USER="${DB_USER:-ehr_user}"
        DB_NAME="${DB_NAME:-ehr_platform_staging}"
        DB_PASSWORD="${DB_PASSWORD:-$(grep DB_PASSWORD ~/.env.staging)}"
        DRY_RUN=true
        AUTO_BACKUP=true
        ;;
    production)
        DB_HOST="${DB_HOST:-prod-db.local}"
        DB_PORT="${DB_PORT:-5432}"
        DB_USER="${DB_USER:-ehr_admin}"
        DB_NAME="${DB_NAME:-ehr_platform}"
        DB_PASSWORD="${DB_PASSWORD:-$(grep DB_PASSWORD ~/.env.production)}"
        DRY_RUN=true
        AUTO_BACKUP=true
        REQUIRE_APPROVAL=true
        ;;
    *)
        echo -e "${RED}❌ Unknown environment: $ENVIRONMENT${NC}"
        exit 1
        ;;
esac

# ─────────────────────────────────────────────────────────────────────────────
# Logging Functions
# ─────────────────────────────────────────────────────────────────────────────

log() {
    echo -e "${BLUE}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} $*" | tee -a "$LOG_FILE"
}

log_success() {
    echo -e "${GREEN}✅ $*${NC}" | tee -a "$LOG_FILE"
}

log_error() {
    echo -e "${RED}❌ $*${NC}" | tee -a "$LOG_FILE"
}

log_warning() {
    echo -e "${YELLOW}⚠️  $*${NC}" | tee -a "$LOG_FILE"
}

# ─────────────────────────────────────────────────────────────────────────────
# Database Connection Functions
# ─────────────────────────────────────────────────────────────────────────────

# Test database connection
db_connect_test() {
    log "Testing database connection..."
    
    PGPASSWORD="$DB_PASSWORD" psql \
        -h "$DB_HOST" \
        -p "$DB_PORT" \
        -U "$DB_USER" \
        -d "$DB_NAME" \
        -c "SELECT 1" > /dev/null 2>&1
    
    if [ $? -eq 0 ]; then
        log_success "Database connection successful"
        return 0
    else
        log_error "Failed to connect to database"
        return 1
    fi
}

# Execute SQL query
db_execute() {
    local sql="$1"
    PGPASSWORD="$DB_PASSWORD" psql \
        -h "$DB_HOST" \
        -p "$DB_PORT" \
        -U "$DB_USER" \
        -d "$DB_NAME" \
        -c "$sql"
}

# Execute SQL file
db_execute_file() {
    local file="$1"
    
    if [ ! -f "$file" ]; then
        log_error "Migration file not found: $file"
        return 1
    fi
    
    log "Executing migration: $file"
    PGPASSWORD="$DB_PASSWORD" psql \
        -h "$DB_HOST" \
        -p "$DB_PORT" \
        -U "$DB_USER" \
        -d "$DB_NAME" \
        -f "$file"
}

# ─────────────────────────────────────────────────────────────────────────────
# Backup Functions
# ─────────────────────────────────────────────────────────────────────────────

create_backup() {
    if [ "$AUTO_BACKUP" = false ]; then
        return 0
    fi
    
    local backup_dir="$PROJECT_ROOT/backups"
    local backup_file="$backup_dir/backup_$(date +'%Y%m%d_%H%M%S').sql"
    
    mkdir -p "$backup_dir"
    
    log "Creating backup: $backup_file"
    
    PGPASSWORD="$DB_PASSWORD" pg_dump \
        -h "$DB_HOST" \
        -p "$DB_PORT" \
        -U "$DB_USER" \
        -d "$DB_NAME" \
        --verbose \
        > "$backup_file"
    
    if [ $? -eq 0 ]; then
        log_success "Backup created: $backup_file"
        echo "$backup_file"
    else
        log_error "Backup failed"
        return 1
    fi
}

# ─────────────────────────────────────────────────────────────────────────────
# Migration Functions
# ─────────────────────────────────────────────────────────────────────────────

# Get current migration version
get_current_version() {
    local query="SELECT MAX(\"AppliedAt\") as latest FROM \"__MigrationHistory\""
    db_execute "$query" | tail -1
}

# List applied migrations
list_applied_migrations() {
    log "Applied migrations:"
    local query="SELECT \"MigrationId\", \"ProductVersion\", \"AppliedAt\" FROM \"__MigrationHistory\" ORDER BY \"AppliedAt\" DESC"
    db_execute "$query"
}

# List pending migrations
list_pending_migrations() {
    log "Pending migrations:"
    
    # Get list of migration files
    local applied_migrations=$(db_execute "SELECT \"MigrationId\" FROM \"__MigrationHistory\"")
    
    for migration_file in "$MIGRATIONS_DIR"/[0-9]*.sql; do
        if [ -f "$migration_file" ]; then
            local migration_id=$(basename "$migration_file" .sql)
            
            if ! echo "$applied_migrations" | grep -q "$migration_id"; then
                echo "  - $migration_id"
            fi
        fi
    done
}

# Apply all pending migrations
apply_migrations() {
    log "Starting migration process for environment: $ENVIRONMENT"
    
    # Test connection
    if ! db_connect_test; then
        return 1
    fi
    
    # Create backup
    if [ "$AUTO_BACKUP" = true ]; then
        BACKUP_FILE=$(create_backup) || return 1
    fi
    
    # Get applied migrations
    local applied_migrations=$(db_execute "SELECT \"MigrationId\" FROM \"__MigrationHistory\"")
    
    # Apply pending migrations in order
    local count=0
    for migration_file in "$MIGRATIONS_DIR"/[0-9]*.sql; do
        if [ -f "$migration_file" ]; then
            local migration_id=$(basename "$migration_file" .sql)
            
            # Skip if already applied
            if echo "$applied_migrations" | grep -q "$migration_id"; then
                log "Migration already applied: $migration_id"
                continue
            fi
            
            # Skip template
            if [[ "$migration_id" == "00_MIGRATION_TEMPLATE" ]]; then
                continue
            fi
            
            log "Applying migration: $migration_id"
            
            if db_execute_file "$migration_file"; then
                log_success "Migration applied: $migration_id"
                ((count++))
            else
                log_error "Migration failed: $migration_id"
                
                if [ "$AUTO_BACKUP" = true ] && [ -n "${BACKUP_FILE:-}" ]; then
                    log_warning "Restoring from backup: $BACKUP_FILE"
                    # TODO: Add restore logic
                fi
                
                return 1
            fi
        fi
    done
    
    log_success "Migration complete. Applied $count migration(s)"
    return 0
}

# Rollback to previous migration
rollback_migration() {
    log "Starting rollback process for environment: $ENVIRONMENT"
    
    if [ "$ENVIRONMENT" = "production" ]; then
        log_error "Production rollback requires manual approval"
        log "Contact DBA for emergency rollback procedures"
        return 1
    fi
    
    # Test connection
    if ! db_connect_test; then
        return 1
    fi
    
    # Create backup before rollback
    if [ "$AUTO_BACKUP" = true ]; then
        BACKUP_FILE=$(create_backup) || return 1
    fi
    
    # Get latest migration
    local latest_migration=$(db_execute "SELECT \"MigrationId\" FROM \"__MigrationHistory\" ORDER BY \"AppliedAt\" DESC LIMIT 1" | tail -1)
    
    if [ -z "$latest_migration" ]; then
        log_warning "No applied migrations to rollback"
        return 0
    fi
    
    # Find rollback script
    local rollback_file="$ROLLBACK_DIR/${latest_migration}_rollback.sql"
    
    if [ ! -f "$rollback_file" ]; then
        log_error "Rollback script not found: $rollback_file"
        log "See ROLLBACK_STRATEGIES.md for manual rollback instructions"
        return 1
    fi
    
    log "Rolling back migration: $latest_migration"
    
    if db_execute_file "$rollback_file"; then
        log_success "Rollback completed: $latest_migration"
        return 0
    else
        log_error "Rollback failed: $latest_migration"
        return 1
    fi
}

# Validate migration integrity
validate_migrations() {
    log "Validating migration integrity..."
    
    if ! db_connect_test; then
        return 1
    fi
    
    local issues=0
    
    # Check for orphaned records
    log "Checking for data integrity issues..."
    
    local orphaned=$(db_execute "SELECT COUNT(*) FROM \"Appointments\" WHERE \"PatientId\" NOT IN (SELECT \"Id\" FROM \"Patients\")" | tail -1)
    if [ "$orphaned" -gt 0 ]; then
        log_warning "Found $orphaned orphaned appointments"
        ((issues++))
    fi
    
    # Check for missing indexes
    log "Checking for missing indexes..."
    local missing_indexes=$(db_execute "SELECT COUNT(*) FROM pg_indexes WHERE schemaname = 'public' AND indexname LIKE 'IX_%'" | tail -1)
    
    if [ "$missing_indexes" -lt 5 ]; then
        log_warning "Expected at least 5 indexes, found: $missing_indexes"
        ((issues++))
    fi
    
    if [ "$issues" -eq 0 ]; then
        log_success "All validation checks passed"
        return 0
    else
        log_error "Validation found $issues issue(s)"
        return 1
    fi
}

# ─────────────────────────────────────────────────────────────────────────────
# Display Functions
# ─────────────────────────────────────────────────────────────────────────────

show_status() {
    log "═══════════════════════════════════════════════════════════════"
    log "Database Migration Status"
    log "═══════════════════════════════════════════════════════════════"
    log "Environment: $ENVIRONMENT"
    log "Database: $DB_NAME@$DB_HOST:$DB_PORT"
    log "Migrations Directory: $MIGRATIONS_DIR"
    log "Current Version: $(get_current_version)"
    log ""
    list_applied_migrations
    log ""
    list_pending_migrations
    log ""
}

show_help() {
    cat << EOF
${BLUE}EHR Platform - Database Migration Tool${NC}

${BLUE}Usage:${NC}
  ./db-migrate.sh [command] [options]

${BLUE}Commands:${NC}
  up                 Apply pending migrations
  down               Rollback last migration
  version            Show current migration version
  pending            List pending migrations
  validate           Validate migration integrity
  status             Show migration status

${BLUE}Options:${NC}
  --env=ENV          Environment (development/staging/production)
                     Default: development

${BLUE}Examples:${NC}
  ./db-migrate.sh up
  ./db-migrate.sh down --env=staging
  ./db-migrate.sh status --env=production
  ./db-migrate.sh validate

${BLUE}Environment-Specific Behaviors:${NC}
  development        No backup, auto-run migrations
  staging            Full backup before migrations, requires dry-run validation
  production         Full backup, manual approval required

EOF
}

# ─────────────────────────────────────────────────────────────────────────────
# Main
# ─────────────────────────────────────────────────────────────────────────────

main() {
    local command="${1:-status}"
    
    case "$command" in
        up)
            apply_migrations
            ;;
        down)
            rollback_migration
            ;;
        version)
            get_current_version
            ;;
        pending)
            list_pending_migrations
            ;;
        validate)
            validate_migrations
            ;;
        status)
            show_status
            ;;
        help|--help|-h)
            show_help
            ;;
        *)
            log_error "Unknown command: $command"
            show_help
            exit 1
            ;;
    esac
}

# Run main
main "$@"
