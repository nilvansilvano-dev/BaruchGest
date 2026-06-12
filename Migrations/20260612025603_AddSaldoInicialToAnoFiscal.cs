using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceiroAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddSaldoInicialToAnoFiscal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SaldoInicial",
                table: "AnoFiscal",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SaldoInicial",
                table: "AnoFiscal");
        }
    }
}
