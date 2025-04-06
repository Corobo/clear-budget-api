terraform {
  required_providers {
    rabbitmq = {
      source  = "cyrilgdn/rabbitmq"
      version = "~> 1.8.0"
    }
  }
}

provider "rabbitmq" {
  endpoint = var.rabbitmq_url
  username = var.rabbitmq_user
  password = var.rabbitmq_password
}

resource "rabbitmq_exchange" "category_exchange" {
  name     = "category.exchange"
  vhost    = var.rabbitmq_vhost
  settings {
    type        = "topic"
    durable     = true
    auto_delete = false
  }
}
resource "rabbitmq_queue" "transactions_category_queue" {
  name  = "transactions.category.queue"
  vhost = var.rabbitmq_vhost
  settings {
    durable     = true
    auto_delete = false
  }
}

resource "rabbitmq_binding" "category_to_transactions_queue" {
  source           = rabbitmq_exchange.category_exchange.name
  destination      = rabbitmq_queue.transactions_category_queue.name
  destination_type = "queue"
  routing_key      = "#"
  vhost            = var.rabbitmq_vhost
}
