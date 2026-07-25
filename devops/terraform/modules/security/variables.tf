variable "project_name"    { type = string }
variable "environment"     { type = string }
variable "cloud_provider"  { type = string }
variable "region"          { type = string }
variable "cluster_arn"     { type = string }
variable "storage_bucket"  { type = string }
variable "tags"            { type = map(string) }
