using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace APIs.Migrations.Auth
{
    /// <inheritdoc />
    public partial class AddOtpandResetPasswordTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AuthUsers",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AuthUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 14, 0, 49, 10, 982, DateTimeKind.Utc).AddTicks(7223),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AssignedAt",
                table: "AuthUserAppRoles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 14, 0, 49, 10, 981, DateTimeKind.Utc).AddTicks(6473),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AuthSubmenu",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AuthSubmenu",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 14, 0, 49, 10, 919, DateTimeKind.Utc).AddTicks(7284),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AuthRoles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 14, 0, 49, 10, 978, DateTimeKind.Utc).AddTicks(5264),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AuthRefreshToken",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AuthRefreshToken",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 14, 0, 49, 10, 917, DateTimeKind.Utc).AddTicks(8519),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AuthApplications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 14, 0, 49, 10, 913, DateTimeKind.Utc).AddTicks(8322),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateTable(
                name: "OtpVerification",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OtpCodeHash = table.Column<string>(type: "text", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValue: new DateTime(2025, 10, 14, 0, 49, 10, 921, DateTimeKind.Utc).AddTicks(3137)),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpVerification", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetToken",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "text", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordResetToken_AuthUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AuthUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetToken_UserId",
                table: "PasswordResetToken",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OtpVerification");

            migrationBuilder.DropTable(
                name: "PasswordResetToken");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AuthUsers",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "AuthUsers",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2025, 10, 14, 0, 49, 10, 982, DateTimeKind.Utc).AddTicks(7223));

            migrationBuilder.AlterColumn<DateTime>(
                name: "AssignedAt",
                table: "AuthUserAppRoles",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2025, 10, 14, 0, 49, 10, 981, DateTimeKind.Utc).AddTicks(6473));

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AuthSubmenu",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AuthSubmenu",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2025, 10, 14, 0, 49, 10, 919, DateTimeKind.Utc).AddTicks(7284));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AuthRoles",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2025, 10, 14, 0, 49, 10, 978, DateTimeKind.Utc).AddTicks(5264));

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AuthRefreshToken",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AuthRefreshToken",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2025, 10, 14, 0, 49, 10, 917, DateTimeKind.Utc).AddTicks(8519));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AuthApplications",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2025, 10, 14, 0, 49, 10, 913, DateTimeKind.Utc).AddTicks(8322));
        }
    }
}
