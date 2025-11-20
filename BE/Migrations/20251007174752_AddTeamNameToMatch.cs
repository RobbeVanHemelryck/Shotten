using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BE.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamNameToMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TeamName",
                table: "Matches",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeamName",
                table: "Matches");
        }
    }
}
