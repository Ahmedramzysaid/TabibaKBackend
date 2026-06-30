using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class SplitAppointmentDateAndTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Payments_PaymentID",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_DoctorID",
                table: "Appointments");

            migrationBuilder.RenameIndex(
                name: "IX_Appointments_PatientID",
                table: "Appointments",
                newName: "IX_Appointment_Patient");

            migrationBuilder.AddColumn<DateOnly>(
                name: "AppointmentDate",
                table: "Appointments",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "AppointmentTime",
                table: "Appointments",
                type: "time",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE Appointments
                SET AppointmentDate = CAST(AppointmentDateTime AS date),
                    AppointmentTime = CAST(AppointmentDateTime AS time)
                WHERE AppointmentDateTime IS NOT NULL
                """);

            migrationBuilder.DropColumn(
                name: "AppointmentDateTime",
                table: "Appointments");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "AppointmentDate",
                table: "Appointments",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "AppointmentTime",
                table: "Appointments",
                type: "time",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "ReminderSent",
                table: "Appointments",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "AdditionalNotes",
                table: "Appointments",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "AppointmentID",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_Doctor_DateTime",
                table: "Appointments",
                columns: new[] { "DoctorID", "AppointmentDate", "AppointmentTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_Status",
                table: "Appointments",
                column: "AppointmentStatus");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Payments_PaymentID",
                table: "Appointments",
                column: "PaymentID",
                principalTable: "Payments",
                principalColumn: "PaymentID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Payments_PaymentID",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointment_Doctor_DateTime",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointment_Status",
                table: "Appointments");

            migrationBuilder.AddColumn<DateTime>(
                name: "AppointmentDateTime",
                table: "Appointments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE Appointments
                SET AppointmentDateTime = CAST(AppointmentDate AS datetime2) + CAST(AppointmentTime AS datetime2)
                WHERE AppointmentDate IS NOT NULL AND AppointmentTime IS NOT NULL
                """);

            migrationBuilder.DropColumn(
                name: "AppointmentDate",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "AppointmentTime",
                table: "Appointments");

            migrationBuilder.RenameIndex(
                name: "IX_Appointment_Patient",
                table: "Appointments",
                newName: "IX_Appointments_PatientID");

            migrationBuilder.AlterColumn<bool>(
                name: "ReminderSent",
                table: "Appointments",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "AdditionalNotes",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "AppointmentID",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWID()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AppointmentDateTime",
                table: "Appointments",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorID",
                table: "Appointments",
                column: "DoctorID");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Payments_PaymentID",
                table: "Appointments",
                column: "PaymentID",
                principalTable: "Payments",
                principalColumn: "PaymentID");
        }
    }
}
