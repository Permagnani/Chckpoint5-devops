#!/usr/bin/env bash
# =====================================================================
# DimDim - Deploy alternativo via Azure CLI (az webapp deploy)
# Use se preferir não usar o GitHub Actions.
#   bash scripts/02-deploy-cli.sh
# =====================================================================
set -euo pipefail

RM="${RM:-rm000000}"
RG="rg-dimdim"
WEBAPP="app-dimdim-${RM}"
RAIZ="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

echo ">> Publicando a aplicação (.NET 8, Release)"
rm -rf "$RAIZ/publish" "$RAIZ/app.zip"
dotnet publish "$RAIZ/src/DimDim.Web/DimDim.Web.csproj" -c Release -o "$RAIZ/publish"

echo ">> Compactando"
(cd "$RAIZ/publish" && zip -qr "$RAIZ/app.zip" .)

echo ">> Enviando para o Web App $WEBAPP"
az webapp deploy \
  --resource-group "$RG" \
  --name "$WEBAPP" \
  --src-path "$RAIZ/app.zip" \
  --type zip

echo ">> Pronto: https://${WEBAPP}.azurewebsites.net"
