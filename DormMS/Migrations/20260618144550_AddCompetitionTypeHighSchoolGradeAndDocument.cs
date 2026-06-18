using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DormMS.Migrations
{
    /// <inheritdoc />
    public partial class AddCompetitionTypeHighSchoolGradeAndDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "HighSchoolAverageGrade",
                table: "Students",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Competitions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DocumentUrl",
                table: "CompetitionApplications",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HighSchoolAverageGrade",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Competitions");

            migrationBuilder.DropColumn(
                name: "DocumentUrl",
                table: "CompetitionApplications");
        }
    }
}
