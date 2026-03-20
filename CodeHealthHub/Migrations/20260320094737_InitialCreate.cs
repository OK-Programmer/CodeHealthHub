using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeHealthHub.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PieChartColours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CategoryName = table.Column<string>(type: "TEXT", nullable: true),
                    HexCode = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PieChartColours", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SonarQubeInstances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Scheme = table.Column<string>(type: "TEXT", nullable: false),
                    Host = table.Column<string>(type: "TEXT", nullable: false),
                    Port = table.Column<int>(type: "INTEGER", nullable: false),
                    AuthToken = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SonarQubeInstances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SonarQubeProjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    LastAnalysisDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Weight = table.Column<double>(type: "REAL", nullable: false),
                    NumOfDevelopers = table.Column<int>(type: "INTEGER", nullable: false),
                    DeveloperCostPerHour = table.Column<double>(type: "REAL", nullable: false),
                    SonarQubeInstanceId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SonarQubeProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SonarQubeProjects_SonarQubeInstances_SonarQubeInstanceId",
                        column: x => x.SonarQubeInstanceId,
                        principalTable: "SonarQubeInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectIssues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IssueKey = table.Column<string>(type: "TEXT", nullable: false),
                    Severity = table.Column<string>(type: "TEXT", nullable: false),
                    Project = table.Column<string>(type: "TEXT", nullable: false),
                    Effort = table.Column<int>(type: "INTEGER", nullable: true),
                    Debt = table.Column<int>(type: "INTEGER", nullable: true),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SonarQubeProjectId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectIssues_SonarQubeProjects_SonarQubeProjectId",
                        column: x => x.SonarQubeProjectId,
                        principalTable: "SonarQubeProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectScans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SonarQubeProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    AnalysisDate = table.Column<DateTime>(type: "TEXT", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Measures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Metric = table.Column<string>(type: "TEXT", nullable: true),
                    Value = table.Column<string>(type: "TEXT", nullable: true),
                    ProjectScanId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Measures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Measures_ProjectScans_ProjectScanId",
                        column: x => x.ProjectScanId,
                        principalTable: "ProjectScans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Measures_ProjectScanId",
                table: "Measures",
                column: "ProjectScanId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectIssues_SonarQubeProjectId",
                table: "ProjectIssues",
                column: "SonarQubeProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectScans_SonarQubeProjectId",
                table: "ProjectScans",
                column: "SonarQubeProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_SonarQubeProjects_SonarQubeInstanceId",
                table: "SonarQubeProjects",
                column: "SonarQubeInstanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Measures");

            migrationBuilder.DropTable(
                name: "PieChartColours");

            migrationBuilder.DropTable(
                name: "ProjectIssues");

            migrationBuilder.DropTable(
                name: "ProjectScans");

            migrationBuilder.DropTable(
                name: "SonarQubeProjects");

            migrationBuilder.DropTable(
                name: "SonarQubeInstances");
        }
    }
}
