using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DormMS.Migrations
{
    /// <inheritdoc />
    public partial class Addmaxstudentstocompetitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "AverageGrade",
                table: "Students",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<int>(
                name: "MaxStudents",
                table: "Competitions",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxStudents",
                table: "Competitions");

            migrationBuilder.AlterColumn<decimal>(
                name: "AverageGrade",
                table: "Students",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);
        }
    }
}
