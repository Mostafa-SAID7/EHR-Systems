variable "project_name" {
  type = string
}

variable "environment" {
  type = string
}

variable "cluster_endpoint" {
  type      = string
  sensitive = true
}

variable "grafana_admin_pass" {
  type      = string
  sensitive = true
}

variable "tags" {
  type = map(string)
}
