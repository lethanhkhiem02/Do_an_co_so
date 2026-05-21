using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Do_an_co_so.Migrations
{
    /// <inheritdoc />
    public partial class ThemChiTietPhongTro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ChieuDai",
                table: "PhongTro",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ChieuRong",
                table: "PhongTro",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CoBanCong",
                table: "PhongTro",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CoNhaVeSinh",
                table: "PhongTro",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChieuDai",
                table: "PhongTro");

            migrationBuilder.DropColumn(
                name: "ChieuRong",
                table: "PhongTro");

            migrationBuilder.DropColumn(
                name: "CoBanCong",
                table: "PhongTro");

            migrationBuilder.DropColumn(
                name: "CoNhaVeSinh",
                table: "PhongTro");
        }
    }
}
