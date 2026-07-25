module "ehr_dev" {
  source = "../../"

  environment            = "dev"
  project_name           = "ehr-platform"
  cloud_provider         = "aws"
  region                 = "us-east-1"
  availability_zones     = ["us-east-1a", "us-east-1b"]
  vpc_cidr               = "10.0.0.0/16"
  k8s_node_instance_type = "t3.medium"
  k8s_node_min_count     = 1
  k8s_node_max_count     = 3
  db_instance_class      = "db.t3.small"
  redis_node_type        = "cache.t3.micro"
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
    key    = "dev/terraform.tfstate"
    region = "us-east-1"
  }
}
