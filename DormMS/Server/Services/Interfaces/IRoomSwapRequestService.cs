namespace DormMS.Server.Services.Interfaces;

using DormMS.Server.Models;

public interface IRoomSwapRequestService
{
    Task<RoomSwapRequestDto> CreateAsync(int senderId, CreateRoomSwapRequestDto dto);
    Task<RoomSwapRequestDto> RespondAsync(int receiverId, int requestId, RespondRoomSwapRequestDto dto);
    Task<List<RoomSwapRequestDto>> GetIncomingAsync(int studentId);
    Task<List<RoomSwapRequestDto>> GetOutgoingAsync(int studentId);
}

// public interface IRoomSwapRequestService
// {
//     Task<RoomSwapRequestDto> CreateAsync(int senderId, CreateRoomSwapRequestDto dto);
//     Task<RoomSwapRequestDto> RespondAsync(int receiverId, int requestId, RespondRoomSwapRequestDto dto);
//     Task<List<RoomSwapRequestDto>> GetIncomingAsync(int studentId);
//     Task<List<RoomSwapRequestDto>> GetOutgoingAsync(int studentId);
// }