#!/bin/sh
set -e
export VAULT_ADDR=http://vault:8200

echo "==> Waiting for Vault to be reachable..."
until vault status >/dev/null 2>&1; VAULT_EXIT=$?; [ "$VAULT_EXIT" != "1" ]; do
  echo "Vault not reachable yet (exit=$VAULT_EXIT)..."
  sleep 2
done
echo "==> Vault is reachable."

echo "==> Checking Vault initialization status..."
VAULT_STATUS=$(vault status -format=json 2>/dev/null) || true
if [ -z "$VAULT_STATUS" ]; then
  VAULT_STATUS='{}'
fi
INITIALIZED=$(echo "$VAULT_STATUS" | grep -o '"initialized":[^,}]*' | cut -d':' -f2 | tr -d ' ')

if [ "$INITIALIZED" != "true" ]; then
  echo "==> Initializing Vault..."

  INIT_OUTPUT=$(vault operator init -key-shares=5 -key-threshold=3)

  echo "$INIT_OUTPUT"
  echo ""
  echo "=========================================="
  echo "SAVE THESE CREDENTIALS IMMEDIATELY!"
  echo "=========================================="

  KEY1=$(echo "$INIT_OUTPUT" | grep "Unseal Key 1:" | awk '{print $NF}')
  KEY2=$(echo "$INIT_OUTPUT" | grep "Unseal Key 2:" | awk '{print $NF}')
  KEY3=$(echo "$INIT_OUTPUT" | grep "Unseal Key 3:" | awk '{print $NF}')
  ROOT_TOKEN=$(echo "$INIT_OUTPUT" | grep "Initial Root Token:" | awk '{print $NF}')

  mkdir -p /vault-credentials
  echo "$KEY1" > /vault-credentials/unseal-key-1
  echo "$KEY2" > /vault-credentials/unseal-key-2
  echo "$KEY3" > /vault-credentials/unseal-key-3
  echo "$ROOT_TOKEN" > /vault-credentials/root-token

  echo "==> Unsealing Vault (1/3)..."
  vault operator unseal "$KEY1"

  echo "==> Unsealing Vault (2/3)..."
  vault operator unseal "$KEY2"

  echo "==> Unsealing Vault (3/3)..."
  vault operator unseal "$KEY3"

  export VAULT_TOKEN="$ROOT_TOKEN"
else
  echo "==> Vault already initialized"

  SEALED=$(echo "$VAULT_STATUS" | grep -o '"sealed":[^,}]*' | cut -d':' -f2 | tr -d ' ')
  if [ "$SEALED" = "true" ]; then
    echo "==> Vault is sealed, attempting to unseal..."

    if [ -f /vault-credentials/unseal-key-1 ] && [ -f /vault-credentials/unseal-key-2 ] && [ -f /vault-credentials/unseal-key-3 ]; then
      echo "==> Found saved unseal keys, unsealing..."

      KEY1=$(cat /vault-credentials/unseal-key-1)
      KEY2=$(cat /vault-credentials/unseal-key-2)
      KEY3=$(cat /vault-credentials/unseal-key-3)

      echo "==> Unsealing Vault (1/3)..."
      vault operator unseal "$KEY1"

      echo "==> Unsealing Vault (2/3)..."
      vault operator unseal "$KEY2"

      echo "==> Unsealing Vault (3/3)..."
      vault operator unseal "$KEY3"

      echo "==> Vault unsealed successfully!"
    else
      echo "ERROR: Vault is sealed but no saved unseal keys found!"
      exit 1
    fi
  fi

  if [ -z "$VAULT_TOKEN" ]; then
    if [ -f /vault-credentials/root-token ]; then
      export VAULT_TOKEN=$(cat /vault-credentials/root-token)
      echo "==> Using saved root token"
    else
      echo "ERROR: VAULT_TOKEN not provided and no saved token found!"
      exit 1
    fi
  fi
fi

echo "==> Logging in..."
vault login "$VAULT_TOKEN" > /dev/null

echo "==> Enabling transit secrets engine..."
vault secrets enable transit 2>/dev/null || echo "Transit already enabled"

echo "==> Enabling AppRole auth method..."
vault auth enable approle 2>/dev/null || echo "AppRole already enabled"

echo "==> Enabling audit logging..."
vault audit enable file file_path=/vault/logs/audit.log 2>/dev/null || echo "Audit already enabled"

SERVICES="identity kyc document"

for SERVICE in $SERVICES
do
  echo "==> Setting up $SERVICE service..."

  vault write transit/keys/$SERVICE \
    type=aes256-gcm96 \
    exportable=false \
    allow_plaintext_backup=false 2>/dev/null || echo "  Key already exists"

  vault policy write $SERVICE - <<POLICY
path "transit/encrypt/$SERVICE" {
  capabilities = ["update"]
}
path "transit/decrypt/$SERVICE" {
  capabilities = ["update"]
}
path "transit/rewrap/$SERVICE" {
  capabilities = ["update"]
}
path "sys/renew-self" {
  capabilities = ["update"]
}
POLICY

  vault write auth/approle/role/$SERVICE \
    token_policies="$SERVICE" \
    token_ttl=30m \
    token_max_ttl=2h \
    token_num_uses=0 \
    bind_secret_id=true \
    secret_id_ttl=0

  ROLE_ID=$(vault read -field=role_id auth/approle/role/$SERVICE/role-id)
  SECRET_ID=$(vault write -field=secret_id -f auth/approle/role/$SERVICE/secret-id)

  mkdir -p /vault-credentials/$SERVICE
  echo "$ROLE_ID" > /vault-credentials/$SERVICE/role-id
  echo "$SECRET_ID" > /vault-credentials/$SERVICE/secret-id

  echo "✓ $SERVICE configured (RoleID: $ROLE_ID)"
done

echo ""
echo "==> Vault configuration complete!"