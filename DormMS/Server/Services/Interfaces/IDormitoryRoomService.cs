using DormMS.Server.Models;
using DormMS.Server.DTOs;

namespace DormMS.Server.Services.Interfaces;

public interface IDormitoryRoomService
{
    Task<IEnumerable<DormitoryRoom>> GetAllAsync();
    Task<DormitoryRoom?> GetByIdAsync(int id);
    Task<DormitoryRoom?> CreateAsync(DormitoryRoomCreateUpdateDto dto);
    Task<bool?> UpdateAsync(int id, DormitoryRoomCreateUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
