terraform {
  required_providers {
    keycloak = {
      source  = "mrparkers/keycloak"
      version = "~> 4.1.0"
    }
  }
}

provider "keycloak" {
  client_id = "admin-cli"
  username  = var.keycloak_admin_user
  password  = var.keycloak_admin_password
  url       = var.keycloak_url
  realm     = "master"
}

resource "keycloak_openid_client" "clear_budget" {
  realm_id                 = var.keycloak_realm
  client_id                = "clear-budget"
  name                     = "clear-budget"
  enabled                  = true
  public_client            = false
  service_accounts_enabled = true
}

resource "keycloak_role" "clear_budget" {
  realm_id  = var.keycloak_realm
  client_id = keycloak_openid_client.clear_budget.id
  name      = "clear-budget"
}

resource "keycloak_role" "clear_budget_admin" {
  realm_id  = var.keycloak_realm
  client_id = keycloak_openid_client.clear_budget.id
  name      = "clear-budget-admin"
}

resource "keycloak_role" "clear_budget_m2m" {
  realm_id  = var.keycloak_realm
  client_id = keycloak_openid_client.clear_budget.id
  name      = "clear-budget-m2m"
}

resource "keycloak_openid_client_service_account_user" "sa" {
  realm_id  = var.keycloak_realm
  client_id = keycloak_openid_client.clear_budget.id
}

resource "keycloak_user_roles" "assign_m2m_role_to_sa" {
  realm_id  = var.keycloak_realm
  user_id   = keycloak_openid_client_service_account_user.sa.id
  client_id = keycloak_openid_client.clear_budget.id
  role_ids  = [keycloak_role.clear_budget_m2m.id]
}
