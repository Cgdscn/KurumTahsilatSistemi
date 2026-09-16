using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KurumTahsilatSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddMusteriBorcRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MusteriId",
                table: "Borclar",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Borclar_MusteriId",
                table: "Borclar",
                column: "MusteriId");

            migrationBuilder.AddForeignKey(
                name: "FK_Borclar_Musteriler_MusteriId",
                table: "Borclar",
                column: "MusteriId",
                principalTable: "Musteriler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Borclar_Musteriler_MusteriId",
                table: "Borclar");

            migrationBuilder.DropIndex(
                name: "IX_Borclar_MusteriId",
                table: "Borclar");

            migrationBuilder.DropColumn(
                name: "MusteriId",
                table: "Borclar");
        }
    }
}
