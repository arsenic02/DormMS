namespace DormMS.Server.Repositories.Interfaces;

using DormMS.Server.Models;

public interface IMoveOutRequestRepository
{
    Task<MoveOutRequest?> GetByIdAsync(int id);
    Task<MoveOutRequest?> GetByIdTrackedAsync(int id);
    Task<MoveOutRequest?> GetPendingForStudentAsync(int studentId);
    Task<MoveOutRequest?> GetLatestForStudentAsync(int studentId);
    Task<List<MoveOutRequest>> GetPendingForDormAsync(int dormId);
    Task AddAsync(MoveOutRequest request);
    Task UpdateAsync(MoveOutRequest request);
}
