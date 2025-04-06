terraform {
  required_providers {
    keycloak = {
      source  = "mrparkers/keycloak"
      version = "~> 4.4.0"
    }
  }
}

# Provider configuration
provider "keycloak" {
  client_id = "admin-cli"
  username  = var.keycloak_admin_user
  password  = var.keycloak_admin_password
  url       = var.keycloak_url
  realm     = "master"
}

# Create realm
resource "keycloak_realm" "realm_name" {
  realm   = var.keycloak_realm
  enabled = true
}

# ---------------------
# OpenID Clients
# ---------------------

# Backend confidential client
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

# Frontend public client
resource "keycloak_openid_client" "clear_budget_frontend" {
  realm_id                     = keycloak_realm.realm_name.id
  client_id                    = "clear-budget-frontend"
  name                         = "clear-budget-frontend"
  enabled                      = true
  access_type                  = "PUBLIC"
  standard_flow_enabled        = true
  direct_access_grants_enabled = true
  valid_redirect_uris          = ["http://localhost:4200/*"]
}

# ---------------------
# Realm Roles
# ---------------------

resource "keycloak_role" "clear_budget" {
  realm_id = keycloak_realm.realm_name.id
  name     = "clear-budget"
}

resource "keycloak_role" "clear_budget_admin" {
  realm_id = keycloak_realm.realm_name.id
  name     = "clear-budget-admin"
}

resource "keycloak_role" "clear_budget_m2m" {
  realm_id = keycloak_realm.realm_name.id
  name     = "clear-budget-m2m"
}

# ---------------------
# Assign roles to backend service account (m2m)
# ---------------------

data "keycloak_openid_client_service_account_user" "sa_user" {
  realm_id  = keycloak_realm.realm_name.id
  client_id = keycloak_openid_client.clear_budget.id
}

resource "keycloak_user_roles" "assign_m2m_role_to_sa" {
  realm_id = keycloak_realm.realm_name.id
  user_id  = data.keycloak_openid_client_service_account_user.sa_user.id
  role_ids = [
    keycloak_role.clear_budget.id,
    keycloak_role.clear_budget_m2m.id
  ]
}

# ---------------------
# Add "clear-budget" audience to both clients
# ---------------------

resource "keycloak_openid_audience_protocol_mapper" "clear_budget_m2m_audience" {
  name                      = "clear-budget-audience"
  realm_id                  = keycloak_realm.realm_name.id
  client_id                 = keycloak_openid_client.clear_budget.id
  included_client_audience = "clear-budget"
  add_to_id_token           = true
  add_to_access_token       = true
}

resource "keycloak_openid_audience_protocol_mapper" "frontend_audience" {
  name                      = "clear-budget-audience"
  realm_id                  = keycloak_realm.realm_name.id
  client_id                 = keycloak_openid_client.clear_budget_frontend.id
  included_client_audience = "clear-budget"
  add_to_id_token           = true
  add_to_access_token       = true
}

# ---------------------
# Write backend client secret to file for backend injection
# ---------------------

resource "local_file" "transaction_client_secret" {
  content = jsonencode({
    client_id     = keycloak_openid_client.clear_budget.client_id,
    client_secret = keycloak_openid_client.clear_budget.client_secret
  })
  filename = "/shared/client-secret.transaction.json"
}

# ---------------------
# Test user and roles
# ---------------------

resource "keycloak_user" "test_user" {
  realm_id   = keycloak_realm.realm_name.id
  username   = "testuser"
  enabled    = true
  email      = "testuser@example.com"
  first_name = "Test"
  last_name  = "User"
  initial_password {
    value     = "Test1234!"
    temporary = false
  }
}

resource "keycloak_user_roles" "test_user_roles" {
  realm_id = keycloak_realm.realm_name.id
  user_id  = keycloak_user.test_user.id
  role_ids = [
    keycloak_role.clear_budget.id,
    keycloak_role.clear_budget_admin.id
  ]
}
