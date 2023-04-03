using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseServices.Migrations
{
    /// <inheritdoc />
    public partial class adjustrotationmemberslist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessRotation");

            migrationBuilder.AddColumn<string>(
                name: "BusinessId",
                table: "Rotations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rotations_BusinessId",
                table: "Rotations",
                column: "BusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rotations_Businesses_BusinessId",
                table: "Rotations",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rotations_Businesses_BusinessId",
                table: "Rotations");

            migrationBuilder.DropIndex(
                name: "IX_Rotations_BusinessId",
                table: "Rotations");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Rotations");

            migrationBuilder.CreateTable(
                name: "BusinessRotation",
                columns: table => new
                {
                    ActiveRotationsId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BusinessesId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessRotation", x => new { x.ActiveRotationsId, x.BusinessesId });
                    table.ForeignKey(
                        name: "FK_BusinessRotation_Businesses_BusinessesId",
                        column: x => x.BusinessesId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BusinessRotation_Rotations_ActiveRotationsId",
                        column: x => x.ActiveRotationsId,
                        principalTable: "Rotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessRotation_BusinessesId",
                table: "BusinessRotation",
                column: "BusinessesId");
        }
    }
}
