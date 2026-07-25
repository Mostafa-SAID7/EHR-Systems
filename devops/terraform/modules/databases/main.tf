# =============================================================================
# Databases Module — RDS PostgreSQL, DocumentDB (MongoDB), ElastiCache Redis
# =============================================================================

locals {
  name_prefix = "${var.project_name}-${var.environment}"
}

# ── RDS PostgreSQL (one instance per logical DB, shared cluster) ───────────────
resource "aws_db_subnet_group" "postgres" {
  count      = var.cloud_provider == "aws" ? 1 : 0
  name       = "${local.name_prefix}-postgres-subnet-group"
  subnet_ids = var.private_subnet_ids
  tags       = var.tags
}

resource "aws_rds_cluster" "postgres" {
  count = var.cloud_provider == "aws" ? 1 : 0

  cluster_identifier      = "${local.name_prefix}-aurora-pg"
  engine                  = "aurora-postgresql"
  engine_version          = var.postgres_version
  database_name           = "ehr_identity"
  master_username         = "ehr_admin"
  manage_master_user_password = true   # AWS Secrets Manager rotation

  db_subnet_group_name    = aws_db_subnet_group.postgres[0].name
  deletion_protection     = var.environment == "prod"
  storage_encrypted       = true       # HIPAA: encryption at rest
  backup_retention_period = var.environment == "prod" ? 35 : 7

  tags = var.tags
}

resource "aws_rds_cluster_instance" "postgres" {
  count = var.cloud_provider == "aws" ? (var.environment == "prod" ? 2 : 1) : 0

  identifier         = "${local.name_prefix}-aurora-pg-${count.index}"
  cluster_identifier = aws_rds_cluster.postgres[0].id
  instance_class     = var.db_instance_class
  engine             = aws_rds_cluster.postgres[0].engine
  engine_version     = aws_rds_cluster.postgres[0].engine_version
  tags               = var.tags
}

# ── ElastiCache Redis ─────────────────────────────────────────────────────────
resource "aws_elasticache_subnet_group" "redis" {
  count      = var.cloud_provider == "aws" ? 1 : 0
  name       = "${local.name_prefix}-redis-subnet-group"
  subnet_ids = var.private_subnet_ids
}

resource "aws_elasticache_replication_group" "redis" {
  count = var.cloud_provider == "aws" ? 1 : 0

  replication_group_id = "${local.name_prefix}-redis"
  description          = "EHR Platform Redis cluster"
  node_type            = var.redis_node_type
  port                 = 6379
  parameter_group_name = "default.redis7"

  num_cache_clusters = var.environment == "prod" ? 2 : 1
  automatic_failover_enabled = var.environment == "prod"

  at_rest_encryption_enabled  = true   # HIPAA
  transit_encryption_enabled  = true   # HIPAA

  subnet_group_name = aws_elasticache_subnet_group.redis[0].name
  tags              = var.tags
}
