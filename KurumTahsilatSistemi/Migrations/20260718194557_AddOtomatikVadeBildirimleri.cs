using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KurumTahsilatSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddOtomatikVadeBildirimleri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BildirimKodu",
                table: "Bildirimler",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BorcId",
                table: "Bildirimler",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bildirimler_ApplicationUserId_BildirimKodu",
                table: "Bildirimler",
                columns: new[] { "ApplicationUserId", "BildirimKodu" },
                unique: true,
                filter: "[BildirimKodu] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Bildirimler_BorcId",
                table: "Bildirimler",
                column: "BorcId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bildirimler_Borclar_BorcId",
                table: "Bildirimler",
                column: "BorcId",
                principalTable: "Borclar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bildirimler_Borclar_BorcId",
                table: "Bildirimler");

            migrationBuilder.DropIndex(
                name: "IX_Bildirimler_ApplicationUserId_BildirimKodu",
                table: "Bildirimler");

            migrationBuilder.DropIndex(
                name: "IX_Bildirimler_BorcId",
                table: "Bildirimler");

            migrationBuilder.DropColumn(
                name: "BildirimKodu",
                table: "Bildirimler");

            migrationBuilder.DropColumn(
                name: "BorcId",
                table: "Bildirimler");
        }
    }
}
