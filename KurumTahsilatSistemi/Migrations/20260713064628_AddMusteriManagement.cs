using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KurumTahsilatSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddMusteriManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "musterileri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MusteriNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AdSoyad = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Tckn = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    VergiNo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Adres = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_musterileri", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_musterileri_MusteriNo",
                table: "musterileri",
                column: "MusteriNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_musterileri_Tckn",
                table: "musterileri",
                column: "Tckn",
                unique: true,
                filter: "[Tckn] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_musterileri_VergiNo",
                table: "musterileri",
                column: "VergiNo",
                unique: true,
                filter: "[VergiNo] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "musterileri");
        }
    }
}
