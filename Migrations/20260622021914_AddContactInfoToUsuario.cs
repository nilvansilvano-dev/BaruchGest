using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceiroAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddContactInfoToUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Crc",
                table: "Usuario",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Documento",
                table: "Usuario",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Endereco",
                table: "Usuario",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefone",
                table: "Usuario",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoDocumento",
                table: "Usuario",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Crc",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "Documento",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "Endereco",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "Telefone",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "TipoDocumento",
                table: "Usuario");
        }
    }
}
