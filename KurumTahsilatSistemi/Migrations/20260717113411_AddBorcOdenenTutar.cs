using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KurumTahsilatSistemi.Migrations
{
    /// <inheritdoc />
    public partial class AddBorcOdenenTutar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OdenenTutar",
                table: "Borclar",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OdenenTutar",
                table: "Borclar");
        }
    }
}
