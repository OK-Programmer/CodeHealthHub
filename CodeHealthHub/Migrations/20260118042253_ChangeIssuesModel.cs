using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeHealthHub.Migrations
{
    /// <inheritdoc />
    public partial class ChangeIssuesModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Issues");

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
                name: "IssueImpact",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SoftwareQuality = table.Column<string>(type: "TEXT", nullable: false),
                    Severity = table.Column<string>(type: "TEXT", nullable: false),
                    ProjectIssueId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueImpact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueImpact_ProjectIssues_ProjectIssueId",
                        column: x => x.ProjectIssueId,
                        principalTable: "ProjectIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IssueImpact_ProjectIssueId",
                table: "IssueImpact",
                column: "ProjectIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectIssues_SonarQubeProjectId",
                table: "ProjectIssues",
                column: "SonarQubeProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IssueImpact");

            migrationBuilder.DropTable(
                name: "ProjectIssues");

            migrationBuilder.CreateTable(
                name: "Issues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SonarQubeProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    Debt = table.Column<string>(type: "TEXT", nullable: false),
                    Effort = table.Column<string>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Project = table.Column<string>(type: "TEXT", nullable: false),
                    Severity = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Issues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Issues_SonarQubeProjects_SonarQubeProjectId",
                        column: x => x.SonarQubeProjectId,
                        principalTable: "SonarQubeProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Issues_SonarQubeProjectId",
                table: "Issues",
                column: "SonarQubeProjectId");
        }
    }
}
