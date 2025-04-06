variable "keycloak_url" {
  description = "URL of the Keycloak server"
  type        = string
}

variable "keycloak_admin_user" {
  description = "Admin username for Keycloak"
  type        = string
}

variable "keycloak_admin_password" {
  description = "Admin password for Keycloak"
  type        = string
}

variable "keycloak_realm" {
  description = "Realm where the client and roles will be created"
  type        = string
  default     = "customer"
}
