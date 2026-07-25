# =============================================================================
# Storage Module — S3 buckets for backups and HIPAA audit exports
# =============================================================================

locals {
  name_prefix = "${var.project_name}-${var.environment}"
}

resource "aws_s3_bucket" "backups" {
  count  = var.cloud_provider == "aws" ? 1 : 0
  bucket = "${local.name_prefix}-db-backups"
  tags   = var.tags
}

resource "aws_s3_bucket_versioning" "backups" {
  count  = var.cloud_provider == "aws" ? 1 : 0
  bucket = aws_s3_bucket.backups[0].id
  versioning_configuration { status = "Enabled" }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "backups" {
  count  = var.cloud_provider == "aws" ? 1 : 0
  bucket = aws_s3_bucket.backups[0].id
  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "aws:kms"   # HIPAA: AES-256 via KMS
    }
  }
}

resource "aws_s3_bucket_lifecycle_configuration" "backups" {
  count  = var.cloud_provider == "aws" ? 1 : 0
  bucket = aws_s3_bucket.backups[0].id

  rule {
    id     = "transition-to-glacier"
    status = "Enabled"
    transition {
      days          = 90
      storage_class = "GLACIER"
    }
    expiration { days = 2557 }  # 7 years (HIPAA requirement)
  }
}

# Block all public access
resource "aws_s3_bucket_public_access_block" "backups" {
  count  = var.cloud_provider == "aws" ? 1 : 0
  bucket = aws_s3_bucket.backups[0].id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

# HIPAA audit export bucket (separate, immutable)
resource "aws_s3_bucket" "audit_exports" {
  count  = var.cloud_provider == "aws" ? 1 : 0
  bucket = "${local.name_prefix}-audit-exports"
  tags   = merge(var.tags, { Purpose = "hipaa-audit" })
}

resource "aws_s3_bucket_object_lock_configuration" "audit_exports" {
  count  = var.cloud_provider == "aws" ? 1 : 0
  bucket = aws_s3_bucket.audit_exports[0].id

  rule {
    default_retention {
      mode = "COMPLIANCE"
      years = 7
    }
  }
}
