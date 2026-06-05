using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DormMS.Migrations
{
    /// <inheritdoc />
    public partial class StudentFinancialstatusupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentFinancialStatuses_Admins_VerifiedByAdminId",
                table: "StudentFinancialStatuses");

            migrationBuilder.DropIndex(
                name: "IX_StudentFinancialStatuses_VerifiedByAdminId",
                table: "StudentFinancialStatuses");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "StudentFinancialStatuses");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "StudentFinancialStatuses");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                table: "StudentFinancialStatuses");

            migrationBuilder.DropColumn(
                name: "VerifiedByAdminId",
                table: "StudentFinancialStatuses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "StudentFinancialStatuses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "StudentFinancialStatuses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                table: "StudentFinancialStatuses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerifiedByAdminId",
                table: "StudentFinancialStatuses",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentFinancialStatuses_VerifiedByAdminId",
                table: "StudentFinancialStatuses",
                column: "VerifiedByAdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentFinancialStatuses_Admins_VerifiedByAdminId",
                table: "StudentFinancialStatuses",
                column: "VerifiedByAdminId",
                principalTable: "Admins",
                principalColumn: "Id");
        }
    }
}
