# =============================================================================
# Messaging Module — Amazon MSK (Kafka) + RabbitMQ via Amazon MQ
# =============================================================================

locals {
  name_prefix = "${var.project_name}-${var.environment}"
}

# ── Amazon MSK (Kafka) ────────────────────────────────────────────────────────
resource "aws_msk_cluster" "kafka" {
  count          = var.cloud_provider == "aws" ? 1 : 0
  cluster_name   = "${local.name_prefix}-kafka"
  kafka_version  = "3.6.0"
  number_of_broker_nodes = var.environment == "prod" ? 3 : 1

  broker_node_group_info {
    instance_type   = var.kafka_instance_type
    client_subnets  = var.private_subnet_ids
    storage_info {
      ebs_storage_info { volume_size = 100 }
    }
  }

  encryption_info {
    encryption_in_transit {
      client_broker = "TLS"          # HIPAA: in-transit encryption
      in_cluster    = true
    }
  }

  logging_info {
    broker_logs {
      cloudwatch_logs {
        enabled   = true
        log_group = "/aws/msk/${local.name_prefix}"
      }
    }
  }

  tags = var.tags
}

# ── Amazon MQ (RabbitMQ) ──────────────────────────────────────────────────────
resource "aws_mq_broker" "rabbitmq" {
  count              = var.cloud_provider == "aws" ? 1 : 0
  broker_name        = "${local.name_prefix}-rabbitmq"
  engine_type        = "RabbitMQ"
  engine_version     = "3.12.13"
  host_instance_type = "mq.t3.micro"
  deployment_mode    = var.environment == "prod" ? "CLUSTER_MULTI_AZ" : "SINGLE_INSTANCE"

  user {
    username = "ehr_user"
    password = random_password.rabbitmq[0].result
  }

  subnet_ids         = [var.private_subnet_ids[0]]
  publicly_accessible = false

  tags = var.tags
}

resource "random_password" "rabbitmq" {
  count   = var.cloud_provider == "aws" ? 1 : 0
  length  = 24
  special = false
}
