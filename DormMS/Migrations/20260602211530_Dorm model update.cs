using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DormMS.Migrations
{
    /// <inheritdoc />
    public partial class Dormmodelupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "Dorms");

            migrationBuilder.DropColumn(
                name: "CurrentOccupancy",
                table: "Dorms");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "Dorms",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentOccupancy",
                table: "Dorms",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
