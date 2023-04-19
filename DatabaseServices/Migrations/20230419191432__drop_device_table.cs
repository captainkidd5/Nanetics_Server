using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseServices.Migrations
{
    /// <inheritdoc />
    public partial class dropdevicetable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Devices");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CloudToDeviceMessageCount = table.Column<int>(type: "int", nullable: false),
                    ConnectionState = table.Column<int>(type: "int", nullable: false),
                    ConnectionStateUpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ETag = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GenerationId = table.Column<int>(type: "int", nullable: false),
                    HardwareId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    LastActivityTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StatusUpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    X509PrimaryThumbprint = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devices_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Devices_UserId",
                table: "Devices",
                column: "UserId");
        }
    }
}
