using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KurumTahsilatSistemi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kurumlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KurumKodu = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    KurumAdi = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    HesapNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kurumlar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DosyaAktarimlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KurumId = table.Column<int>(type: "int", nullable: false),
                    DosyaAdi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    YuklemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToplamKayitSayisi = table.Column<int>(type: "int", nullable: false),
                    BasariliKayitSayisi = table.Column<int>(type: "int", nullable: false),
                    HataliKayitSayisi = table.Column<int>(type: "int", nullable: false),
                    Durum = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DosyaAktarimlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DosyaAktarimlari_Kurumlar_KurumId",
                        column: x => x.KurumId,
                        principalTable: "Kurumlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TahsilatUrunleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UrunKodu = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UrunAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    KurumId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TahsilatUrunleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TahsilatUrunleri_Kurumlar_KurumId",
                        column: x => x.KurumId,
                        principalTable: "Kurumlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Borclar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReferansNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MusteriNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MusteriAdi = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    BorcTutari = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SonOdemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Durum = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TahsilatUrunuId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Borclar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Borclar_TahsilatUrunleri_TahsilatUrunuId",
                        column: x => x.TahsilatUrunuId,
                        principalTable: "TahsilatUrunleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tahsilatlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IslemNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BorcId = table.Column<int>(type: "int", nullable: false),
                    OdenenTutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IslemTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OdemeKanali = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IslemDurumu = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tahsilatlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tahsilatlar_Borclar_BorcId",
                        column: x => x.BorcId,
                        principalTable: "Borclar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Borclar_ReferansNo",
                table: "Borclar",
                column: "ReferansNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Borclar_TahsilatUrunuId",
                table: "Borclar",
                column: "TahsilatUrunuId");

            migrationBuilder.CreateIndex(
                name: "IX_DosyaAktarimlari_KurumId",
                table: "DosyaAktarimlari",
                column: "KurumId");

            migrationBuilder.CreateIndex(
                name: "IX_Kurumlar_KurumKodu",
                table: "Kurumlar",
                column: "KurumKodu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tahsilatlar_BorcId",
                table: "Tahsilatlar",
                column: "BorcId");

            migrationBuilder.CreateIndex(
                name: "IX_TahsilatUrunleri_KurumId",
                table: "TahsilatUrunleri",
                column: "KurumId");

            migrationBuilder.CreateIndex(
                name: "IX_TahsilatUrunleri_UrunKodu",
                table: "TahsilatUrunleri",
                column: "UrunKodu",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DosyaAktarimlari");

            migrationBuilder.DropTable(
                name: "Tahsilatlar");

            migrationBuilder.DropTable(
                name: "Borclar");

            migrationBuilder.DropTable(
                name: "TahsilatUrunleri");

            migrationBuilder.DropTable(
                name: "Kurumlar");
        }
    }
}
