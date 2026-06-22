using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceiroAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioIdToCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "GrupoReceita",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "GrupoDespesa",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "CategoriaReceita",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "CategoriaDespesa",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrupoReceita_UsuarioId",
                table: "GrupoReceita",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_GrupoDespesa_UsuarioId",
                table: "GrupoDespesa",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriaReceita_UsuarioId",
                table: "CategoriaReceita",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriaDespesa_UsuarioId",
                table: "CategoriaDespesa",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_CategoriaDespesa_Usuario_UsuarioId",
                table: "CategoriaDespesa",
                column: "UsuarioId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CategoriaReceita_Usuario_UsuarioId",
                table: "CategoriaReceita",
                column: "UsuarioId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GrupoDespesa_Usuario_UsuarioId",
                table: "GrupoDespesa",
                column: "UsuarioId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GrupoReceita_Usuario_UsuarioId",
                table: "GrupoReceita",
                column: "UsuarioId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoriaDespesa_Usuario_UsuarioId",
                table: "CategoriaDespesa");

            migrationBuilder.DropForeignKey(
                name: "FK_CategoriaReceita_Usuario_UsuarioId",
                table: "CategoriaReceita");

            migrationBuilder.DropForeignKey(
                name: "FK_GrupoDespesa_Usuario_UsuarioId",
                table: "GrupoDespesa");

            migrationBuilder.DropForeignKey(
                name: "FK_GrupoReceita_Usuario_UsuarioId",
                table: "GrupoReceita");

            migrationBuilder.DropIndex(
                name: "IX_GrupoReceita_UsuarioId",
                table: "GrupoReceita");

            migrationBuilder.DropIndex(
                name: "IX_GrupoDespesa_UsuarioId",
                table: "GrupoDespesa");

            migrationBuilder.DropIndex(
                name: "IX_CategoriaReceita_UsuarioId",
                table: "CategoriaReceita");

            migrationBuilder.DropIndex(
                name: "IX_CategoriaDespesa_UsuarioId",
                table: "CategoriaDespesa");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "GrupoReceita");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "GrupoDespesa");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "CategoriaReceita");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "CategoriaDespesa");
        }
    }
}
