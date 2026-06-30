using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class RefactorMedicalRecordPrescriptionTesting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_MedicalRecords_MedicalRecordID",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalRecords_Doctors_DoctorID",
                table: "MedicalRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalRecords_Patients_PatientID",
                table: "MedicalRecords");

            // Drop foreign key from Prescriptions to MedicalRecords first
            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_MedicalRecords_MedicalRecordId",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_MedicalRecordId",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_MedicalRecords_PatientID",
                table: "MedicalRecords");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_MedicalRecordID",
                table: "Appointments");

            // Drop primary keys before altering IDENTITY columns
            migrationBuilder.DropPrimaryKey(
                name: "PK_Prescriptions",
                table: "Prescriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MedicalRecords",
                table: "MedicalRecords");

            // Drop and recreate PrescriptionId column (from int IDENTITY to Guid)
            migrationBuilder.DropColumn(
                name: "PrescriptionID",
                table: "Prescriptions");

            migrationBuilder.AddColumn<Guid>(
                name: "PrescriptionId",
                table: "Prescriptions",
                type: "uniqueidentifier",
                nullable: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Prescriptions",
                table: "Prescriptions",
                column: "PrescriptionId");

            // Drop and recreate MedicalRecordId column in MedicalRecords (from int IDENTITY to Guid)
            migrationBuilder.DropColumn(
                name: "MedicalRecordID",
                table: "MedicalRecords");

            migrationBuilder.AddColumn<Guid>(
                name: "MedicalRecordId",
                table: "MedicalRecords",
                type: "uniqueidentifier",
                nullable: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MedicalRecords",
                table: "MedicalRecords",
                column: "MedicalRecordId");

            // Rename other columns
            migrationBuilder.RenameColumn(
                name: "PatientID",
                table: "MedicalRecords",
                newName: "PatientId");

            migrationBuilder.RenameColumn(
                name: "DoctorID",
                table: "MedicalRecords",
                newName: "DoctorId");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalRecords_DoctorID",
                table: "MedicalRecords",
                newName: "IX_MedicalRecords_DoctorId");

            migrationBuilder.RenameColumn(
                name: "MedicalRecordID",
                table: "Appointments",
                newName: "MedicalRecordId");

            migrationBuilder.AlterColumn<string>(
                name: "SpecialInstructions",
                table: "Prescriptions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            // Update MedicalRecordId in Prescriptions table (foreign key) - drop and recreate
            migrationBuilder.DropColumn(
                name: "MedicalRecordId",
                table: "Prescriptions");

            migrationBuilder.AddColumn<Guid>(
                name: "MedicalRecordId",
                table: "Prescriptions",
                type: "uniqueidentifier",
                nullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "AdditionalNotes",
                table: "MedicalRecords",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MedicalRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "MedicalRecords",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileType",
                table: "MedicalRecords",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            // Drop and recreate MedicalRecordId in Appointments table (from int to Guid)
            migrationBuilder.DropColumn(
                name: "MedicalRecordId",
                table: "Appointments");

            migrationBuilder.AddColumn<Guid>(
                name: "MedicalRecordId",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Testings",
                columns: table => new
                {
                    TestingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DateExam = table.Column<DateTime>(type: "date", nullable: false),
                    MedicalRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Testings", x => x.TestingId);
                    table.ForeignKey(
                        name: "FK_Testings_MedicalRecords_MedicalRecordId",
                        column: x => x.MedicalRecordId,
                        principalTable: "MedicalRecords",
                        principalColumn: "MedicalRecordId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_MedicalRecordId",
                table: "Prescriptions",
                column: "MedicalRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_PatientId",
                table: "MedicalRecords",
                column: "PatientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_MedicalRecordId",
                table: "Appointments",
                column: "MedicalRecordId",
                unique: true,
                filter: "[MedicalRecordId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Testings_MedicalRecordId",
                table: "Testings",
                column: "MedicalRecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_MedicalRecords_MedicalRecordId",
                table: "Appointments",
                column: "MedicalRecordId",
                principalTable: "MedicalRecords",
                principalColumn: "MedicalRecordId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalRecords_Doctors_DoctorId",
                table: "MedicalRecords",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalRecords_Patients_PatientId",
                table: "MedicalRecords",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_MedicalRecords_MedicalRecordId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalRecords_Doctors_DoctorId",
                table: "MedicalRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalRecords_Patients_PatientId",
                table: "MedicalRecords");

            migrationBuilder.DropTable(
                name: "Testings");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_MedicalRecordId",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_MedicalRecords_PatientId",
                table: "MedicalRecords");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_MedicalRecordId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "FileType",
                table: "MedicalRecords");

            migrationBuilder.RenameColumn(
                name: "PrescriptionId",
                table: "Prescriptions",
                newName: "PrescriptionID");

            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "MedicalRecords",
                newName: "PatientID");

            migrationBuilder.RenameColumn(
                name: "DoctorId",
                table: "MedicalRecords",
                newName: "DoctorID");

            migrationBuilder.RenameColumn(
                name: "MedicalRecordId",
                table: "MedicalRecords",
                newName: "MedicalRecordID");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalRecords_DoctorId",
                table: "MedicalRecords",
                newName: "IX_MedicalRecords_DoctorID");

            migrationBuilder.RenameColumn(
                name: "MedicalRecordId",
                table: "Appointments",
                newName: "MedicalRecordID");

            migrationBuilder.AlterColumn<string>(
                name: "SpecialInstructions",
                table: "Prescriptions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MedicalRecordId",
                table: "Prescriptions",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "PrescriptionID",
                table: "Prescriptions",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "AdditionalNotes",
                table: "MedicalRecords",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MedicalRecordID",
                table: "MedicalRecords",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "MedicalRecordID",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_MedicalRecordId",
                table: "Prescriptions",
                column: "MedicalRecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_PatientID",
                table: "MedicalRecords",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_MedicalRecordID",
                table: "Appointments",
                column: "MedicalRecordID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_MedicalRecords_MedicalRecordID",
                table: "Appointments",
                column: "MedicalRecordID",
                principalTable: "MedicalRecords",
                principalColumn: "MedicalRecordID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalRecords_Doctors_DoctorID",
                table: "MedicalRecords",
                column: "DoctorID",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalRecords_Patients_PatientID",
                table: "MedicalRecords",
                column: "PatientID",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
