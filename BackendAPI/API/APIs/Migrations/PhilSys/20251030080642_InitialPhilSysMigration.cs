using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace APIs.Migrations.PhilSys
{
    /// <inheritdoc />
    public partial class InitialPhilSysMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PhilSysTransactions",
                columns: table => new
                {
                    Tid = table.Column<Guid>(type: "uuid", nullable: false),
                    InquiryType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MiddleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Suffix = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    BirthDate = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    PCN = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    FaceLivenessSessionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WebHookUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsTransacted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    HashToken = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedLivenessIdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    TransactedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhilSysTransactions", x => x.Tid);
                });

            migrationBuilder.CreateTable(
                name: "PhilSysTransactionResults",
                columns: table => new
                {
                    Trid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    idv_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    verified = table.Column<bool>(type: "boolean", nullable: false),
                    data_subject_digital_id = table.Column<string>(type: "text", nullable: true),
                    data_subject_national_id_number = table.Column<string>(type: "text", nullable: true),
                    data_subject_face_image_url = table.Column<string>(type: "text", nullable: true),
                    data_subject_full_name = table.Column<string>(type: "text", nullable: true),
                    data_subject_first_name = table.Column<string>(type: "text", nullable: true),
                    data_subject_middle_name = table.Column<string>(type: "text", nullable: true),
                    data_subject_last_name = table.Column<string>(type: "text", nullable: true),
                    data_subject_suffix = table.Column<string>(type: "text", nullable: true),
                    data_subject_gender = table.Column<string>(type: "text", nullable: true),
                    data_subject_marital_status = table.Column<string>(type: "text", nullable: true),
                    data_subject_birth_date = table.Column<string>(type: "text", nullable: true),
                    data_subject_email = table.Column<string>(type: "text", nullable: true),
                    data_subject_mobile_number = table.Column<string>(type: "text", nullable: true),
                    data_subject_blood_type = table.Column<string>(type: "text", nullable: true),
                    data_subject_address_permanent = table.Column<string>(type: "text", nullable: true),
                    data_subject_address_present = table.Column<string>(type: "text", nullable: true),
                    data_subject_place_of_birth_full = table.Column<string>(type: "text", nullable: true),
                    data_subject_place_of_birth_municipality = table.Column<string>(type: "text", nullable: true),
                    data_subject_place_of_birth_province = table.Column<string>(type: "text", nullable: true),
                    data_subject_place_of_birth_country = table.Column<string>(type: "text", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true),
                    message = table.Column<string>(type: "text", nullable: true),
                    error_description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhilSysTransactionResults", x => x.Trid);
                    table.ForeignKey(
                        name: "FK_PhilSysTransactionResults_PhilSysTransactions_idv_session_id",
                        column: x => x.idv_session_id,
                        principalTable: "PhilSysTransactions",
                        principalColumn: "Tid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhilSysTransactionResults_idv_session_id",
                table: "PhilSysTransactionResults",
                column: "idv_session_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhilSysTransactionResults");

            migrationBuilder.DropTable(
                name: "PhilSysTransactions");
        }
    }
}
