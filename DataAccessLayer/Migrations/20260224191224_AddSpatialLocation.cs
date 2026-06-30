using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddSpatialLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Point>(
                name: "Location",
                table: "AspNetUsers",
                type: "geography",
                nullable: true);

            // Note: No index on the geography column.
            // SQL Server spatial indexes require PK ≤ 895 bytes,
            // but Identity's nvarchar(450) Id = 900 bytes, exceeding the limit.
            // The column works correctly without an index; add one later if needed
            // after reducing the PK size (e.g. nvarchar(128)).
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Location",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "AspNetUsers");
        }
    }
}
