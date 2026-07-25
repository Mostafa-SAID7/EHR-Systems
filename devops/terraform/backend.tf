# =============================================================================
# Remote State — S3 + DynamoDB locking (AWS default)
# Override via -backend-config for Azure Blob or GCS.
# =============================================================================
#
# Before first use:
#   aws s3 mb s3://ehr-platform-terraform-state-<account-id>
#   aws dynamodb create-table \
#     --table-name ehr-platform-terraform-locks \
#     --attribute-definitions AttributeName=LockID,AttributeType=S \
#     --key-schema AttributeName=LockID,KeyType=HASH \
#     --billing-mode PAY_PER_REQUEST
#
terraform {
  backend "s3" {
    bucket         = "ehr-platform-terraform-state"   # replace with your bucket
    key            = "terraform.tfstate"               # overridden per-env
    region         = "us-east-1"
    encrypt        = true                              # SSE-S3
    dynamodb_table = "ehr-platform-terraform-locks"   # state locking
  }
}

# Azure alternative (comment out S3 block, uncomment this):
# terraform {
#   backend "azurerm" {
#     resource_group_name  = "ehr-platform-tfstate-rg"
#     storage_account_name = "ehrplatformtfstate"
#     container_name       = "tfstate"
#     key                  = "terraform.tfstate"
#   }
# }

# GCS alternative:
# terraform {
#   backend "gcs" {
#     bucket = "ehr-platform-terraform-state"
#     prefix = "terraform/state"
#   }
# }
