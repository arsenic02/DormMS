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
}
