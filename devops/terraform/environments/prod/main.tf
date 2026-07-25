module "ehr_prod" {
  source = "../../"

  environment            = "prod"
  project_name           = "ehr-platform"
  cloud_provider         = "aws"
  region                 = "us-east-1"
  availability_zones     = ["us-east-1a", "us-east-1b", "us-east-1c"]
  vpc_cidr               = "10.2.0.0/16"
  k8s_node_instance_type = "t3.xlarge"
  k8s_node_min_count     = 3
  k8s_node_max_count     = 20
  db_instance_class      = "db.r6g.large"
  redis_node_type        = "cache.r6g.large"
  kafka_instance_type    = "kafka.m5.large"
  grafana_admin_password = var.grafana_admin_password
  owner_team             = "platform-engineering"
}

variable "grafana_admin_password" {
  type      = string
  sensitive = true
}

terraform {
  backend "s3" {
    bucket         = "ehr-platform-terraform-state"
    key            = "prod/terraform.tfstate"
    region         = "us-east-1"
    encrypt        = true
    dynamodb_table = "ehr-platform-terraform-locks"
  }
}
