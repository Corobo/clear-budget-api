variable "rabbitmq_url" {
  description = "URL del panel de administración de RabbitMQ"
  type        = string
}

variable "rabbitmq_user" {
  description = "Usuario de RabbitMQ"
  type        = string
}

variable "rabbitmq_password" {
  description = "Contraseña de RabbitMQ"
  type        = string
}

variable "rabbitmq_vhost" {
  description = "Virtual host en RabbitMQ"
  type        = string
  default     = "/"
}
