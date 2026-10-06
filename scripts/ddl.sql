/* =====================================================================
   DimDim - DDL das tabelas (Azure SQL Database)
   Relacionamento: Clientes (1) ---- (N) Contas
   ===================================================================== */

IF OBJECT_ID('dbo.Contas', 'U') IS NOT NULL DROP TABLE dbo.Contas;
IF OBJECT_ID('dbo.Clientes', 'U') IS NOT NULL DROP TABLE dbo.Clientes;
GO

CREATE TABLE dbo.Clientes (
    Id            INT IDENTITY(1,1) NOT NULL,
    Nome          NVARCHAR(100)     NOT NULL,
    Cpf           CHAR(11)          NOT NULL,
    Email         NVARCHAR(150)     NOT NULL,
    Telefone      NVARCHAR(20)      NULL,
    DataCadastro  DATETIME2         NOT NULL CONSTRAINT DF_Clientes_DataCadastro DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Clientes PRIMARY KEY (Id),
    CONSTRAINT UQ_Clientes_Cpf UNIQUE (Cpf)
);
GO

CREATE TABLE dbo.Contas (
    Id            INT IDENTITY(1,1) NOT NULL,
    Numero        NVARCHAR(20)      NOT NULL,
    Agencia       NVARCHAR(10)      NOT NULL,
    Tipo          NVARCHAR(20)      NOT NULL,
    Saldo         DECIMAL(18,2)     NOT NULL CONSTRAINT DF_Contas_Saldo DEFAULT 0,
    DataAbertura  DATETIME2         NOT NULL CONSTRAINT DF_Contas_DataAbertura DEFAULT SYSUTCDATETIME(),
    ClienteId     INT               NOT NULL,
    CONSTRAINT PK_Contas PRIMARY KEY (Id),
    CONSTRAINT UQ_Contas_Numero UNIQUE (Numero),
    CONSTRAINT CK_Contas_Tipo CHECK (Tipo IN ('Corrente', 'Poupanca', 'Investimento')),
    CONSTRAINT CK_Contas_Saldo CHECK (Saldo >= 0),
    CONSTRAINT FK_Contas_Clientes FOREIGN KEY (ClienteId)
        REFERENCES dbo.Clientes (Id) ON DELETE CASCADE
);
GO

CREATE INDEX IX_Contas_ClienteId ON dbo.Contas (ClienteId);
GO
