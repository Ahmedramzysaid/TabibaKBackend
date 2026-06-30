using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class DigitalPrescriptionItemSpotlightsRemoveFrequencyDuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Spotlights",
                table: "DigitalPrescriptionItems",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.DropColumn(
                name: "DurationHowLong",
                table: "DigitalPrescriptionItems");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "DigitalPrescriptionItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Spotlights",
                table: "DigitalPrescriptionItems");

            migrationBuilder.AddColumn<string>(
                name: "DurationHowLong",
                table: "DigitalPrescriptionItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "DigitalPrescriptionItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
