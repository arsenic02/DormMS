namespace DormMS.Server.Repositories.Interfaces;

public interface ICompetitionRepository
{
    Task<IEnumerable<Competition>> GetAllAsync();
    Task<Competition?> GetByIdAsync(int id);
    Task<Competition?> GetByIdTrackedAsync(int id);
    Task<IEnumerable<Competition>> GetOpenCompetitionsAsync();
    Task AddAsync(Competition competition);
    Task UpdateAsync(Competition competition);
    Task<bool> DeleteAsync(int id);
}
