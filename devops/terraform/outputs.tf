# =============================================================================
# Root Outputs — values useful after apply (e.g. for CI/CD or documentation)
# =============================================================================

output "cluster_endpoint" {
  description = "Kubernetes API server endpoint"
  value       = module.kubernetes.cluster_endpoint
  sensitive   = true
}

output "cluster_name" {
  description = "Kubernetes cluster name"
  value       = module.kubernetes.cluster_name
}

output "postgres_endpoints" {
  description = "Map of PostgreSQL endpoint addresses by database name"
  value       = module.databases.postgres_endpoints
  sensitive   = true
}

output "redis_endpoint" {
  description = "Redis cluster endpoint"
  value       = module.databases.redis_endpoint
  sensitive   = true
}

output "kafka_bootstrap_servers" {
  description = "Kafka bootstrap server addresses"
  value       = module.messaging.kafka_bootstrap_servers
  sensitive   = true
}

output "backup_bucket_name" {
  description = "S3/Blob Storage bucket for database backups"
  value       = module.storage.backup_bucket_name
}

output "secrets_manager_arn" {
  description = "ARN/ID of the secrets store for application secrets"
  value       = module.security.secrets_store_id
  sensitive   = true
}

output "grafana_url" {
  description = "Grafana dashboard URL"
  value       = module.monitoring.grafana_url
}
