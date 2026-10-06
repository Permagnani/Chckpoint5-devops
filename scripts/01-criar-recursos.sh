#!/usr/bin/env bash
# =====================================================================
# DimDim - Criação de TODOS os recursos no Azure via Azure CLI
#   Resource Group, Azure SQL Server + Database (PaaS), firewall,
#   tabelas (DDL), Log Analytics + Application Insights,
#   App Service Plan (Linux) + Web App .NET 8 e configurações.
#
# Uso (Azure Cloud Shell - Bash):
#   export SQL_ADMIN_PASSWORD='SuaSenhaForte@2026'   # nunca commite a senha
#   bash scripts/01-criar-recursos.sh
# =====================================================================
set -euo pipefail

# ---------- Variáveis (ajuste o RM para garantir nomes únicos) ----------
RM="${RM:-rm000000}"                         # RM do representante do grupo
LOCATION="${LOCATION:-canadacentral}"        # se der erro de região: eastus2, centralus...
RG="rg-dimdim"
SQL_SERVER="sql-dimdim-${RM}"
SQL_DB="db-dimdim"
SQL_ADMIN_USER="${SQL_ADMIN_USER:-dimdimadmin}"
PLAN="plan-dimdim"
PLAN_SKU="${PLAN_SKU:-F1}"                   # F1 (gratuito) ou B1
WEBAPP="app-dimdim-${RM}"
LAW="log-dimdim"
APPI="appi-dimdim"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# ---------- Senha do banco: vem do ambiente ou é digitada (não fica no código) ----------
if [[ -z "${SQL_ADMIN_PASSWORD:-}" ]]; then
  read -r -s -p "Senha do administrador do SQL Server: " SQL_ADMIN_PASSWORD
  echo
fi

echo ">> Registrando providers necessários"
for p in Microsoft.Web Microsoft.Sql Microsoft.Insights Microsoft.OperationalInsights; do
  az provider register --namespace "$p" --output none
done

echo ">> Extensão application-insights do CLI"
az extension add --name application-insights --upgrade --only-show-errors --output none

echo ">> [1/8] Resource Group: $RG ($LOCATION)"
az group create --name "$RG" --location "$LOCATION" --output none

echo ">> [2/8] Azure SQL Server: $SQL_SERVER"
az sql server create \
  --name "$SQL_SERVER" \
  --resource-group "$RG" \
  --location "$LOCATION" \
  --admin-user "$SQL_ADMIN_USER" \
  --admin-password "$SQL_ADMIN_PASSWORD" \
  --enable-public-network true \
  --output none

echo ">> [3/8] Azure SQL Database: $SQL_DB"
az sql db create \
  --resource-group "$RG" \
  --server "$SQL_SERVER" \
  --name "$SQL_DB" \
  --service-objective Basic \
  --backup-storage-redundancy Local \
  --output none

echo ">> [4/8] Firewall: serviços do Azure + IP desta máquina"
az sql server firewall-rule create \
  --resource-group "$RG" --server "$SQL_SERVER" \
  --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0 \
  --output none
MEU_IP="$(curl -s https://api.ipify.org || true)"
if [[ -n "$MEU_IP" ]]; then
  az sql server firewall-rule create \
    --resource-group "$RG" --server "$SQL_SERVER" \
    --name AllowCliente --start-ip-address "$MEU_IP" --end-ip-address "$MEU_IP" \
    --output none
fi

echo ">> [5/8] Criando as tabelas (scripts/ddl.sql)"
if command -v sqlcmd >/dev/null 2>&1; then
  sqlcmd -S "tcp:${SQL_SERVER}.database.windows.net,1433" -d "$SQL_DB" \
         -U "$SQL_ADMIN_USER" -P "$SQL_ADMIN_PASSWORD" -i "${SCRIPT_DIR}/ddl.sql"
else
  echo "   sqlcmd não encontrado: execute scripts/ddl.sql no Query editor do portal."
fi

echo ">> [6/8] Log Analytics + Application Insights"
az monitor log-analytics workspace create \
  --resource-group "$RG" --workspace-name "$LAW" --location "$LOCATION" --output none
LAW_ID="$(az monitor log-analytics workspace show -g "$RG" -n "$LAW" --query id -o tsv)"
az monitor app-insights component create \
  --app "$APPI" --resource-group "$RG" --location "$LOCATION" \
  --application-type web --kind web --workspace "$LAW_ID" --output none
APPI_CONN="$(az monitor app-insights component show --app "$APPI" -g "$RG" --query connectionString -o tsv)"

echo ">> [7/8] App Service Plan ($PLAN_SKU, Linux) + Web App .NET 8: $WEBAPP"
az appservice plan create \
  --name "$PLAN" --resource-group "$RG" --location "$LOCATION" \
  --sku "$PLAN_SKU" --is-linux --output none
az webapp create \
  --name "$WEBAPP" --resource-group "$RG" --plan "$PLAN" \
  --runtime "DOTNETCORE:8.0" --output none
az webapp update --name "$WEBAPP" --resource-group "$RG" --https-only true --output none

echo ">> [8/8] Configurações do Web App (segredos ficam no Azure, não no código)"
SQL_CONN="Server=tcp:${SQL_SERVER}.database.windows.net,1433;Initial Catalog=${SQL_DB};Persist Security Info=False;User ID=${SQL_ADMIN_USER};Password=${SQL_ADMIN_PASSWORD};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
az webapp config connection-string set \
  --name "$WEBAPP" --resource-group "$RG" \
  --connection-string-type SQLAzure \
  --settings DimDimDb="$SQL_CONN" --output none
az webapp config appsettings set \
  --name "$WEBAPP" --resource-group "$RG" \
  --settings APPLICATIONINSIGHTS_CONNECTION_STRING="$APPI_CONN" ASPNETCORE_ENVIRONMENT=Production \
  --output none

# Necessário para o GitHub Actions usar o Publish Profile
az resource update --resource-group "$RG" --namespace Microsoft.Web \
  --resource-type basicPublishingCredentialsPolicies --parent "sites/${WEBAPP}" \
  --name scm --set properties.allow=true --output none

echo
echo "================================================================"
echo " Recursos criados!"
echo " Web App : https://${WEBAPP}.azurewebsites.net"
echo " SQL     : ${SQL_SERVER}.database.windows.net / ${SQL_DB}"
echo " Insights: ${APPI}"
echo "================================================================"
echo " Próximo passo (deploy via GitHub Actions):"
echo "  1) Secret AZURE_WEBAPP_PUBLISH_PROFILE = conteúdo do comando abaixo"
echo "  2) Variable AZURE_WEBAPP_NAME = ${WEBAPP}"
echo
echo "  az webapp deployment list-publishing-profiles -g ${RG} -n ${WEBAPP} --xml"
echo
if command -v gh >/dev/null 2>&1 && gh auth status >/dev/null 2>&1; then
  read -r -p "GitHub CLI detectado. Configurar secret e variable no repositório agora? (s/N) " RESP
  if [[ "$RESP" =~ ^[sS]$ ]]; then
    az webapp deployment list-publishing-profiles -g "$RG" -n "$WEBAPP" --xml \
      | gh secret set AZURE_WEBAPP_PUBLISH_PROFILE
    gh variable set AZURE_WEBAPP_NAME --body "$WEBAPP"
    echo "  Secret e variable configurados."
  fi
fi
