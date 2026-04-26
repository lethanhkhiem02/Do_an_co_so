using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Do_an_co_so.Migrations
{
    /// <inheritdoc />
    public partial class AddVipAndWallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVip",
                table: "PhongTro",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "SoDu",
                table: "AspNetUsers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVip",
                table: "PhongTro");

            migrationBuilder.DropColumn(
                name: "SoDu",
                table: "AspNetUsers");
        }
    }
}
