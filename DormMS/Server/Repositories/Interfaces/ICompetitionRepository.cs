namespace DormMS.Server.Repositories.Interfaces;

public interface ICompetitionRepository
{
    Task<IEnumerable<Competition>> GetAllAsync();
    Task<Competition?> GetByIdAsync(int id);
    Task<Competition?> GetByIdTrackedAsync(int id);
    Task<IEnumerable<Competition>> GetOpenCompetitionsAsync();

   // Task<CompetitionGetDto?> OpenCompetitionAsync(int id);
    Task<IEnumerable<CompetitionApplication>> GetApplicationsByCompetitionIdAsync(int competitionId);
    Task<CompetitionApplication?> GetApplicationByIdAsync(int id);
    Task<CompetitionApplication?> GetApplicationByStudentAndCompetitionAsync(int studentId, int competitionId);
    Task AddAsync(Competition competition);
    Task AddApplicationAsync(CompetitionApplication application);
    Task UpdateAsync(Competition competition);
    Task UpdateApplicationAsync(CompetitionApplication application);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteApplicationAsync(int id);
}
