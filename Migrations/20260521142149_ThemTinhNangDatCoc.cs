using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Do_an_co_so.Migrations
{
    /// <inheritdoc />
    public partial class ThemTinhNangDatCoc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "HanDatCoc",
                table: "PhongTro",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguoiDatCocId",
                table: "PhongTro",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TienCoc",
                table: "PhongTro",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoaiHoaDon",
                table: "HoaDons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HanDatCoc",
                table: "PhongTro");

            migrationBuilder.DropColumn(
                name: "NguoiDatCocId",
                table: "PhongTro");

            migrationBuilder.DropColumn(
                name: "TienCoc",
                table: "PhongTro");

            migrationBuilder.DropColumn(
                name: "LoaiHoaDon",
                table: "HoaDons");
        }
    }
}
