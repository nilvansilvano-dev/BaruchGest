using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceiroAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnoFiscal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ano = table.Column<int>(type: "int", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnoFiscal", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cliente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ValorMensal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cliente", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GrupoDespesa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoDespesa", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GrupoReceita",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoReceita", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategoriaDespesa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GrupoDespesaId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriaDespesa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoriaDespesa_GrupoDespesa_GrupoDespesaId",
                        column: x => x.GrupoDespesaId,
                        principalTable: "GrupoDespesa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CategoriaReceita",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GrupoReceitaId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriaReceita", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoriaReceita_GrupoReceita_GrupoReceitaId",
                        column: x => x.GrupoReceitaId,
                        principalTable: "GrupoReceita",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LancamentoDespesa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnoFiscalId = table.Column<int>(type: "int", nullable: false),
                    CategoriaDespesaId = table.Column<int>(type: "int", nullable: false),
                    Mes = table.Column<int>(type: "int", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Observacao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LancadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LancamentoDespesa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LancamentoDespesa_AnoFiscal_AnoFiscalId",
                        column: x => x.AnoFiscalId,
                        principalTable: "AnoFiscal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LancamentoDespesa_CategoriaDespesa_CategoriaDespesaId",
                        column: x => x.CategoriaDespesaId,
                        principalTable: "CategoriaDespesa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LancamentoReceita",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnoFiscalId = table.Column<int>(type: "int", nullable: false),
                    CategoriaReceitaId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: true),
                    Mes = table.Column<int>(type: "int", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Observacao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LancadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LancamentoReceita", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LancamentoReceita_AnoFiscal_AnoFiscalId",
                        column: x => x.AnoFiscalId,
                        principalTable: "AnoFiscal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LancamentoReceita_CategoriaReceita_CategoriaReceitaId",
                        column: x => x.CategoriaReceitaId,
                        principalTable: "CategoriaReceita",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LancamentoReceita_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Cliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoriaDespesa_GrupoDespesaId",
                table: "CategoriaDespesa",
                column: "GrupoDespesaId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriaReceita_GrupoReceitaId",
                table: "CategoriaReceita",
                column: "GrupoReceitaId");

            migrationBuilder.CreateIndex(
                name: "IX_LancamentoDespesa_AnoFiscalId",
                table: "LancamentoDespesa",
                column: "AnoFiscalId");

            migrationBuilder.CreateIndex(
                name: "IX_LancamentoDespesa_CategoriaDespesaId",
                table: "LancamentoDespesa",
                column: "CategoriaDespesaId");

            migrationBuilder.CreateIndex(
                name: "IX_LancamentoReceita_AnoFiscalId",
                table: "LancamentoReceita",
                column: "AnoFiscalId");

            migrationBuilder.CreateIndex(
                name: "IX_LancamentoReceita_CategoriaReceitaId",
                table: "LancamentoReceita",
                column: "CategoriaReceitaId");

            migrationBuilder.CreateIndex(
                name: "IX_LancamentoReceita_ClienteId",
                table: "LancamentoReceita",
                column: "ClienteId");

            // ---- Dados iniciais ----

            migrationBuilder.Sql(@"
                INSERT INTO GrupoReceita (Nome) VALUES ('Entradas'), ('Outros');

                INSERT INTO CategoriaReceita (GrupoReceitaId, Nome) VALUES
                    (1, 'Clientes'),
                    (1, 'BO Financeiro - IA'),
                    (1, 'Estágio SMN'),
                    (1, 'Outros'),
                    (2, 'Transferência de poupança'),
                    (2, 'Renda de juros'),
                    (2, 'Restituições'),
                    (2, 'Saldo anterior');

                UPDATE CategoriaReceita SET Ativo = 0 WHERE Nome = 'Saldo anterior';

                INSERT INTO GrupoDespesa (Nome) VALUES
                    ('Geral'), ('Empresa'), ('Acordos e Empréstimos'),
                    ('Serviços de Utilidade'), ('Transporte'), ('Pessoal');

                INSERT INTO CategoriaDespesa (GrupoDespesaId, Nome) VALUES
                    (1,'Compra de Mercadoria'),(1,'Academia'),(1,'Estágio'),(1,'Empréstimos'),
                    (1,'Cartões de Crédito'),(1,'Apartamento'),(1,'Presentes'),(1,'Taxas Bancárias'),
                    (1,'Viagens'),(1,'Isabel'),(1,'Ofertas/Dízimos'),(1,'Outros'),
                    (2,'Impostos (federais)'),(2,'Sistema'),(2,'Pessoal'),(2,'Marketing'),
                    (2,'Despesas Diversas'),(2,'Outros'),
                    (3,'Neide'),(3,'Carlos'),(3,'Elias'),(3,'Dias'),(3,'Outros'),
                    (4,'Telefone'),(4,'Internet'),(4,'Luz'),(4,'Gás'),(4,'Água'),
                    (4,'Condomínio'),(4,'Outros'),
                    (5,'Combustível'),(5,'Prestação de Carro'),(5,'Consertos'),(5,'Emplacamento'),
                    (5,'Consórcio'),(5,'Uber'),(5,'Prestação de Moto'),(5,'Outros'),
                    (6,'Supermercado'),(6,'Alimentação'),(6,'Saúde'),(6,'Manutenção / Reforma'),
                    (6,'Farmácia'),(6,'Roupas'),(6,'Cabelo / Beleza'),(6,'Padaria'),
                    (6,'Feira'),(6,'Cursos'),(6,'Outros');
            ");

            // ---- Views ----

            migrationBuilder.Sql(@"
                CREATE VIEW vw_ResumoMensal AS
                SELECT
                    af.Ano,
                    m.Mes,
                    ISNULL(r.TotalReceita, 0) AS TotalReceita,
                    ISNULL(d.TotalDespesa, 0) AS TotalDespesa,
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
            ");

            migrationBuilder.Sql(@"
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
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_DespesaPorGrupo;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_ResumoMensal;");

            migrationBuilder.DropTable(
                name: "LancamentoDespesa");

            migrationBuilder.DropTable(
                name: "LancamentoReceita");

            migrationBuilder.DropTable(
                name: "CategoriaDespesa");

            migrationBuilder.DropTable(
                name: "AnoFiscal");

            migrationBuilder.DropTable(
                name: "CategoriaReceita");

            migrationBuilder.DropTable(
                name: "Cliente");

            migrationBuilder.DropTable(
                name: "GrupoDespesa");

            migrationBuilder.DropTable(
                name: "GrupoReceita");
        }
    }
}
