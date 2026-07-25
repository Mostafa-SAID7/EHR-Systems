# =============================================================================
# Security Module — IAM, KMS, WAF, Secrets Manager (HIPAA-aligned)
# =============================================================================

locals {
  name_prefix = "${var.project_name}-${var.environment}"
}

# ── KMS key for encryption at rest ────────────────────────────────────────────
resource "aws_kms_key" "ehr" {
  count               = var.cloud_provider == "aws" ? 1 : 0
  description         = "EHR Platform master encryption key (${var.environment})"
  enable_key_rotation = true   # HIPAA: annual key rotation
  tags                = var.tags
}

resource "aws_kms_alias" "ehr" {
  count         = var.cloud_provider == "aws" ? 1 : 0
  name          = "alias/${local.name_prefix}-master"
  target_key_id = aws_kms_key.ehr[0].key_id
}

# ── AWS Secrets Manager (application secrets) ─────────────────────────────────
resource "aws_secretsmanager_secret" "app_secrets" {
  count       = var.cloud_provider == "aws" ? 1 : 0
  name        = "${local.name_prefix}/app-secrets"
  kms_key_id  = aws_kms_key.ehr[0].arn
  description = "EHR Platform application secrets (JWT, encryption keys, DB passwords)"
  tags        = var.tags
}

resource "aws_secretsmanager_secret_rotation" "app_secrets" {
  count              = var.cloud_provider == "aws" ? 1 : 0
  secret_id          = aws_secretsmanager_secret.app_secrets[0].id
  rotation_lambda_arn = aws_lambda_function.rotation[0].arn

  rotation_rules {
    automatically_after_days = 90   # HIPAA: rotate every 90 days
  }
}

# Placeholder rotation Lambda (replace with real rotation logic)
resource "aws_lambda_function" "rotation" {
  count         = var.cloud_provider == "aws" ? 1 : 0
  function_name = "${local.name_prefix}-secret-rotation"
  role          = aws_iam_role.rotation_lambda[0].arn
  runtime       = "python3.11"
  handler       = "lambda_function.lambda_handler"
  filename      = "${path.module}/rotation_placeholder.zip"
  tags          = var.tags
}

resource "aws_iam_role" "rotation_lambda" {
  count = var.cloud_provider == "aws" ? 1 : 0
  name  = "${local.name_prefix}-secret-rotation-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action    = "sts:AssumeRole"
      Effect    = "Allow"
      Principal = { Service = "lambda.amazonaws.com" }
    }]
  })

  tags = var.tags
}

# ── WAF (Web Application Firewall) for the ALB/API Gateway ───────────────────
resource "aws_wafv2_web_acl" "ehr" {
  count = var.cloud_provider == "aws" ? 1 : 0
  name  = "${local.name_prefix}-waf"
  scope = "REGIONAL"

  default_action { allow {} }

  rule {
    name     = "AWSManagedRulesCommonRuleSet"
    priority = 1
    override_action { none {} }
    statement {
      managed_rule_group_statement {
        name        = "AWSManagedRulesCommonRuleSet"
        vendor_name = "AWS"
      }
    }
    visibility_config {
      cloudwatch_metrics_enabled = true
      metric_name                = "CommonRuleSet"
      sampled_requests_enabled   = true
    }
  }

  rule {
    name     = "RateLimitRule"
    priority = 2
    action { block {} }
    statement {
      rate_based_statement {
        limit              = 2000
        aggregate_key_type = "IP"
      }
    }
    visibility_config {
      cloudwatch_metrics_enabled = true
      metric_name                = "RateLimit"
      sampled_requests_enabled   = true
    }
  }

  visibility_config {
    cloudwatch_metrics_enabled = true
    metric_name                = "${local.name_prefix}-waf"
    sampled_requests_enabled   = true
  }

  tags = var.tags
}
