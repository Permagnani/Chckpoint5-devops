#!/usr/bin/env bash
# Remove todos os recursos do projeto (rode após a correção para não gastar créditos)
set -euo pipefail
az group delete --name rg-dimdim --yes --no-wait
echo "Exclusão do resource group rg-dimdim iniciada."
