using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KurumTahsilatSistemi.Migrations
{
    /// <inheritdoc />
    public partial class addUserMusteriRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_MusteriId",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_MusteriId",
                table: "AspNetUsers",
                column: "MusteriId",
                unique: true,
                filter: "[MusteriId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_MusteriId",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_MusteriId",
                table: "AspNetUsers",
                column: "MusteriId");
        }
    }
}
