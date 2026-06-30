using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyPrescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dosage",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "FrequencyPerDay",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "MedicationName",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Prescriptions");

            migrationBuilder.RenameColumn(
                name: "SpecialInstructions",
                table: "Prescriptions",
                newName: "FilePath");

            migrationBuilder.AddColumn<string>(
                name: "FileType",
                table: "Prescriptions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Prescriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Prescriptions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileType",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Prescriptions");

            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "Prescriptions",
                newName: "SpecialInstructions");

            migrationBuilder.AddColumn<string>(
                name: "Dosage",
                table: "Prescriptions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Prescriptions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<short>(
                name: "FrequencyPerDay",
                table: "Prescriptions",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<string>(
                name: "MedicationName",
                table: "Prescriptions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Prescriptions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
