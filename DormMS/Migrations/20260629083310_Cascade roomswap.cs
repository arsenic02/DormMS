using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DormMS.Migrations
{
    /// <inheritdoc />
    public partial class Cascaderoomswap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoomSwapRequests_DormitoryRooms_ReceiverRoomId",
                table: "RoomSwapRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomSwapRequests_DormitoryRooms_SenderRoomId",
                table: "RoomSwapRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomSwapRequests_Students_ReceiverId",
                table: "RoomSwapRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomSwapRequests_Students_SenderId",
                table: "RoomSwapRequests");

            migrationBuilder.AddForeignKey(
                name: "FK_RoomSwapRequests_DormitoryRooms_ReceiverRoomId",
                table: "RoomSwapRequests",
                column: "ReceiverRoomId",
                principalTable: "DormitoryRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomSwapRequests_DormitoryRooms_SenderRoomId",
                table: "RoomSwapRequests",
                column: "SenderRoomId",
                principalTable: "DormitoryRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomSwapRequests_Students_ReceiverId",
                table: "RoomSwapRequests",
                column: "ReceiverId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomSwapRequests_Students_SenderId",
                table: "RoomSwapRequests",
                column: "SenderId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.DropForeignKey(
                name: "FK_RoomSwapRequests_Students_ReceiverId",
                table: "RoomSwapRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomSwapRequests_Students_SenderId",
                table: "RoomSwapRequests");

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

            migrationBuilder.AddForeignKey(
                name: "FK_RoomSwapRequests_Students_ReceiverId",
                table: "RoomSwapRequests",
                column: "ReceiverId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomSwapRequests_Students_SenderId",
                table: "RoomSwapRequests",
                column: "SenderId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
