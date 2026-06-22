using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceiroAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioIdMultiTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Cliente",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "AnoFiscal",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_UsuarioId",
                table: "Cliente",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_AnoFiscal_UsuarioId",
                table: "AnoFiscal",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_AnoFiscal_Usuario_UsuarioId",
                table: "AnoFiscal",
                column: "UsuarioId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cliente_Usuario_UsuarioId",
                table: "Cliente",
                column: "UsuarioId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnoFiscal_Usuario_UsuarioId",
                table: "AnoFiscal");

            migrationBuilder.DropForeignKey(
                name: "FK_Cliente_Usuario_UsuarioId",
                table: "Cliente");

            migrationBuilder.DropIndex(
                name: "IX_Cliente_UsuarioId",
                table: "Cliente");

            migrationBuilder.DropIndex(
                name: "IX_AnoFiscal_UsuarioId",
                table: "AnoFiscal");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Cliente");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "AnoFiscal");
        }
    }
}
