using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIs.Migrations.ATS
{
    /// <inheritdoc />
    public partial class AddAtsAuditTrailATSMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditTrail",
                schema: "ats",
                columns: table => new
                {
                    AuditEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Action = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Area = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Outcome = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FailureReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DurationMs = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UserFullName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AtsRoleId = table.Column<int>(type: "integer", nullable: true),
                    AtsClientId = table.Column<int>(type: "integer", nullable: true),
                    IsPlatformSuperAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TraceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Payload = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditTrail", x => x.AuditEntryId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrail_OccurredAt",
                schema: "ats",
                table: "AuditTrail",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrail_OccurredAt_AuditEntryId",
                schema: "ats",
                table: "AuditTrail",
                columns: new[] { "OccurredAt", "AuditEntryId" },
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditTrail",
                schema: "ats");
        }
    }
}
