using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeHealthHub.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectWeight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Weight",
                table: "SonarQubeProjects",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Weight",
                table: "SonarQubeProjects");
        }
    }
}
