using DormMS.Server.Models;
using DormMS.Server.DTOs;

namespace DormMS.Server.Services.Interfaces;

public interface ICompetitionService
{
    Task<IEnumerable<CompetitionGetDto>> GetAllAsync();
    Task<CompetitionGetDto?> GetByIdAsync(int id);
    Task<IEnumerable<CompetitionGetDto>> GetOpenCompetitionsAsync(int studentId);

    Task<CompetitionGetDto?> OpenCompetitionAsync(int id);

    Task<CompetitionGetDto?> ToggleCompetitionStatusAsync(int id);
    Task<CompetitionGetDto?> CreateAsync(CompetitionCreateDto dto, int userId, string role);
    Task<CompetitionGetDto?> UpdateAsync(int id, CompetitionUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
