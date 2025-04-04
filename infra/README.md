# Infrastructure Provisioning with Terraform

This directory contains the infrastructure as code setup for provisioning **Keycloak** and **RabbitMQ** dependencies used by the ClearBudget microservices architecture.

---

## 📁 Directory Structure

```
infra/
├── keycloak/           # Terraform module to configure client, roles, and service account
│   ├── main.tf
│   └── variables.tf
├── rabbitmq/           # Terraform module to provision exchange, queue, and bindings
│   ├── main.tf
│   └── variables.tf
├── terraform.tfvars    # Optional: override default variable values
└── README.md
```

---

## 🚀 Usage (Local)

Make sure the following services are running in Docker before provisioning:

- `Keycloak` available at `http://localhost:8080`
- `RabbitMQ Management UI` at `http://localhost:15672`

### 🧪 Initialize and Apply

Each module can be applied individually:

```bash
cd infra/keycloak
terraform init
terraform apply
```

Or for RabbitMQ:

```bash
cd infra/rabbitmq
terraform init
terraform apply
```

You can pass custom values using a `.tfvars` file or environment variables:

```bash
terraform apply -var-file=../terraform.tfvars
```

---

## 🤖 Usage in CI/CD

When integrating into a CI pipeline:

- Use environment variables to override sensitive or dynamic values.
- Avoid hardcoding service URLs or credentials.

Example:

```yaml
- name: Terraform Apply
  run: terraform apply -auto-approve
  env:
    TF_VAR_keycloak_url: http://keycloak:8080
    TF_VAR_keycloak_admin_user: admin
    TF_VAR_keycloak_admin_password: admin
    TF_VAR_rabbitmq_url: http://rabbitmq:15672
    TF_VAR_rabbitmq_user: user
    TF_VAR_rabbitmq_password: password
```

---

## ✅ Goals

- Fully automate Keycloak setup:
  - Client: `clear-budget`
  - Roles: `clear-budget`, `clear-budget-admin`, `clear-budget-m2m`
  - Assign `clear-budget-m2m` to service account

- Provision RabbitMQ:
  - Exchange: `category.exchange`
  - Queue: `transactions.category.queue`
  - Bind queue to exchange (routing key: `#`)
