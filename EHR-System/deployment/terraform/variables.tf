variable "aws_region" {
  type        = string
  description = "AWS region"
  default     = "us-east-1"
}

variable "environment" {
  type        = string
  description = "Environment name (dev, staging, prod)"
  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "Environment must be dev, staging, or prod."
  }
}

variable "vpc_cidr" {
  type        = string
  description = "CIDR block for VPC"
  default     = "10.0.0.0/16"
}

variable "availability_zones" {
  type        = list(string)
  description = "Availability zones"
  default     = ["us-east-1a", "us-east-1b", "us-east-1c"]
}

variable "public_subnet_cidrs" {
  type        = list(string)
  description = "Public subnet CIDR blocks"
  default     = ["10.0.1.0/24", "10.0.2.0/24", "10.0.3.0/24"]
}

variable "private_subnet_cidrs" {
  type        = list(string)
  description = "Private subnet CIDR blocks"
  default     = ["10.0.11.0/24", "10.0.12.0/24", "10.0.13.0/24"]
}

variable "kubernetes_version" {
  type        = string
  description = "Kubernetes version"
  default     = "1.28"
}

variable "node_group_desired_size" {
  type        = number
  description = "Desired number of nodes"
  default     = 3
}

variable "node_group_min_size" {
  type        = number
  description = "Minimum number of nodes"
  default     = 2
}

variable "node_group_max_size" {
  type        = number
  description = "Maximum number of nodes"
  default     = 10
}

variable "node_instance_types" {
  type        = list(string)
  description = "Node instance types"
  default     = ["t3.large", "t3.xlarge"]
}

variable "db_master_username" {
  type        = string
  description = "Database master username"
  sensitive   = true
}

variable "db_master_password" {
  type        = string
  description = "Database master password"
  sensitive   = true
}
