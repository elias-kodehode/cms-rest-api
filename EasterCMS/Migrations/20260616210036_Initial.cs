using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EasterCMS.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Participants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Age = table.Column<int>(type: "integer", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Prizes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    InStock = table.Column<bool>(type: "boolean", nullable: false),
                    Collected = table.Column<bool>(type: "boolean", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    ParticipantId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prizes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prizes_Participants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participants",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Participants",
                columns: new[] { "Id", "Age", "City", "CreatedAt", "FullName", "UpdatedAt" },
                values: new object[] { new Guid("880595b4-1b81-40dc-8d41-5744284d8864"), 29, "Ulsteinvik", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Elias Sørensen", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "Prizes",
                columns: new[] { "Id", "Collected", "InStock", "Name", "ParticipantId", "Value" },
                values: new object[,]
                {
                    { new Guid("880595b4-1b81-40dc-8d41-5744284d1234"), false, true, "Vase", new Guid("880595b4-1b81-40dc-8d41-5744284d8864"), 1500.0 },
                    { new Guid("880595b4-1b81-40dc-8d41-5744284d1235"), false, true, "Chocolate", new Guid("880595b4-1b81-40dc-8d41-5744284d8864"), 50.0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prizes_ParticipantId",
                table: "Prizes",
                column: "ParticipantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Prizes");

            migrationBuilder.DropTable(
                name: "Participants");
        }
    }
}
