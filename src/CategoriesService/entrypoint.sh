#!/bin/bash
set -e

echo "🔒 Waiting for Terraform to finish provisioning..."
while [ ! -f /shared/.terraform_done ]; do
  echo "⏳ Still waiting for Terraform..."
  sleep 2
done

echo "✅ Terraform provisioning detected. Starting service..."
exec dotnet "$DLL_NAME"

