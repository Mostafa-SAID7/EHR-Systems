output "secrets_store_id" {
  value     = var.cloud_provider == "aws" ? aws_secretsmanager_secret.app_secrets[0].arn : ""
  sensitive = true
}
output "kms_key_arn" {
  value     = var.cloud_provider == "aws" ? aws_kms_key.ehr[0].arn : ""
  sensitive = true
}
output "waf_acl_arn" {
  value = var.cloud_provider == "aws" ? aws_wafv2_web_acl.ehr[0].arn : ""
}
