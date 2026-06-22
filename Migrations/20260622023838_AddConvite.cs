using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceiroAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddConvite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Convite",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ContadorId = table.Column<int>(type: "int", nullable: false),
                    EmailConvidado = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Usado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ExpiraEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Convite", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Convite_Usuario_ContadorId",
                        column: x => x.ContadorId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Convite_ContadorId",
                table: "Convite",
                column: "ContadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Convite_Token",
                table: "Convite",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Convite");
        }
    }
}
