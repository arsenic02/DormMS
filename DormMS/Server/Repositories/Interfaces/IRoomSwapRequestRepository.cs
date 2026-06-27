namespace DormMS.Server.Repositories.Interfaces;

using DormMS.Server.Models;

public interface IRoomSwapRequestRepository
{
    Task<RoomSwapRequest?> GetByIdAsync(int id);
    Task<RoomSwapRequest?> GetByIdTrackedAsync(int id);
    Task<List<RoomSwapRequest>> GetIncomingForStudentAsync(int studentId);
    Task<List<RoomSwapRequest>> GetOutgoingForStudentAsync(int studentId);
    Task<bool> HasPendingRequestBetweenAsync(int senderId, int receiverId);
    Task<bool> HasAnyPendingRequestInvolvingAsync(int studentId);
    Task AddAsync(RoomSwapRequest request);
    Task UpdateAsync(RoomSwapRequest request);
}

// public interface IRoomSwapRequestRepository
// {
//     Task<RoomSwapRequest?> GetByIdAsync(int id);
//     Task<RoomSwapRequest?> GetByIdTrackedAsync(int id);
//     Task<List<RoomSwapRequest>> GetIncomingForStudentAsync(int studentId);
//     Task<List<RoomSwapRequest>> GetOutgoingForStudentAsync(int studentId);
//     Task<bool> HasPendingRequestBetweenAsync(int senderId, int receiverId);
//     Task<bool> HasAnyPendingRequestInvolvingAsync(int studentId);
//     Task AddAsync(RoomSwapRequest request);
//     Task UpdateAsync(RoomSwapRequest request);
// }