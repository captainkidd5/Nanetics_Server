using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseServices.Migrations
{
    /// <inheritdoc />
    public partial class dropdevices1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_Groupings_GroupingId",
                table: "Devices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Devices",
                table: "Devices");

            migrationBuilder.RenameTable(
                name: "Devices",
                newName: "Device");

            migrationBuilder.RenameIndex(
                name: "IX_Devices_GroupingId",
                table: "Device",
                newName: "IX_Device_GroupingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Device",
                table: "Device",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Device_Groupings_GroupingId",
                table: "Device",
                column: "GroupingId",
                principalTable: "Groupings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Device_Groupings_GroupingId",
                table: "Device");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Device",
                table: "Device");

            migrationBuilder.RenameTable(
                name: "Device",
                newName: "Devices");

            migrationBuilder.RenameIndex(
                name: "IX_Device_GroupingId",
                table: "Devices",
                newName: "IX_Devices_GroupingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Devices",
                table: "Devices",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_Groupings_GroupingId",
                table: "Devices",
                column: "GroupingId",
                principalTable: "Groupings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
