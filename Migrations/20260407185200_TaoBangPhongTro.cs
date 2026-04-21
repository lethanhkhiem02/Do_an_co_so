using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Do_an_co_so.Migrations
{
    /// <inheritdoc />
    public partial class TaoBangPhongTro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhongTros_AspNetUsers_ChuTroId",
                table: "PhongTros");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PhongTros",
                table: "PhongTros");

            migrationBuilder.RenameTable(
                name: "PhongTros",
                newName: "PhongTro");

            migrationBuilder.RenameIndex(
                name: "IX_PhongTros_ChuTroId",
                table: "PhongTro",
                newName: "IX_PhongTro_ChuTroId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PhongTro",
                table: "PhongTro",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PhongTro_AspNetUsers_ChuTroId",
                table: "PhongTro",
                column: "ChuTroId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhongTro_AspNetUsers_ChuTroId",
                table: "PhongTro");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PhongTro",
                table: "PhongTro");

            migrationBuilder.RenameTable(
                name: "PhongTro",
                newName: "PhongTros");

            migrationBuilder.RenameIndex(
                name: "IX_PhongTro_ChuTroId",
                table: "PhongTros",
                newName: "IX_PhongTros_ChuTroId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PhongTros",
                table: "PhongTros",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PhongTros_AspNetUsers_ChuTroId",
                table: "PhongTros",
                column: "ChuTroId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
