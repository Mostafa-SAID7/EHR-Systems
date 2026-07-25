variable "project_name"        { type = string }
variable "environment"         { type = string }
variable "cloud_provider"      { type = string }
variable "region"              { type = string }
variable "vpc_id"              { type = string }
variable "private_subnet_ids"  { type = list(string) }
variable "kafka_instance_type" { type = string }
variable "tags"                { type = map(string) }
