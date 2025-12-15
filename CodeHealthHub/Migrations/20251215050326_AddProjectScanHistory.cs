using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeHealthHub.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectScanHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Measures_ProjectMeasures_ProjectMeasuresId",
                table: "Measures");

            migrationBuilder.DropTable(
                name: "ProjectMeasures");

            migrationBuilder.RenameColumn(
                name: "ProjectMeasuresId",
                table: "Measures",
                newName: "ProjectScanId");

            migrationBuilder.RenameIndex(
                name: "IX_Measures_ProjectMeasuresId",
                table: "Measures",
                newName: "IX_Measures_ProjectScanId");

            migrationBuilder.CreateTable(
                name: "ProjectScans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SonarQubeProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    AnalysisDate = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectScans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectScans_SonarQubeProjects_SonarQubeProjectId",
                        column: x => x.SonarQubeProjectId,
                        principalTable: "SonarQubeProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectScans_SonarQubeProjectId",
                table: "ProjectScans",
                column: "SonarQubeProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Measures_ProjectScans_ProjectScanId",
                table: "Measures",
                column: "ProjectScanId",
                principalTable: "ProjectScans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Measures_ProjectScans_ProjectScanId",
                table: "Measures");

            migrationBuilder.DropTable(
                name: "ProjectScans");

            migrationBuilder.RenameColumn(
                name: "ProjectScanId",
                table: "Measures",
                newName: "ProjectMeasuresId");

            migrationBuilder.RenameIndex(
                name: "IX_Measures_ProjectScanId",
                table: "Measures",
                newName: "IX_Measures_ProjectMeasuresId");

            migrationBuilder.CreateTable(
                name: "ProjectMeasures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    LastAnalysisDate = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMeasures", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Measures_ProjectMeasures_ProjectMeasuresId",
                table: "Measures",
                column: "ProjectMeasuresId",
                principalTable: "ProjectMeasures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
