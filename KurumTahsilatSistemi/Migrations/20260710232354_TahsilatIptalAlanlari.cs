using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KurumTahsilatSistemi.Migrations
{
    /// <inheritdoc />
    public partial class TahsilatIptalAlanlari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IptalNedeni",
                table: "Tahsilatlar",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IptalTarihi",
                table: "Tahsilatlar",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IptalNedeni",
                table: "Tahsilatlar");

            migrationBuilder.DropColumn(
                name: "IptalTarihi",
                table: "Tahsilatlar");
        }
    }
}
