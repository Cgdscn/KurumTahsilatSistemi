using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KurumTahsilatSistemi.Migrations
{
    /// <inheritdoc />
    public partial class LastAddMusteriManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_musterileri",
                table: "musterileri");

            migrationBuilder.RenameTable(
                name: "musterileri",
                newName: "Musteriler");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Musteriler",
                newName: "Telefon");

            migrationBuilder.RenameIndex(
                name: "IX_musterileri_VergiNo",
                table: "Musteriler",
                newName: "IX_Musteriler_VergiNo");

            migrationBuilder.RenameIndex(
                name: "IX_musterileri_Tckn",
                table: "Musteriler",
                newName: "IX_Musteriler_Tckn");

            migrationBuilder.RenameIndex(
                name: "IX_musterileri_MusteriNo",
                table: "Musteriler",
                newName: "IX_Musteriler_MusteriNo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Musteriler",
                table: "Musteriler",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Musteriler",
                table: "Musteriler");

            migrationBuilder.RenameTable(
                name: "Musteriler",
                newName: "musterileri");

            migrationBuilder.RenameColumn(
                name: "Telefon",
                table: "musterileri",
                newName: "Phone");

            migrationBuilder.RenameIndex(
                name: "IX_Musteriler_VergiNo",
                table: "musterileri",
                newName: "IX_musterileri_VergiNo");

            migrationBuilder.RenameIndex(
                name: "IX_Musteriler_Tckn",
                table: "musterileri",
                newName: "IX_musterileri_Tckn");

            migrationBuilder.RenameIndex(
                name: "IX_Musteriler_MusteriNo",
                table: "musterileri",
                newName: "IX_musterileri_MusteriNo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_musterileri",
                table: "musterileri",
                column: "Id");
        }
    }
}
