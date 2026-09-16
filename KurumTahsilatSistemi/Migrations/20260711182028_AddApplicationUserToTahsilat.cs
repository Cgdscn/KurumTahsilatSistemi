using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KurumTahsilatSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationUserToTahsilat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Tahsilatlar",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tahsilatlar_ApplicationUserId",
                table: "Tahsilatlar",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tahsilatlar_AspNetUsers_ApplicationUserId",
                table: "Tahsilatlar",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tahsilatlar_AspNetUsers_ApplicationUserId",
                table: "Tahsilatlar");

            migrationBuilder.DropIndex(
                name: "IX_Tahsilatlar_ApplicationUserId",
                table: "Tahsilatlar");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Tahsilatlar");
        }
    }
}
