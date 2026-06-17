using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasterCMS.Migrations
{
    /// <inheritdoc />
    public partial class Seeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Prizes",
                keyColumn: "Id",
                keyValue: new Guid("880595b4-1b81-40dc-8d41-5744284d1235"),
                column: "ParticipantId",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Prizes",
                keyColumn: "Id",
                keyValue: new Guid("880595b4-1b81-40dc-8d41-5744284d1235"),
                column: "ParticipantId",
                value: new Guid("880595b4-1b81-40dc-8d41-5744284d8864"));
        }
    }
}
