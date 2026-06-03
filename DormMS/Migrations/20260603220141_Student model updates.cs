using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DormMS.Migrations
{
    /// <inheritdoc />
    public partial class Studentmodelupdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsProfileComplete",
                table: "Students");

            migrationBuilder.AddColumn<string>(
                name: "JMBG",
                table: "Students",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MunicipalityOfResidence",
                table: "Students",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ProfileStatus",
                table: "Students",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Municipality",
                table: "Dorms",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "StudentFinancialStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    TotalFamilyIncome = table.Column<decimal>(type: "numeric", nullable: false),
                    FamilyMemberCount = table.Column<int>(type: "integer", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedByAdminId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentFinancialStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentFinancialStatus_Admins_VerifiedByAdminId",
                        column: x => x.VerifiedByAdminId,
                        principalTable: "Admins",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentFinancialStatus_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentFinancialStatus_StudentId",
                table: "StudentFinancialStatus",
                column: "StudentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentFinancialStatus_VerifiedByAdminId",
                table: "StudentFinancialStatus",
                column: "VerifiedByAdminId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentFinancialStatus");

            migrationBuilder.DropColumn(
                name: "JMBG",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "MunicipalityOfResidence",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ProfileStatus",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Municipality",
                table: "Dorms");

            migrationBuilder.AddColumn<bool>(
                name: "IsProfileComplete",
                table: "Students",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
