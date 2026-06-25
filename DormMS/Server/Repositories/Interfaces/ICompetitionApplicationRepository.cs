using DormMS.Server.Models;

namespace DormMS.Server.Repositories.Interfaces;

public interface ICompetitionApplicationRepository
{
    Task<IEnumerable<CompetitionApplication>> GetApplicationsByCompetitionIdAsync(int competitionId);
    Task<List<int>> GetApplicationIdsByCompetitionIdAsync(int competitionId);
    Task<IEnumerable<CompetitionApplication>> GetAllAsync();
    Task<IEnumerable<CompetitionApplication>> GetByStatusAsync(ApplicationStatus status);
    Task<CompetitionApplication?> GetApplicationByIdAsync(int id);
    Task<CompetitionApplication?> GetApplicationByStudentAndCompetitionAsync(int studentId, int competitionId);

    Task<(List<CompetitionApplication> Applications, int TotalCount)> GetPagedByCompetitionIdAsync(
    int competitionId, int pageNumber, int pageSize, ApplicationStatus? status);
    Task AddAsync(CompetitionApplication application);
    Task UpdateAsync(CompetitionApplication application);
    Task<bool> DeleteAsync(int id);
    Task<int> DeleteByCompetitionIdAsync(int competitionId);
}
