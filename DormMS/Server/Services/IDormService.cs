public interface IDormService
{
    Task<IEnumerable<Dorm>> GetAllAsync();
    Task<Dorm?> GetByIdAsync(int id);
    Task<Dorm?> CreateAsync(DormCreateUpdateDto dto);
    Task<bool?> UpdateAsync(int id, DormCreateUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
