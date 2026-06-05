using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DormMS.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentFinancialStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentFinancialStatus_Admins_VerifiedByAdminId",
                table: "StudentFinancialStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentFinancialStatus_Students_StudentId",
                table: "StudentFinancialStatus");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentFinancialStatus",
                table: "StudentFinancialStatus");

            migrationBuilder.RenameTable(
                name: "StudentFinancialStatus",
                newName: "StudentFinancialStatuses");

            migrationBuilder.RenameIndex(
                name: "IX_StudentFinancialStatus_VerifiedByAdminId",
                table: "StudentFinancialStatuses",
                newName: "IX_StudentFinancialStatuses_VerifiedByAdminId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentFinancialStatus_StudentId",
                table: "StudentFinancialStatuses",
                newName: "IX_StudentFinancialStatuses_StudentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentFinancialStatuses",
                table: "StudentFinancialStatuses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentFinancialStatuses_Admins_VerifiedByAdminId",
                table: "StudentFinancialStatuses",
                column: "VerifiedByAdminId",
                principalTable: "Admins",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentFinancialStatuses_Students_StudentId",
                table: "StudentFinancialStatuses",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentFinancialStatuses_Admins_VerifiedByAdminId",
                table: "StudentFinancialStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentFinancialStatuses_Students_StudentId",
                table: "StudentFinancialStatuses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentFinancialStatuses",
                table: "StudentFinancialStatuses");

            migrationBuilder.RenameTable(
                name: "StudentFinancialStatuses",
                newName: "StudentFinancialStatus");

            migrationBuilder.RenameIndex(
                name: "IX_StudentFinancialStatuses_VerifiedByAdminId",
                table: "StudentFinancialStatus",
                newName: "IX_StudentFinancialStatus_VerifiedByAdminId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentFinancialStatuses_StudentId",
                table: "StudentFinancialStatus",
                newName: "IX_StudentFinancialStatus_StudentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentFinancialStatus",
                table: "StudentFinancialStatus",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentFinancialStatus_Admins_VerifiedByAdminId",
                table: "StudentFinancialStatus",
                column: "VerifiedByAdminId",
                principalTable: "Admins",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentFinancialStatus_Students_StudentId",
                table: "StudentFinancialStatus",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
