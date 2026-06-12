-- ============================================================
-- CONTROLE FINANCEIRO - Script de criação do banco de dados
-- SQL Server
-- ============================================================

CREATE DATABASE ControleFinanceiro;
GO

USE ControleFinanceiro;
GO

-- ============================================================
-- TABELA: Anos fiscais
-- ============================================================
CREATE TABLE AnoFiscal (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Ano         INT NOT NULL UNIQUE,
    Descricao   NVARCHAR(100) NULL,
    CriadoEm   DATETIME2 DEFAULT GETDATE()
);

-- ============================================================
-- GRUPOS DE RECEITA
-- ============================================================
CREATE TABLE GrupoReceita (
    Id      INT IDENTITY(1,1) PRIMARY KEY,
    Nome    NVARCHAR(100) NOT NULL,   -- Ex: 'Entradas', 'Outros'
    Ativo   BIT DEFAULT 1
);

INSERT INTO GrupoReceita (Nome) VALUES ('Entradas'), ('Outros');

-- ============================================================
-- CATEGORIAS DE RECEITA
-- ============================================================
CREATE TABLE CategoriaReceita (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    GrupoReceitaId  INT NOT NULL REFERENCES GrupoReceita(Id),
    Nome            NVARCHAR(100) NOT NULL,  -- Ex: 'BO Financeiro - IA', 'Estágio SMN'
    Ativo           BIT DEFAULT 1
);

INSERT INTO CategoriaReceita (GrupoReceitaId, Nome) VALUES
    (1, 'Clientes'),
    (1, 'BO Financeiro - IA'),
    (1, 'Estágio SMN'),
    (1, 'Outros'),
    (2, 'Transferência de poupança'),
    (2, 'Renda de juros'),
    (2, 'Restituições'),
    (2, 'Saldo anterior');  -- legado: desativar após implementar SaldoInicial no AnoFiscal

-- ============================================================
-- CLIENTES (lista de clientes da aba Receita)
-- ============================================================
CREATE TABLE Cliente (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Nome        NVARCHAR(150) NOT NULL,
    ValorMensal DECIMAL(18,2) NULL,   -- valor padrão/contratado
    Ativo       BIT DEFAULT 1,
    CriadoEm   DATETIME2 DEFAULT GETDATE()
);

-- ============================================================
-- LANÇAMENTOS DE RECEITA
-- ============================================================
CREATE TABLE LancamentoReceita (
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    AnoFiscalId         INT NOT NULL REFERENCES AnoFiscal(Id),
    CategoriaReceitaId  INT NOT NULL REFERENCES CategoriaReceita(Id),
    ClienteId           INT NULL REFERENCES Cliente(Id),
    Mes                 TINYINT NOT NULL CHECK (Mes BETWEEN 1 AND 12),
    Valor               DECIMAL(18,2) NOT NULL DEFAULT 0,
    Observacao          NVARCHAR(500) NULL,
    LancadoEm          DATETIME2 DEFAULT GETDATE(),
    AtualizadoEm       DATETIME2 NULL
);

-- ============================================================
-- GRUPOS DE DESPESA
-- ============================================================
CREATE TABLE GrupoDespesa (
    Id      INT IDENTITY(1,1) PRIMARY KEY,
    Nome    NVARCHAR(100) NOT NULL,  -- Ex: 'Geral', 'Empresa', 'Transporte'...
    Ativo   BIT DEFAULT 1
);

INSERT INTO GrupoDespesa (Nome) VALUES
    ('Geral'),
    ('Empresa'),
    ('Acordos e Empréstimos'),
    ('Serviços de Utilidade'),
    ('Transporte'),
    ('Pessoal');

-- ============================================================
-- CATEGORIAS DE DESPESA
-- ============================================================
CREATE TABLE CategoriaDespesa (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    GrupoDespesaId  INT NOT NULL REFERENCES GrupoDespesa(Id),
    Nome            NVARCHAR(150) NOT NULL,
    Ativo           BIT DEFAULT 1
);

INSERT INTO CategoriaDespesa (GrupoDespesaId, Nome) VALUES
    -- Geral
    (1, 'Compra de Mercadoria'),
    (1, 'Academia'),
    (1, 'Estágio'),
    (1, 'Empréstimos'),
    (1, 'Cartões de Crédito'),
    (1, 'Apartamento'),
    (1, 'Presentes'),
    (1, 'Taxas Bancárias'),
    (1, 'Viagens'),
    (1, 'Isabel'),
    (1, 'Ofertas/Dízimos'),
    (1, 'Outros'),
    -- Empresa
    (2, 'Impostos (federais)'),
    (2, 'Sistema'),
    (2, 'Pessoal'),
    (2, 'Marketing'),
    (2, 'Despesas Diversas'),
    (2, 'Outros'),
    -- Acordos e Empréstimos
    (3, 'Neide'),
    (3, 'Carlos'),
    (3, 'Elias'),
    (3, 'Dias'),
    (3, 'Outros'),
    -- Serviços de Utilidade
    (4, 'Telefone'),
    (4, 'Internet'),
    (4, 'Luz'),
    (4, 'Gás'),
    (4, 'Água'),
    (4, 'Condomínio'),
    (4, 'Outros'),
    -- Transporte
    (5, 'Combustível'),
    (5, 'Prestação de Carro'),
    (5, 'Consertos'),
    (5, 'Emplacamento'),
    (5, 'Consórcio'),
    (5, 'Uber'),
    (5, 'Prestação de Moto'),
    (5, 'Outros'),
    -- Pessoal
    (6, 'Supermercado'),
    (6, 'Alimentação'),
    (6, 'Saúde'),
    (6, 'Manutenção / Reforma'),
    (6, 'Farmácia'),
    (6, 'Roupas'),
    (6, 'Cabelo / Beleza'),
    (6, 'Padaria'),
    (6, 'Feira'),
    (6, 'Cursos'),
    (6, 'Outros');

-- ============================================================
-- LANÇAMENTOS DE DESPESA
-- ============================================================
CREATE TABLE LancamentoDespesa (
    Id                  INT IDENTITY(1,1) PRIMARY KEY,
    AnoFiscalId         INT NOT NULL REFERENCES AnoFiscal(Id),
    CategoriaDespesaId  INT NOT NULL REFERENCES CategoriaDespesa(Id),
    Mes                 TINYINT NOT NULL CHECK (Mes BETWEEN 1 AND 12),
    Valor               DECIMAL(18,2) NOT NULL DEFAULT 0,
    Observacao          NVARCHAR(500) NULL,
    LancadoEm          DATETIME2 DEFAULT GETDATE(),
    AtualizadoEm       DATETIME2 NULL
);

-- ============================================================
-- VIEW: Resumo mensal (espelha a aba "Resumo" da planilha)
-- ============================================================
CREATE VIEW vw_ResumoMensal AS
SELECT
    af.Ano,
    m.Mes,
    ISNULL(r.TotalReceita, 0)   AS TotalReceita,
    ISNULL(d.TotalDespesa, 0)   AS TotalDespesa,
    ISNULL(r.TotalReceita, 0) - ISNULL(d.TotalDespesa, 0) AS SaldoMensal
FROM AnoFiscal af
CROSS JOIN (
    SELECT 1 AS Mes UNION SELECT 2 UNION SELECT 3  UNION SELECT 4
    UNION SELECT 5  UNION SELECT 6 UNION SELECT 7  UNION SELECT 8
    UNION SELECT 9  UNION SELECT 10 UNION SELECT 11 UNION SELECT 12
) m
LEFT JOIN (
    SELECT AnoFiscalId, Mes, SUM(Valor) AS TotalReceita
    FROM LancamentoReceita
    GROUP BY AnoFiscalId, Mes
) r ON r.AnoFiscalId = af.Id AND r.Mes = m.Mes
LEFT JOIN (
    SELECT AnoFiscalId, Mes, SUM(Valor) AS TotalDespesa
    FROM LancamentoDespesa
    GROUP BY AnoFiscalId, Mes
) d ON d.AnoFiscalId = af.Id AND d.Mes = m.Mes;

-- ============================================================
-- VIEW: Resumo por grupo de despesa
-- ============================================================
CREATE VIEW vw_DespesaPorGrupo AS
SELECT
    af.Ano,
    ld.Mes,
    gd.Nome AS Grupo,
    SUM(ld.Valor) AS Total
FROM LancamentoDespesa ld
INNER JOIN CategoriaDespesa cd ON cd.Id = ld.CategoriaDespesaId
INNER JOIN GrupoDespesa gd     ON gd.Id = cd.GrupoDespesaId
INNER JOIN AnoFiscal af        ON af.Id = ld.AnoFiscalId
GROUP BY af.Ano, ld.Mes, gd.Nome;
GO
