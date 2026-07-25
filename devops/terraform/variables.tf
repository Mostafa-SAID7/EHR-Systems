# =============================================================================
# Root Variables
# =============================================================================

variable "project_name" {
  description = "Short name used in resource naming (e.g. ehr-platform)"
  type        = string
  default     = "ehr-platform"
}

variable "environment" {
  description = "Deployment environment: dev | staging | prod"
  type        = string
  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "environment must be dev, staging, or prod."
  }
}

variable "cloud_provider" {
  description = "Target cloud: aws | azure | gcp"
  type        = string
  default     = "aws"
  validation {
    condition     = contains(["aws", "azure", "gcp"], var.cloud_provider)
    error_message = "cloud_provider must be aws, azure, or gcp."
  }
}

variable "region" {
  description = "Cloud region (e.g. us-east-1, eastus, us-central1)"
  type        = string
  default     = "us-east-1"
}

variable "owner_team" {
  description = "Team that owns this environment (for cost allocation tags)"
  type        = string
  default     = "platform-engineering"
}

# ── Networking ────────────────────────────────────────────────────────────────
variable "vpc_cidr" {
  description = "CIDR block for the VPC"
  type        = string
  default     = "10.0.0.0/16"
}

variable "availability_zones" {
  description = "List of AZs to distribute subnets across"
  type        = list(string)
  default     = ["us-east-1a", "us-east-1b", "us-east-1c"]
}

# ── Kubernetes ────────────────────────────────────────────────────────────────
variable "k8s_node_instance_type" {
  description = "EC2/VM instance type for K8s worker nodes"
  type        = string
  default     = "t3.large"
}

variable "k8s_node_min_count" {
  description = "Minimum number of worker nodes"
  type        = number
  default     = 2
}

variable "k8s_node_max_count" {
  description = "Maximum number of worker nodes (autoscaling)"
  type        = number
  default     = 10
}

# ── Databases ─────────────────────────────────────────────────────────────────
variable "db_instance_class" {
  description = "RDS/Cloud SQL instance class"
  type        = string
  default     = "db.t3.medium"
}

variable "redis_node_type" {
  description = "ElastiCache/MemoryStore node type"
  type        = string
  default     = "cache.t3.small"
}

# ── Messaging ─────────────────────────────────────────────────────────────────
variable "kafka_instance_type" {
  description = "MSK/Confluent broker instance type"
  type        = string
  default     = "kafka.t3.small"
}

# ── Monitoring ────────────────────────────────────────────────────────────────
variable "grafana_admin_password" {
  description = "Grafana admin password (inject via TF_VAR_grafana_admin_password)"
  type        = string
  sensitive   = true
}
