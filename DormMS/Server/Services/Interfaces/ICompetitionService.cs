using DormMS.Server.Models;
using DormMS.Server.DTOs;

namespace DormMS.Server.Services.Interfaces;

public interface ICompetitionService
{
    Task<IEnumerable<CompetitionGetDto>> GetAllAsync();
    Task<PagedResultDto<CompetitionGetDto>> GetPagedAsync(
    int pageNumber, int pageSize, CompetitionType? type, CompetitionStatus? status);
    Task<CompetitionGetDto?> GetByIdAsync(int id);
    Task<IEnumerable<CompetitionGetDto>> GetOpenCompetitionsAsync(int studentId, bool isFreshman);

    //Task<CompetitionGetDto?> OpenCompetitionAsync(int id);

    Task<CompetitionGetDto?> ToggleCompetitionStatusAsync(int id);

   
    //Task<CompetitionGetDto?> CreateAsync(CompetitionCreateDto dto, int userId, string role);
    Task<CreateCompetitionResult> CreateAsync(CompetitionCreateDto dto, int userId, string role);
    Task<CompetitionGetDto?> UpdateAsync(int id, CompetitionUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}

 public record CreateCompetitionResult(CompetitionGetDto? Competition, string? Error);