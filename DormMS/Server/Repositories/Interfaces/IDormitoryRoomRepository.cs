using DormMS.Server.Models;

namespace DormMS.Server.Repositories.Interfaces;

public interface IDormitoryRoomRepository
{
    Task<IEnumerable<DormitoryRoom>> GetAllAsync();
    Task<DormitoryRoom?> GetByIdAsync(int id);
    Task<DormitoryRoom?> GetByIdTrackedAsync(int id);
    Task AddAsync(DormitoryRoom room);
    Task UpdateAsync(DormitoryRoom room);
    Task<bool> DeleteAsync(int id);
    Task<bool> DormExistsAsync(int dormId);
    Task<bool> RoomNumberExistsAsync(int dormId, string roomNumber, int? excludeId = null);
    Task<(List<DormitoryRoom> Rooms, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
    Task<List<Student>> GetStudentsByRoomIdsAsync(List<int> roomIds);
    Task<Dictionary<int, string>> GetDormNamesByIdsAsync(List<int> dormIds);
}
