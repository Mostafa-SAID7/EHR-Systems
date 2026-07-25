output "grafana_url" {
  description = "Grafana dashboard URL (access via kubectl port-forward or ingress)"
  value       = "http://grafana.${var.environment}.ehr-platform.example.com"
}
