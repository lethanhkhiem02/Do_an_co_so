using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Do_an_co_so.Migrations
{
    /// <inheritdoc />
    public partial class AddLatLongToPhongTro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "PhongTro",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "PhongTro",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "PhongTro");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "PhongTro");
        }
    }
}
