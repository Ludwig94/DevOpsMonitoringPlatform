using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Monitoring.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MonitoringTargets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    MonitoringInterval = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoringTargets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MonitoringResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MonitoringTargetId = table.Column<int>(type: "int", nullable: false),
                    ResponseTime = table.Column<int>(type: "int", nullable: false),
                    StatusCode = table.Column<int>(type: "int", nullable: false),
                    IsHealthy = table.Column<bool>(type: "bit", nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoringResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonitoringResults_MonitoringTargets_MonitoringTargetId",
                        column: x => x.MonitoringTargetId,
                        principalTable: "MonitoringTargets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "MonitoringTargets",
                columns: new[] { "Id", "CreatedAt", "IsActive", "MonitoringInterval", "Name", "UpdatedAt", "Url" },
                values: new object[] { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, 60, "Google", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://www.google.com" });

            migrationBuilder.CreateIndex(
                name: "IX_MonitoringResults_CheckedAt",
                table: "MonitoringResults",
                column: "CheckedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MonitoringResults_MonitoringTargetId",
                table: "MonitoringResults",
                column: "MonitoringTargetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonitoringResults");

            migrationBuilder.DropTable(
                name: "MonitoringTargets");
        }
    }
}
