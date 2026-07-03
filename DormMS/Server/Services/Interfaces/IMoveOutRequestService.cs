namespace DormMS.Server.Services.Interfaces;

using DormMS.Server.Models;

public interface IMoveOutRequestService
{
    Task<MoveOutRequestDto> CreateAsync(int studentUserId);
    Task<MoveOutRequestDto?> GetMyRequestAsync(int studentUserId);
    Task<MoveOutRequestDto?> GetMyCompletedRequestAsync(int studentUserId);
    Task<MoveOutRequestDto> ApproveAsync(int managerUserId, int requestId);
    Task<MoveOutRequestDto> RejectAsync(int managerUserId, int requestId);
    Task<List<MoveOutRequestDto>> GetPendingForManagerAsync(int managerUserId);
}
