# =============================================================================
# EHR Platform — Terraform Root Module
# Provider-agnostic wrapper: set TF_VAR_cloud_provider to "aws", "azure", or "gcp"
# =============================================================================

terraform {
  required_version = ">= 1.6"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
    google = {
      source  = "hashicorp/google"
      version = "~> 5.0"
    }
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~> 2.23"
    }
    helm = {
      source  = "hashicorp/helm"
      version = "~> 2.11"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.5"
    }
  }

  backend "s3" {
    # Configured in environments/<env>/backend.tf or via -backend-config flags
    # terraform init -backend-config=environments/prod/backend.hcl
  }
}

# ---------------------------------------------------------------------------
# Networking
# ---------------------------------------------------------------------------
module "networking" {
  source = "./modules/networking"

  project_name       = var.project_name
  environment        = var.environment
  cloud_provider     = var.cloud_provider
  region             = var.region
  vpc_cidr           = var.vpc_cidr
  availability_zones = var.availability_zones
  tags               = local.common_tags
}

# ---------------------------------------------------------------------------
# Kubernetes Cluster (EKS / AKS / GKE)
# ---------------------------------------------------------------------------
module "kubernetes" {
  source = "./modules/kubernetes"

  project_name        = var.project_name
  environment         = var.environment
  cloud_provider      = var.cloud_provider
  region              = var.region
  vpc_id              = module.networking.vpc_id
  private_subnet_ids  = module.networking.private_subnet_ids
  node_instance_type  = var.k8s_node_instance_type
  node_min_count      = var.k8s_node_min_count
  node_max_count      = var.k8s_node_max_count
  tags                = local.common_tags
}

# ---------------------------------------------------------------------------
# Databases
# ---------------------------------------------------------------------------
module "databases" {
  source = "./modules/databases"

  project_name        = var.project_name
  environment         = var.environment
  cloud_provider      = var.cloud_provider
  region              = var.region
  vpc_id              = module.networking.vpc_id
  private_subnet_ids  = module.networking.private_subnet_ids
  db_instance_class   = var.db_instance_class
  postgres_version    = "16"
  mongo_version       = "7.0"
  redis_node_type     = var.redis_node_type
  tags                = local.common_tags
}

# ---------------------------------------------------------------------------
# Messaging (Kafka + RabbitMQ)
# ---------------------------------------------------------------------------
module "messaging" {
  source = "./modules/messaging"

  project_name       = var.project_name
  environment        = var.environment
  cloud_provider     = var.cloud_provider
  region             = var.region
  vpc_id             = module.networking.vpc_id
  private_subnet_ids = module.networking.private_subnet_ids
  kafka_instance_type = var.kafka_instance_type
  tags               = local.common_tags
}

# ---------------------------------------------------------------------------
# Object Storage (backups, HIPAA audit exports)
# ---------------------------------------------------------------------------
module "storage" {
  source = "./modules/storage"

  project_name   = var.project_name
  environment    = var.environment
  cloud_provider = var.cloud_provider
  region         = var.region
  tags           = local.common_tags
}

# ---------------------------------------------------------------------------
# Security (IAM, WAF, Secret Manager, KMS)
# ---------------------------------------------------------------------------
module "security" {
  source = "./modules/security"

  project_name    = var.project_name
  environment     = var.environment
  cloud_provider  = var.cloud_provider
  region          = var.region
  cluster_arn     = module.kubernetes.cluster_arn
  storage_bucket  = module.storage.backup_bucket_arn
  tags            = local.common_tags
}

# ---------------------------------------------------------------------------
# Monitoring (Prometheus, Grafana via Helm)
# ---------------------------------------------------------------------------
module "monitoring" {
  source = "./modules/monitoring"

  project_name        = var.project_name
  environment         = var.environment
  cluster_endpoint    = module.kubernetes.cluster_endpoint
  grafana_admin_pass  = var.grafana_admin_password
  tags                = local.common_tags

  depends_on = [module.kubernetes]
}

# ---------------------------------------------------------------------------
# Locals
# ---------------------------------------------------------------------------
locals {
  common_tags = {
    Project     = var.project_name
    Environment = var.environment
    ManagedBy   = "terraform"
    Compliance  = "hipaa"
    Owner       = var.owner_team
  }
}
