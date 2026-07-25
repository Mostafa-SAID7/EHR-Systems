# =============================================================================
# Kubernetes Module — EKS (AWS) / AKS (Azure) / GKE (GCP)
# =============================================================================

locals {
  name_prefix    = "${var.project_name}-${var.environment}"
  cluster_name   = "${local.name_prefix}-cluster"
}

# ── AWS EKS ───────────────────────────────────────────────────────────────────
module "eks" {
  count   = var.cloud_provider == "aws" ? 1 : 0
  source  = "terraform-aws-modules/eks/aws"
  version = "~> 20.0"

  cluster_name    = local.cluster_name
  cluster_version = "1.29"

  vpc_id     = var.vpc_id
  subnet_ids = var.private_subnet_ids

  # API server access: private by default (HIPAA)
  cluster_endpoint_private_access = true
  cluster_endpoint_public_access  = var.environment != "prod"

  eks_managed_node_groups = {
    default = {
      min_size       = var.node_min_count
      max_size       = var.node_max_count
      desired_size   = var.node_min_count
      instance_types = [var.node_instance_type]

      labels = {
        Environment = var.environment
        Project     = var.project_name
      }
    }
  }

  # Enable IRSA for pod-level IAM
  enable_irsa = true

  tags = var.tags
}

# ── Azure AKS ─────────────────────────────────────────────────────────────────
resource "azurerm_kubernetes_cluster" "main" {
  count = var.cloud_provider == "azure" ? 1 : 0

  name                = local.cluster_name
  location            = var.region
  resource_group_name = "${local.name_prefix}-rg"
  dns_prefix          = local.cluster_name

  default_node_pool {
    name       = "default"
    node_count = var.node_min_count
    vm_size    = var.node_instance_type
  }

  identity {
    type = "SystemAssigned"
  }

  tags = var.tags
}
