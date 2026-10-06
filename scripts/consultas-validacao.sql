/* Consultas para mostrar no vídeo a persistência após cada operação CRUD.
   Rode no Query editor do Azure SQL Database (portal) ou via sqlcmd. */

-- Tabela Clientes
SELECT Id, Nome, Cpf, Email, Telefone, DataCadastro
FROM dbo.Clientes
ORDER BY Id;

-- Tabela Contas
SELECT Id, Agencia, Numero, Tipo, Saldo, DataAbertura, ClienteId
FROM dbo.Contas
ORDER BY Id;

-- Relacionamento (JOIN) Clientes x Contas
SELECT cl.Id AS ClienteId, cl.Nome, ct.Id AS ContaId, ct.Agencia, ct.Numero, ct.Tipo, ct.Saldo
FROM dbo.Clientes cl
LEFT JOIN dbo.Contas ct ON ct.ClienteId = cl.Id
ORDER BY cl.Id, ct.Id;
