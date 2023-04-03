using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseServices.Migrations
{
    /// <inheritdoc />
    public partial class addcloudflareapi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CloudFlareApiTokenId",
                table: "Businesses",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CloudFlareApiToken",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AccessKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SecretKey = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CloudFlareApiToken", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_CloudFlareApiTokenId",
                table: "Businesses",
                column: "CloudFlareApiTokenId");

            migrationBuilder.AddForeignKey(
                name: "FK_Businesses_CloudFlareApiToken_CloudFlareApiTokenId",
                table: "Businesses",
                column: "CloudFlareApiTokenId",
                principalTable: "CloudFlareApiToken",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Businesses_CloudFlareApiToken_CloudFlareApiTokenId",
                table: "Businesses");

            migrationBuilder.DropTable(
                name: "CloudFlareApiToken");

            migrationBuilder.DropIndex(
                name: "IX_Businesses_CloudFlareApiTokenId",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "CloudFlareApiTokenId",
                table: "Businesses");
        }
    }
}
