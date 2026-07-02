using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DormMS.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFinancialStatusVerificationStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VerificationStatus",
                table: "StudentFinancialStatuses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                "UPDATE \"StudentFinancialStatuses\" SET \"VerificationStatus\" = CASE WHEN \"IsVerified\" = true THEN 1 ELSE 0 END");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "StudentFinancialStatuses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                table: "StudentFinancialStatuses");

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "StudentFinancialStatuses",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
