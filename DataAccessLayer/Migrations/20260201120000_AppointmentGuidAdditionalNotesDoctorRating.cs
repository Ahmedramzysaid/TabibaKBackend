using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AppointmentGuidAdditionalNotesDoctorRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Add AdditionalNotes to Appointment
            migrationBuilder.AddColumn<string>(
                name: "AdditionalNotes",
                table: "Appointments",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            // 2) Change AppointmentID from int to uniqueidentifier
            migrationBuilder.AddColumn<Guid>(
                name: "AppointmentIdNew",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("UPDATE Appointments SET AppointmentIdNew = NEWID();");

            migrationBuilder.AlterColumn<Guid>(
                name: "AppointmentIdNew",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.DropPrimaryKey(
                name: "PK_Appointments",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "AppointmentID",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "AppointmentIdNew",
                table: "Appointments",
                newName: "AppointmentID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Appointments",
                table: "Appointments",
                column: "AppointmentID");

            // 3) Doctor: Rating and RatingCount
            migrationBuilder.AddColumn<decimal>(
                name: "Rating",
                table: "Doctors",
                type: "decimal(3,2)",
                precision: 3,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RatingCount",
                table: "Doctors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // 4) DoctorRating table
            migrationBuilder.CreateTable(
                name: "DoctorRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppointmentID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorID = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    PatientID = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Rating = table.Column<byte>(type: "tinyint", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorRatings_Appointments_AppointmentID",
                        column: x => x.AppointmentID,
                        principalTable: "Appointments",
                        principalColumn: "AppointmentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DoctorRatings_Doctors_DoctorID",
                        column: x => x.DoctorID,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DoctorRatings_Patients_PatientID",
                        column: x => x.PatientID,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorRatings_AppointmentID",
                table: "DoctorRatings",
                column: "AppointmentID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorRatings_DoctorID",
                table: "DoctorRatings",
                column: "DoctorID");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorRatings_PatientID",
                table: "DoctorRatings",
                column: "PatientID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoctorRatings");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "RatingCount",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "AdditionalNotes",
                table: "Appointments");

            // Reverting AppointmentID to int would lose data; PK change is not reverted in Down.
        }
    }
}
