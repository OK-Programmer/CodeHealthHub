using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeHealthHub.Migrations
{
    /// <inheritdoc />
    public partial class LinkInstanceToProjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SonarQubeInstanceId",
                table: "SonarQubeProjects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SonarQubeProjects_SonarQubeInstanceId",
                table: "SonarQubeProjects",
                column: "SonarQubeInstanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_SonarQubeProjects_SonarQubeInstances_SonarQubeInstanceId",
                table: "SonarQubeProjects",
                column: "SonarQubeInstanceId",
                principalTable: "SonarQubeInstances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SonarQubeProjects_SonarQubeInstances_SonarQubeInstanceId",
                table: "SonarQubeProjects");

            migrationBuilder.DropIndex(
                name: "IX_SonarQubeProjects_SonarQubeInstanceId",
                table: "SonarQubeProjects");

            migrationBuilder.DropColumn(
                name: "SonarQubeInstanceId",
                table: "SonarQubeProjects");
        }
    }
}
