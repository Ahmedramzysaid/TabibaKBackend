using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddPrescriptionItemsDigitalPrescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DigitalPrescriptions",
                columns: table => new
                {
                    DigitalPrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalPrescriptions", x => x.DigitalPrescriptionId);
                    table.ForeignKey(
                        name: "FK_DigitalPrescriptions_MedicalRecords_MedicalRecordId",
                        column: x => x.MedicalRecordId,
                        principalTable: "MedicalRecords",
                        principalColumn: "MedicalRecordId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DigitalPrescriptionItems",
                columns: table => new
                {
                    DigitalPrescriptionItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DigitalPrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicineName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PerDay = table.Column<int>(type: "int", nullable: true),
                    Frequency = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DurationHowLong = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalPrescriptionItems", x => x.DigitalPrescriptionItemId);
                    table.ForeignKey(
                        name: "FK_DigitalPrescriptionItems_DigitalPrescriptions_DigitalPrescriptionId",
                        column: x => x.DigitalPrescriptionId,
                        principalTable: "DigitalPrescriptions",
                        principalColumn: "DigitalPrescriptionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DigitalPrescriptionItems_DigitalPrescriptionId",
                table: "DigitalPrescriptionItems",
                column: "DigitalPrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalPrescriptions_MedicalRecordId",
                table: "DigitalPrescriptions",
                column: "MedicalRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DigitalPrescriptionItems");
            migrationBuilder.DropTable(name: "DigitalPrescriptions");
        }
    }
}
