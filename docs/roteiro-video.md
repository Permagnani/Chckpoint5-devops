# Roteiro do vídeo (mínimo 720p, com explicação falada)

Grave a tela em 1080p (ou no mínimo 720p) e **narre** cada etapa. Penalidades do professor estão marcadas com ⚠️.

## 1. Abertura (≈30 s)
- Nome do grupo e integrantes.
- Mostrar o repositório no GitHub: README, `scripts/`, `src/`, `api/operacoes.json`, desenho da arquitetura.

## 2. Criação dos recursos em nuvem (≈3 min)
- Abrir o Cloud Shell, `git clone`, `export RM=...` e `export SQL_ADMIN_PASSWORD=...` (⚠️ não mostre a senha em texto no repositório; no vídeo, prefira digitar com `read -s` deixando a variável vazia).
- Rodar `bash scripts/01-criar-recursos.sh` e comentar cada etapa.
- No portal, mostrar o resource group `rg-dimdim` com: SQL Server, SQL Database, App Service Plan, Web App, Application Insights e Log Analytics.
- Mostrar as tabelas criadas no Query editor.

## 3. Deploy sendo efetuado (≈2 min)
- Mostrar o secret `AZURE_WEBAPP_PUBLISH_PROFILE` e a variable `AZURE_WEBAPP_NAME` (só os nomes).
- Disparar o workflow (push ou *Run workflow*) e mostrar os jobs `build` e `deploy` concluídos.
- Abrir a URL `https://app-dimdim-<RM>.azurewebsites.net`.

## 4. Testes com persistência (≈5 min) ⚠️ menos 3 pontos se não mostrar cada operação
Deixe duas abas lado a lado: o app e o Query editor. **Depois de cada operação, rode o SELECT da tabela.**

**Clientes**
1. Create: cadastrar 2 clientes → `SELECT * FROM dbo.Clientes`.
2. Read: abrir a lista e o detalhe → mostrar que bate com o SELECT.
3. Update: editar e-mail de um cliente → `SELECT` mostrando o novo e-mail.
4. Delete: fica para o final (passo 9).

**Contas**
5. Create: abrir 2 contas para o cliente 1 e 1 para o cliente 2 → `SELECT * FROM dbo.Contas`.
6. Read: lista e detalhe da conta.
7. Update: mudar tipo e saldo de uma conta → `SELECT`.
8. Delete: encerrar uma conta → `SELECT` mostrando que sumiu.

**Relacionamento**
9. Excluir o cliente 1 → `SELECT` nas duas tabelas mostrando que o cliente e as contas dele foram removidos (cascata).
10. (Opcional) Rodar 2 ou 3 chamadas da API com `curl` e mostrar o resultado no banco.

## 5. Monitoramento do App e Banco (≈2 min) ⚠️ menos 1,5 ponto se não mostrar
- Application Insights → **Live Metrics** enquanto clica no app.
- **Application map** mostrando Web App → SQL.
- **Logs**: rodar as consultas `requests`, `dependencies` (type SQL, com o texto do comando) e `customEvents` do README.

## 6. Encerramento (≈20 s)
- Recapitular: Web App .NET + Azure SQL (PaaS) + deploy automatizado + Application Insights.

## Checklist antes de entregar
- [ ] Repositório **público** ou com o professor como colaborador (⚠️ sem acesso = zero).
- [ ] Vídeo público/não listado no YouTube, 720p+, com áudio.
- [ ] README com integrantes e link do vídeo preenchidos.
- [ ] Nenhuma senha, connection string ou token no código.
- [ ] PDF `<nome_grupo>_webapp.pdf` com: nome do grupo, RM e nome dos integrantes, link do GitHub e link do vídeo (mais nada).
- [ ] Upload feito **só pelo representante** na tarefa do Teams.
- [ ] Recursos ainda no ar até a correção.
