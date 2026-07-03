namespace DormMS.Server.Services.Interfaces;

using DormMS.Server.Models;

public interface IRoomSwapRequestService
{
    Task<RoomSwapRequestDto> CreateAsync(int senderId, CreateRoomSwapRequestDto dto);
    Task<RoomSwapRequestDto> RespondAsync(int receiverId, int requestId, RespondRoomSwapRequestDto dto);
    Task<List<RoomSwapRequestDto>> GetIncomingAsync(int studentId);
    Task<List<RoomSwapRequestDto>> GetOutgoingAsync(int studentId);
    Task<RoomSwapRequestDto> ApproveAsync(int managerUserId, int requestId);
    Task<RoomSwapRequestDto> RejectAsync(int managerUserId, int requestId);
    Task<List<RoomSwapRequestDto>> GetPendingForManagerAsync(int managerUserId);
}
