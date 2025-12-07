using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeHealthHub.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectDevVariables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DeveloperCostPerHour",
                table: "SonarQubeProjects",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "NumOfDevelopers",
                table: "SonarQubeProjects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeveloperCostPerHour",
                table: "SonarQubeProjects");

            migrationBuilder.DropColumn(
                name: "NumOfDevelopers",
                table: "SonarQubeProjects");
        }
    }
}
