using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseServices.Migrations
{
    /// <inheritdoc />
    public partial class addx509thumbprint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "X509PrimaryThumbprint",
                table: "Devices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "X509PrimaryThumbprint",
                table: "Devices");
        }
    }
}
