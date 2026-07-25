output "cluster_name" {
  value = local.cluster_name
}
output "cluster_endpoint" {
  value     = var.cloud_provider == "aws" ? module.eks[0].cluster_endpoint : (var.cloud_provider == "azure" ? azurerm_kubernetes_cluster.main[0].kube_config[0].host : "")
  sensitive = true
}
output "cluster_arn" {
  value     = var.cloud_provider == "aws" ? module.eks[0].cluster_arn : ""
  sensitive = false
}
output "kubeconfig_command" {
  description = "Command to configure kubectl"
  value = var.cloud_provider == "aws" ? "aws eks update-kubeconfig --name ${local.cluster_name} --region ${var.region}" : (var.cloud_provider == "azure" ? "az aks get-credentials --name ${local.cluster_name} --resource-group ${var.project_name}-${var.environment}-rg" : "gcloud container clusters get-credentials ${local.cluster_name} --region ${var.region}")
}
