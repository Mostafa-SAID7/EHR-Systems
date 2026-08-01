terraform {
  required_version = ">= 1.5"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }

  backend "s3" {
    bucket         = "ehr-platform-terraform-state"
    key            = "terraform.tfstate"
    region         = "us-east-1"
    encrypt        = true
    dynamodb_table = "ehr-terraform-locks"
  }
}

provider "aws" {
  region = var.aws_region

  default_tags {
    tags = {
      Environment = var.environment
      Project     = "EHR-Platform"
      ManagedBy   = "Terraform"
      CreatedAt   = timestamp()
    }
  }
}

# VPC and Networking
module "vpc" {
  source = "./modules/vpc"

  environment        = var.environment
  vpc_cidr          = var.vpc_cidr
  availability_zones = var.availability_zones
  private_subnet_cidrs = var.private_subnet_cidrs
  public_subnet_cidrs  = var.public_subnet_cidrs
}

# EKS Cluster
module "eks" {
  source = "./modules/eks"

  cluster_name           = "ehr-platform-${var.environment}"
  cluster_version        = var.kubernetes_version
  vpc_id                = module.vpc.vpc_id
  subnet_ids            = module.vpc.private_subnet_ids
  
  node_groups = {
    general = {
      desired_size       = var.node_group_desired_size
      min_size          = var.node_group_min_size
      max_size          = var.node_group_max_size
      instance_types    = var.node_instance_types
    }
  }

  environment = var.environment
}

# RDS PostgreSQL
module "rds" {
  source = "./modules/rds"

  cluster_identifier    = "ehr-platform-${var.environment}"
  database_name        = "ehr_platform"
  master_username      = var.db_master_username
  master_password      = var.db_master_password
  engine              = "aurora-postgresql"
  engine_version      = "15.2"
  
  subnet_ids           = module.vpc.private_subnet_ids
  security_group_ids   = [aws_security_group.rds.id]
  
  backup_retention_period = var.environment == "prod" ? 30 : 7
  multi_az            = var.environment == "prod"
  
  environment = var.environment
}

# ElastiCache Redis
module "redis" {
  source = "./modules/redis"

  cluster_id              = "ehr-platform-${var.environment}"
  engine                  = "redis"
  engine_version          = "7.0"
  node_type              = var.environment == "prod" ? "cache.r7g.large" : "cache.t4g.micro"
  num_cache_nodes        = var.environment == "prod" ? 3 : 1
  
  subnet_ids             = module.vpc.private_subnet_ids
  security_group_ids     = [aws_security_group.redis.id]
  
  automatic_failover_enabled = var.environment == "prod"
  multi_az_enabled          = var.environment == "prod"
  
  environment = var.environment
}

# SQS for messaging
module "sqs" {
  source = "./modules/sqs"

  queues = {
    patient_events       = { visibility_timeout = 300, message_retention = 1209600 }
    appointment_events   = { visibility_timeout = 300, message_retention = 1209600 }
    integration_events   = { visibility_timeout = 600, message_retention = 1209600 }
    clinical_events      = { visibility_timeout = 300, message_retention = 1209600 }
  }

  environment = var.environment
}

# SNS for pub/sub
module "sns" {
  source = "./modules/sns"

  topics = {
    patient_created      = {}
    appointment_scheduled = {}
    clinical_record_updated = {}
  }

  environment = var.environment
}

# S3 for FileStorage service
module "s3" {
  source = "./modules/s3"

  bucket_name_prefix = "ehr-platform-filestorage"
  
  versioning_enabled  = true
  encryption_enabled  = true
  
  cors_enabled = true
  cors_origins = ["https://ehr-platform.com"]
  
  lifecycle_rules = [
    {
      id      = "archive-old-files"
      prefix  = "archive/"
      days    = 90
      storage_class = "GLACIER"
    }
  ]

  environment = var.environment
}

# IAM Roles for EKS
resource "aws_iam_role" "eks_service_role" {
  name = "ehr-eks-service-role-${var.environment}"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "eks.amazonaws.com"
        }
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "eks_service_policy" {
  role       = aws_iam_role.eks_service_role.name
  policy_arn = "arn:aws:iam::aws:policy/AmazonEKSServiceRolePolicy"
}

# Security Groups
resource "aws_security_group" "rds" {
  name        = "ehr-rds-sg-${var.environment}"
  description = "Security group for RDS"
  vpc_id      = module.vpc.vpc_id

  ingress {
    from_port   = 5432
    to_port     = 5432
    protocol    = "tcp"
    cidr_blocks = [var.vpc_cidr]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "ehr-rds-sg-${var.environment}"
  }
}

resource "aws_security_group" "redis" {
  name        = "ehr-redis-sg-${var.environment}"
  description = "Security group for Redis"
  vpc_id      = module.vpc.vpc_id

  ingress {
    from_port   = 6379
    to_port     = 6379
    protocol    = "tcp"
    cidr_blocks = [var.vpc_cidr]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "ehr-redis-sg-${var.environment}"
  }
}

# CloudWatch Log Group
resource "aws_cloudwatch_log_group" "eks" {
  name              = "/aws/eks/ehr-platform-${var.environment}"
  retention_in_days = var.environment == "prod" ? 30 : 7

  tags = {
    Name = "ehr-eks-logs-${var.environment}"
  }
}
