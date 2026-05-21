using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Do_an_co_so.Migrations
{
    /// <inheritdoc />
    public partial class ThemBangBaoCao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BaoCaos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguoiBaoCaoId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PhongTroId = table.Column<int>(type: "int", nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChiTiet = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayBaoCao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaXuLy = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoCaos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaoCaos_AspNetUsers_NguoiBaoCaoId",
                        column: x => x.NguoiBaoCaoId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaoCaos_PhongTro_PhongTroId",
                        column: x => x.PhongTroId,
                        principalTable: "PhongTro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaos_NguoiBaoCaoId",
                table: "BaoCaos",
                column: "NguoiBaoCaoId");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaos_PhongTroId",
                table: "BaoCaos",
                column: "PhongTroId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaoCaos");
        }
    }
}
