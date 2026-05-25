public interface IDormRepository
{
    Task<IEnumerable<Dorm>> GetAllAsync();
    Task<Dorm?> GetByIdAsync(int id);
    Task<Dorm?> GetByIdTrackedAsync(int id);
    Task AddAsync(Dorm dorm);
    Task UpdateAsync(Dorm dorm);
    Task<bool> DeleteAsync(int id);
}
