using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorAndPatientRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "cc88a3ed-dcb5-5fdc-a543-ddd566778899", "bbbe2222-ccdd-5678-af99-555566667777", "Doctor", "DOCTOR" },
                    { "dd99b4fe-edc6-6ged-b654-eee677889900", "cccf3333-ddee-6789-ag00-666677778888", "Patient", "PATIENT" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "cc88a3ed-dcb5-5fdc-a543-ddd566778899");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "dd99b4fe-edc6-6ged-b654-eee677889900");
        }
    }
}
