using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIs.Migrations.CNX
{
    /// <inheritdoc />
    public partial class InitialCNXMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CNXTalkpushes",
                columns: table => new
                {
                    CandidateId = table.Column<string>(type: "text", nullable: false),
                    JobRequisitionId = table.Column<string>(type: "text", nullable: true),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    MiddleName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    UserPhoneNumber = table.Column<string>(type: "text", nullable: true),
                    MaritalStatus = table.Column<string>(type: "text", nullable: true),
                    PackageAccountName = table.Column<string>(type: "text", nullable: true),
                    CampaignTitle = table.Column<string>(type: "text", nullable: true),
                    Msa = table.Column<string>(type: "text", nullable: true),
                    JobRequisitionPrimaryLocation = table.Column<string>(type: "text", nullable: true),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    HireDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SchoolName = table.Column<string>(type: "text", nullable: true),
                    Education = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    PostalCode = table.Column<string>(type: "text", nullable: true),
                    AddressLine1 = table.Column<string>(type: "text", nullable: true),
                    SssNumber = table.Column<string>(type: "text", nullable: true),
                    ExtractedSssNumber = table.Column<string>(type: "text", nullable: true),
                    TinNumber = table.Column<string>(type: "text", nullable: true),
                    ExtractedTinNumber = table.Column<string>(type: "text", nullable: true),
                    PhilhealthNumber = table.Column<string>(type: "text", nullable: true),
                    ExtractedPhilhealthNumber = table.Column<string>(type: "text", nullable: true),
                    PagIbigNumber = table.Column<string>(type: "text", nullable: true),
                    ExtractedPagIbigNumber = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CNXTalkpushes", x => x.CandidateId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CNXTalkpushes");
        }
    }
}
