using DormMS.Server.Models;
using DormMS.Server.DTOs;

namespace DormMS.Server.Services.Interfaces;

public record ApplyResult(CompetitionApplicationGetDto? Application, string? Error);

public interface ICompetitionApplicationService
{
    Task<ApplyResult> ApplyAsync(int userId, CompetitionApplicationCreateDto dto);
    Task<string> SaveDocumentAsync(IFormFile file);
    Task<IEnumerable<CompetitionApplicationGetDto>> GetApplicationsByCompetitionIdAsync(int competitionId);
    Task<IEnumerable<CompetitionApplicationGetDto>> GetAllAsync();
    Task<IEnumerable<CompetitionApplicationGetDto>> GetVerifiedApplicationsAsync();
    Task<CompetitionApplicationGetDto?> UpdateApplicationAsync(int id, CompetitionApplicationUpdateDto dto);
    Task<bool> DeleteApplicationAsync(int id);
    Task<int> DeleteApplicationsByCompetitionIdAsync(int competitionId);
    Task<CompetitionImportResultDto> BulkApplyAsync(BulkApplicationDto dto);
   // Task<CompetitionImportResultDto> BulkApplyByFreshmanStatusAsync(ApplyByFreshmanStatusDto dto);
    Task<BulkVerifyApplicationsResultDto> BulkVerifyApplicationsAsync(BulkVerifyApplicationsDto dto);
}

public class CompetitionImportResultDto
{
    public int TotalProcessed { get; set; }
    public int SuccessfullyApplied { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();
}
