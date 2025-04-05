terraform {
  required_providers {
    keycloak = {
      source  = "mrparkers/keycloak"
      version = "~> 4.4.0"
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

resource "keycloak_realm" "realm_name" {
  realm   = var.keycloak_realm
  enabled = true
}

resource "keycloak_openid_client" "clear_budget" {
  realm_id                     = keycloak_realm.realm_name.id
  client_id                    = "clear-budget"
  name                         = "clear-budget"
  enabled                      = true
  access_type                  = "CONFIDENTIAL"
  service_accounts_enabled     = true
  standard_flow_enabled        = true
  direct_access_grants_enabled = true
  valid_redirect_uris          = ["http://localhost/*"]
}

resource "keycloak_role" "clear_budget" {
  realm_id  = keycloak_realm.realm_name.id
  client_id = keycloak_openid_client.clear_budget.id
  name      = "clear-budget"
}

resource "keycloak_role" "clear_budget_admin" {
  realm_id  = keycloak_realm.realm_name.id
  client_id = keycloak_openid_client.clear_budget.id
  name      = "clear-budget-admin"
}

resource "keycloak_role" "clear_budget_m2m" {
  realm_id  = keycloak_realm.realm_name.id
  client_id = keycloak_openid_client.clear_budget.id
  name      = "clear-budget-m2m"
}

data "keycloak_openid_client_service_account_user" "sa_user" {
  realm_id  = keycloak_realm.realm_name.id
  client_id = keycloak_openid_client.clear_budget.id
}

resource "keycloak_user_roles" "assign_m2m_role_to_sa" {
  realm_id = keycloak_realm.realm_name.id
  user_id  = data.keycloak_openid_client_service_account_user.sa_user.id
  role_ids = [keycloak_role.clear_budget_m2m.id, keycloak_role.clear_budget.id]
}
