module "ehr_staging" {
  source = "../../"

  environment            = "staging"
  project_name           = "ehr-platform"
  cloud_provider         = "aws"
  region                 = "us-east-1"
  availability_zones     = ["us-east-1a", "us-east-1b", "us-east-1c"]
  vpc_cidr               = "10.1.0.0/16"
  k8s_node_instance_type = "t3.large"
  k8s_node_min_count     = 2
  k8s_node_max_count     = 6
  db_instance_class      = "db.t3.medium"
  redis_node_type        = "cache.t3.small"
  kafka_instance_type    = "kafka.t3.small"
  grafana_admin_password = var.grafana_admin_password
  owner_team             = "platform-engineering"
}

variable "grafana_admin_password" {
  type      = string
  sensitive = true
}

terraform {
  backend "s3" {
    bucket = "ehr-platform-terraform-state"
    key    = "staging/terraform.tfstate"
    region = "us-east-1"
  }
}
