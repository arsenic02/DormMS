using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DormMS.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Surname = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dorms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    CurrentOccupancy = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dorms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DormitoryRooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoomNumber = table.Column<string>(type: "text", nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    CurrentOccupancy = table.Column<int>(type: "integer", nullable: false),
                    IsOccupied = table.Column<bool>(type: "boolean", nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DormId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DormitoryRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DormitoryRooms_Dorms_DormId",
                        column: x => x.DormId,
                        principalTable: "Dorms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DormManagers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Surname = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    DormId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DormManagers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DormManagers_Dorms_DormId",
                        column: x => x.DormId,
                        principalTable: "Dorms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Surname = table.Column<string>(type: "text", nullable: false),
                    HomeAddress = table.Column<string>(type: "text", nullable: false),
                    StudentIDNumber = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Age = table.Column<int>(type: "integer", nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    YearOfEnrollment = table.Column<int>(type: "integer", nullable: false),
                    StudyLevel = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FacultyId = table.Column<int>(type: "integer", nullable: false),
                    ESPB = table.Column<int>(type: "integer", nullable: false),
                    IsBudgetStudent = table.Column<bool>(type: "boolean", nullable: false),
                    DormitoryRoomId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_DormitoryRooms_DormitoryRoomId",
                        column: x => x.DormitoryRoomId,
                        principalTable: "DormitoryRooms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DormitoryRooms_DormId",
                table: "DormitoryRooms",
                column: "DormId");

            migrationBuilder.CreateIndex(
                name: "IX_DormManagers_DormId",
                table: "DormManagers",
                column: "DormId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_DormitoryRoomId",
                table: "Students",
                column: "DormitoryRoomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "DormManagers");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "DormitoryRooms");

            migrationBuilder.DropTable(
                name: "Dorms");
        }
    }
}
