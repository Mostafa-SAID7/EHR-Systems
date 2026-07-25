output "vpc_id" {
  value = var.cloud_provider == "aws" ? aws_vpc.main[0].id : ""
}
output "private_subnet_ids" {
  value = var.cloud_provider == "aws" ? aws_subnet.private[*].id : []
}
output "public_subnet_ids" {
  value = var.cloud_provider == "aws" ? aws_subnet.public[*].id : []
}
