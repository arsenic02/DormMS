using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DormMS.Migrations
{
    /// <inheritdoc />
    public partial class Addrankingresults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RankingResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompetitionId = table.Column<int>(type: "integer", nullable: false),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    FacultyName = table.Column<string>(type: "text", nullable: false),
                    IsFreshman = table.Column<bool>(type: "boolean", nullable: false),
                    CalculatedPoints = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AverageGrade = table.Column<decimal>(type: "numeric", nullable: true),
                    ESPB = table.Column<int>(type: "integer", nullable: true),
                    YearsOfStudy = table.Column<int>(type: "integer", nullable: true),
                    HighSchoolAverageGrade = table.Column<decimal>(type: "numeric", nullable: true),
                    HighSchoolPoints = table.Column<decimal>(type: "numeric", nullable: true),
                    SocialBonus = table.Column<decimal>(type: "numeric", nullable: true),
                    TotalFamilyIncome = table.Column<decimal>(type: "numeric", nullable: false),
                    FamilyMemberCount = table.Column<int>(type: "integer", nullable: false),
                    IncomePerMember = table.Column<decimal>(type: "numeric", nullable: false),
                    RankedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankingResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RankingResults_Competitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalTable: "Competitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RankingResults_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RankingResults_CompetitionId",
                table: "RankingResults",
                column: "CompetitionId");

            migrationBuilder.CreateIndex(
                name: "IX_RankingResults_StudentId",
                table: "RankingResults",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RankingResults");
        }
    }
}
