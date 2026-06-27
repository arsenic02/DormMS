using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DormMS.Migrations
{
    /// <inheritdoc />
    public partial class SwapRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReceiverRoomId",
                table: "RoomSwapRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SenderRoomId",
                table: "RoomSwapRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<int>(type: "integer", nullable: true),
                    DormId = table.Column<int>(type: "integer", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Dorms_DormId",
                        column: x => x.DormId,
                        principalTable: "Dorms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoomSwapRequests_ReceiverRoomId",
                table: "RoomSwapRequests",
                column: "ReceiverRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomSwapRequests_SenderRoomId",
                table: "RoomSwapRequests",
                column: "SenderRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_DormId",
                table: "Notifications",
                column: "DormId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_StudentId",
                table: "Notifications",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoomSwapRequests_DormitoryRooms_ReceiverRoomId",
                table: "RoomSwapRequests",
                column: "ReceiverRoomId",
                principalTable: "DormitoryRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomSwapRequests_DormitoryRooms_SenderRoomId",
                table: "RoomSwapRequests",
                column: "SenderRoomId",
                principalTable: "DormitoryRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoomSwapRequests_DormitoryRooms_ReceiverRoomId",
                table: "RoomSwapRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomSwapRequests_DormitoryRooms_SenderRoomId",
                table: "RoomSwapRequests");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_RoomSwapRequests_ReceiverRoomId",
                table: "RoomSwapRequests");

            migrationBuilder.DropIndex(
                name: "IX_RoomSwapRequests_SenderRoomId",
                table: "RoomSwapRequests");

            migrationBuilder.DropColumn(
                name: "ReceiverRoomId",
                table: "RoomSwapRequests");

            migrationBuilder.DropColumn(
                name: "SenderRoomId",
                table: "RoomSwapRequests");
        }
    }
}
