using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasterCMS.Migrations
{
    /// <inheritdoc />
    public partial class NameIndexing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Participants_FullName",
                table: "Participants",
                column: "FullName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Participants_FullName",
                table: "Participants");
        }
    }
}
