using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace APIs.Migrations.Auth
{
    /// <inheritdoc />
    public partial class RemoveUniqueKeyInAppRoleTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AuthUserAppRoles",
                table: "AuthUserAppRoles");

            migrationBuilder.AddColumn<int>(
                name: "AppRoleId",
                table: "AuthUserAppRoles",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuthUserAppRoles",
                table: "AuthUserAppRoles",
                column: "AppRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthUserAppRoles_UserId",
                table: "AuthUserAppRoles",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AuthUserAppRoles",
                table: "AuthUserAppRoles");

            migrationBuilder.DropIndex(
                name: "IX_AuthUserAppRoles_UserId",
                table: "AuthUserAppRoles");

            migrationBuilder.DropColumn(
                name: "AppRoleId",
                table: "AuthUserAppRoles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuthUserAppRoles",
                table: "AuthUserAppRoles",
                columns: new[] { "UserId", "AppId", "RoleId" });
        }
    }
}
