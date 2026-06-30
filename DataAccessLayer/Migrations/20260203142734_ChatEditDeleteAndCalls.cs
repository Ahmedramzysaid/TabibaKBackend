using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class ChatEditDeleteAndCalls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EditedAtUtc",
                table: "Messages",
                type: "datetime2(2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Calls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CallerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CalleeId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2(2)", nullable: false),
                    EndedAtUtc = table.Column<DateTime>(type: "datetime2(2)", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Calls_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Calls_ConversationId_StartedAtUtc",
                table: "Calls",
                columns: new[] { "ConversationId", "StartedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Calls");

            migrationBuilder.DropColumn(
                name: "EditedAtUtc",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Messages");
        }
    }
}
