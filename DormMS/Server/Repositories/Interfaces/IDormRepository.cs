using DormMS.Server.Models;

namespace DormMS.Server.Repositories.Interfaces;

public interface IDormRepository
{
    Task<IEnumerable<Dorm>> GetAllAsync();
    Task<Dorm?> GetByIdAsync(int id);
    Task<Dorm?> GetByIdTrackedAsync(int id);
    Task AddAsync(Dorm dorm);
    Task UpdateAsync(Dorm dorm);
    Task<bool> DeleteAsync(int id);
    Task<Dorm?> GetByIdWithRoomsAsync(int id);
    Task AddRoomsAsync(IEnumerable<DormitoryRoom> rooms);
    Task ClearStudentRoomAssignmentsAsync(List<int> roomIds);
    Task<int> DeleteRoomsByIdsAsync(List<int> roomIds);
}
