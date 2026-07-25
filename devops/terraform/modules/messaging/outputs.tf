output "kafka_bootstrap_servers" {
  value     = var.cloud_provider == "aws" ? aws_msk_cluster.kafka[0].bootstrap_brokers_tls : ""
  sensitive = true
}
output "rabbitmq_endpoint" {
  value     = var.cloud_provider == "aws" ? aws_mq_broker.rabbitmq[0].instances[0].endpoints[0] : ""
  sensitive = true
}
