using DormMS.Server.Models;

namespace DormMS.Server.Repositories.Interfaces;

public interface ICompetitionRepository
{
    Task<IEnumerable<Competition>> GetAllAsync();
    Task<(List<Competition> Competitions, int TotalCount)> GetPagedAsync(
    int pageNumber, int pageSize, CompetitionType? type, CompetitionStatus? status);
    Task<Competition?> GetByIdAsync(int id);
    Task<Competition?> GetByIdTrackedAsync(int id);
    Task<IEnumerable<Competition>> GetOpenCompetitionsAsync();
    Task AddAsync(Competition competition);
    Task UpdateAsync(Competition competition);
    Task<bool> DeleteAsync(int id);
}
