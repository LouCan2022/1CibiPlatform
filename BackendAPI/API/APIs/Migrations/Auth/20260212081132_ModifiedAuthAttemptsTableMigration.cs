using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIs.Migrations.Auth
{
    /// <inheritdoc />
    public partial class ModifiedAuthAttemptsTableMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "message",
                table: "AuthAttempts",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "attempts",
                table: "AuthAttempts",
                newName: "Attempts");

            migrationBuilder.RenameColumn(
                name: "userId",
                table: "AuthAttempts",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "createAt",
                table: "AuthAttempts",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "AuthAttempts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "AuthAttempts");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "AuthAttempts",
                newName: "message");

            migrationBuilder.RenameColumn(
                name: "Attempts",
                table: "AuthAttempts",
                newName: "attempts");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "AuthAttempts",
                newName: "userId");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "AuthAttempts",
                newName: "createAt");
        }
    }
}
