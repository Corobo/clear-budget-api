#!/bin/bash
set -e

echo "🔒 Waiting for Terraform to finish provisioning..."
while [ ! -f /shared/.terraform_done ]; do
  echo "⏳ Still waiting for Terraform..."
  sleep 2
done

echo "🔎 Extracting client secret from JSON..."
CLIENT_SECRET=$(jq -r '.client_secret' /shared/client-secret.transaction.json)

echo "✅ Secret exported. Starting service..."
exec env Auth__ClientSecret="$CLIENT_SECRET" dotnet "$DLL_NAME"

