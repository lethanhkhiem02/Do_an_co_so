using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Do_an_co_so.Migrations
{
    /// <inheritdoc />
    public partial class CapNhatPhongTro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DienTich",
                table: "PhongTros");

            migrationBuilder.DropColumn(
                name: "TieuDe",
                table: "PhongTros");

            migrationBuilder.DropColumn(
                name: "TrangThai",
                table: "PhongTros");

            migrationBuilder.RenameColumn(
                name: "GiaPhong",
                table: "PhongTros",
                newName: "Gia");

            migrationBuilder.AlterColumn<string>(
                name: "HinhAnh",
                table: "PhongTros",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ChuTroId",
                table: "PhongTros",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhongTros_ChuTroId",
                table: "PhongTros",
                column: "ChuTroId");

            migrationBuilder.AddForeignKey(
                name: "FK_PhongTros_AspNetUsers_ChuTroId",
                table: "PhongTros",
                column: "ChuTroId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhongTros_AspNetUsers_ChuTroId",
                table: "PhongTros");

            migrationBuilder.DropIndex(
                name: "IX_PhongTros_ChuTroId",
                table: "PhongTros");

            migrationBuilder.DropColumn(
                name: "ChuTroId",
                table: "PhongTros");

            migrationBuilder.RenameColumn(
                name: "Gia",
                table: "PhongTros",
                newName: "GiaPhong");

            migrationBuilder.AlterColumn<string>(
                name: "HinhAnh",
                table: "PhongTros",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DienTich",
                table: "PhongTros",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "TieuDe",
                table: "PhongTros",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "TrangThai",
                table: "PhongTros",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
