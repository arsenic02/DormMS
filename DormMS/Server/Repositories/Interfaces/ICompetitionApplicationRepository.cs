namespace DormMS.Server.Repositories.Interfaces;

public interface ICompetitionApplicationRepository
{
    Task<IEnumerable<CompetitionApplication>> GetApplicationsByCompetitionIdAsync(int competitionId);
    Task<CompetitionApplication?> GetApplicationByIdAsync(int id);
    Task<CompetitionApplication?> GetApplicationByStudentAndCompetitionAsync(int studentId, int competitionId);
    Task AddAsync(CompetitionApplication application);
    Task UpdateAsync(CompetitionApplication application);
    Task<bool> DeleteAsync(int id);
}
