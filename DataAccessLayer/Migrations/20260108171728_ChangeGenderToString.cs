using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class ChangeGenderToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add temporary column for string Gender
            migrationBuilder.AddColumn<string>(
                name: "Gender_Temp",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            // Convert existing boolean values to string
            migrationBuilder.Sql(@"
                UPDATE AspNetUsers 
                SET Gender_Temp = CASE 
                    WHEN Gender = 1 THEN 'Male'
                    WHEN Gender = 0 THEN 'Female'
                    ELSE 'Unknown'
                END
            ");

            // Make the column required after data migration
            migrationBuilder.AlterColumn<string>(
                name: "Gender_Temp",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            // Drop the old boolean column
            migrationBuilder.DropColumn(
                name: "Gender",
                table: "AspNetUsers");

            // Rename the temporary column to Gender
            migrationBuilder.RenameColumn(
                name: "Gender_Temp",
                table: "AspNetUsers",
                newName: "Gender");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Add temporary boolean column
            migrationBuilder.AddColumn<bool>(
                name: "Gender_Temp",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Convert string values back to boolean (only if column is string type)
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'AspNetUsers' 
                    AND COLUMN_NAME = 'Gender' 
                    AND DATA_TYPE = 'nvarchar'
                )
                BEGIN
                    UPDATE AspNetUsers 
                    SET Gender_Temp = CASE 
                        WHEN Gender = 'Male' THEN 1
                        WHEN Gender = 'Female' THEN 0
                        ELSE 0
                    END;
                END
            ");

            // Drop the string column
            migrationBuilder.DropColumn(
                name: "Gender",
                table: "AspNetUsers");

            // Rename the temporary column back to Gender
            migrationBuilder.RenameColumn(
                name: "Gender_Temp",
                table: "AspNetUsers",
                newName: "Gender");
        }
    }
}
