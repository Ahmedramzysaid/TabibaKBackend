using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_Doctors_DoctorID",
                table: "Appointment");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_MedicalRecords_MedicalRecordID",
                table: "Appointment");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_Patients_PatientID",
                table: "Appointment");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_Payments_PaymentID",
                table: "Appointment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Appointment",
                table: "Appointment");

            migrationBuilder.RenameTable(
                name: "Appointment",
                newName: "Appointments");

            migrationBuilder.RenameIndex(
                name: "IX_Appointment_PaymentID",
                table: "Appointments",
                newName: "IX_Appointments_PaymentID");

            migrationBuilder.RenameIndex(
                name: "IX_Appointment_PatientID",
                table: "Appointments",
                newName: "IX_Appointments_PatientID");

            migrationBuilder.RenameIndex(
                name: "IX_Appointment_MedicalRecordID",
                table: "Appointments",
                newName: "IX_Appointments_MedicalRecordID");

            migrationBuilder.RenameIndex(
                name: "IX_Appointment_DoctorID",
                table: "Appointments",
                newName: "IX_Appointments_DoctorID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Appointments",
                table: "Appointments",
                column: "AppointmentID");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Doctors_DoctorID",
                table: "Appointments",
                column: "DoctorID",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_MedicalRecords_MedicalRecordID",
                table: "Appointments",
                column: "MedicalRecordID",
                principalTable: "MedicalRecords",
                principalColumn: "MedicalRecordID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Patients_PatientID",
                table: "Appointments",
                column: "PatientID",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Payments_PaymentID",
                table: "Appointments",
                column: "PaymentID",
                principalTable: "Payments",
                principalColumn: "PaymentID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Doctors_DoctorID",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_MedicalRecords_MedicalRecordID",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Patients_PatientID",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Payments_PaymentID",
                table: "Appointments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Appointments",
                table: "Appointments");

            migrationBuilder.RenameTable(
                name: "Appointments",
                newName: "Appointment");

            migrationBuilder.RenameIndex(
                name: "IX_Appointments_PaymentID",
                table: "Appointment",
                newName: "IX_Appointment_PaymentID");

            migrationBuilder.RenameIndex(
                name: "IX_Appointments_PatientID",
                table: "Appointment",
                newName: "IX_Appointment_PatientID");

            migrationBuilder.RenameIndex(
                name: "IX_Appointments_MedicalRecordID",
                table: "Appointment",
                newName: "IX_Appointment_MedicalRecordID");

            migrationBuilder.RenameIndex(
                name: "IX_Appointments_DoctorID",
                table: "Appointment",
                newName: "IX_Appointment_DoctorID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Appointment",
                table: "Appointment",
                column: "AppointmentID");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_Doctors_DoctorID",
                table: "Appointment",
                column: "DoctorID",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_MedicalRecords_MedicalRecordID",
                table: "Appointment",
                column: "MedicalRecordID",
                principalTable: "MedicalRecords",
                principalColumn: "MedicalRecordID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_Patients_PatientID",
                table: "Appointment",
                column: "PatientID",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_Payments_PaymentID",
                table: "Appointment",
                column: "PaymentID",
                principalTable: "Payments",
                principalColumn: "PaymentID");
        }
    }
}
