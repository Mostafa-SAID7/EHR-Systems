output "backup_bucket_name" {
  value = var.cloud_provider == "aws" ? aws_s3_bucket.backups[0].bucket : ""
}
output "backup_bucket_arn" {
  value = var.cloud_provider == "aws" ? aws_s3_bucket.backups[0].arn : ""
}
output "audit_bucket_name" {
  value = var.cloud_provider == "aws" ? aws_s3_bucket.audit_exports[0].bucket : ""
}
