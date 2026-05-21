using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Do_an_co_so.Migrations
{
    /// <inheritdoc />
    public partial class NangCapBaoCaoNguoiDung : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaoCaos_AspNetUsers_NguoiBaoCaoId",
                table: "BaoCaos");

            migrationBuilder.DropForeignKey(
                name: "FK_BaoCaos_PhongTro_PhongTroId",
                table: "BaoCaos");

            migrationBuilder.AlterColumn<int>(
                name: "PhongTroId",
                table: "BaoCaos",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "NguoiBiBaoCaoId",
                table: "BaoCaos",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaos_NguoiBiBaoCaoId",
                table: "BaoCaos",
                column: "NguoiBiBaoCaoId");

            migrationBuilder.AddForeignKey(
                name: "FK_BaoCaos_AspNetUsers_NguoiBaoCaoId",
                table: "BaoCaos",
                column: "NguoiBaoCaoId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BaoCaos_AspNetUsers_NguoiBiBaoCaoId",
                table: "BaoCaos",
                column: "NguoiBiBaoCaoId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BaoCaos_PhongTro_PhongTroId",
                table: "BaoCaos",
                column: "PhongTroId",
                principalTable: "PhongTro",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaoCaos_AspNetUsers_NguoiBaoCaoId",
                table: "BaoCaos");

            migrationBuilder.DropForeignKey(
                name: "FK_BaoCaos_AspNetUsers_NguoiBiBaoCaoId",
                table: "BaoCaos");

            migrationBuilder.DropForeignKey(
                name: "FK_BaoCaos_PhongTro_PhongTroId",
                table: "BaoCaos");

            migrationBuilder.DropIndex(
                name: "IX_BaoCaos_NguoiBiBaoCaoId",
                table: "BaoCaos");

            migrationBuilder.DropColumn(
                name: "NguoiBiBaoCaoId",
                table: "BaoCaos");

            migrationBuilder.AlterColumn<int>(
                name: "PhongTroId",
                table: "BaoCaos",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BaoCaos_AspNetUsers_NguoiBaoCaoId",
                table: "BaoCaos",
                column: "NguoiBaoCaoId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BaoCaos_PhongTro_PhongTroId",
                table: "BaoCaos",
                column: "PhongTroId",
                principalTable: "PhongTro",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
