#!/bin/bash
set -e

rm -f /shared/.terraform_done

echo "⏳ Waiting for RabbitMQ..."
until curl -s http://rabbitmq:15672 > /dev/null; do
  echo "⏳ RabbitMQ not ready yet..."
  sleep 2
done

echo "⏳ Waiting for Keycloak..."
until curl -s http://keycloak:8080/health > /dev/null; do
  echo "⏳ Keycloak not ready yet..."
  sleep 2
done

echo "✅ Dependencies are up. Starting Terraform provisioning..."
cd /infra/keycloak
terraform init
terraform apply -auto-approve

cd /infra/rabbitmq
terraform init
terraform apply -auto-approve

echo "✅ Terraform provisioning complete."
touch /shared/.terraform_done
