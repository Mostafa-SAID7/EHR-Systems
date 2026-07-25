output "postgres_endpoints" {
  value = {
    writer = var.cloud_provider == "aws" ? aws_rds_cluster.postgres[0].endpoint : ""
    reader = var.cloud_provider == "aws" ? aws_rds_cluster.postgres[0].reader_endpoint : ""
  }
  sensitive = true
}
output "redis_endpoint" {
  value     = var.cloud_provider == "aws" ? aws_elasticache_replication_group.redis[0].primary_endpoint_address : ""
  sensitive = true
}
