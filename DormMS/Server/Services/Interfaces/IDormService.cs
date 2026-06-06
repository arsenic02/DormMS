namespace DormMS.Server.Services.Interfaces;

public interface IDormService
{
    Task<IEnumerable<DormGetDto>> GetAllAsync();
    Task<DormGetDto?> GetByIdAsync(int id);
    Task<Dorm?> CreateAsync(DormCreateUpdateDto dto);
    Task<bool?> UpdateAsync(int id, DormCreateUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
