using Microsoft.EntityFrameworkCore;
using DormMS.Server.Services.Interfaces;
using DormMS.Server.Repositories.Interfaces;
using DormMS.Server.Models;
using DormMS.Server.DTOs;

namespace DormMS.Server.Services.Implementations;

public class CompetitionService : ICompetitionService
{
    private readonly ICompetitionRepository _repository;
    private readonly AppDbContext _context;

    public CompetitionService(
        ICompetitionRepository repository,
        AppDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<IEnumerable<CompetitionGetDto>> GetAllAsync()
    {
        var competitions = await _repository.GetAllAsync();
        return competitions.Select(MapToGetDto);
    }

    public async Task<CompetitionGetDto?> GetByIdAsync(int id)
    {
        var competition = await _repository.GetByIdAsync(id);
        return competition is null ? null : MapToGetDto(competition);
    }

    public async Task<IEnumerable<CompetitionGetDto>> GetOpenCompetitionsAsync()
    {
        var competitions = await _repository.GetOpenCompetitionsAsync();
        return competitions.Select(MapToGetDto);
    }

    public async Task<CompetitionGetDto?> OpenCompetitionAsync(int id)
    {
        var competition = await _repository.GetByIdTrackedAsync(id);
        if (competition is null)
            return null;

        if (competition.Status != CompetitionStatus.Draft)
            return null;

        competition.Status = CompetitionStatus.Open;
        await _repository.UpdateAsync(competition);
        return MapToGetDto(competition);
    }

    public async Task<CompetitionGetDto?> ToggleCompetitionStatusAsync(int id)
    {
        var competition = await _repository.GetByIdTrackedAsync(id);
        if (competition is null)
            return null;

        competition.Status = competition.Status switch
        {
            CompetitionStatus.Draft => CompetitionStatus.Open,
            CompetitionStatus.Open => CompetitionStatus.Closed,
            CompetitionStatus.Closed => CompetitionStatus.Open,
            _ => competition.Status
        };

        await _repository.UpdateAsync(competition);
        return MapToGetDto(competition);
    }
    public async Task<CompetitionGetDto?> CreateAsync(CompetitionCreateDto dto, int userId, string role)
    {
        int? adminId = null;
        int? dormManagerId = null;

        if (role == "Admin")
        {
            adminId = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.AdminId)
                .FirstOrDefaultAsync();

            if (adminId is null)
                return null;
        }
        else if (role == "DormManager")
        {
            dormManagerId = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.DormManagerId)
                .FirstOrDefaultAsync();

            if (dormManagerId is null)
                return null;
        }

        var competition = new Competition
        {
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            StartDate = dto.StartDate.ToUniversalTime(),
            EndDate = dto.EndDate.ToUniversalTime(),
            Status = CompetitionStatus.Draft,
            CreatedByAdminId = adminId,
            CreatedByDormManagerId = dormManagerId
        };

        await _repository.AddAsync(competition);
        return MapToGetDto(competition);
    }

    // public async Task<CompetitionGetDto?> CreateAsync(CompetitionCreateDto dto, int? adminId = null, int? dormManagerId = null)
    // {
    //     if (adminId is null && dormManagerId is null)
    //         return null;

    //     var competition = new Competition
    //     {
    //         Name = dto.Name.Trim(),
    //         Description = dto.Description.Trim(),
    //         StartDate = dto.StartDate.ToUniversalTime(),
    //         EndDate = dto.EndDate.ToUniversalTime(),
    //         Status = CompetitionStatus.Draft,
    //         CreatedByAdminId = adminId,
    //         CreatedByDormManagerId = dormManagerId
    //     };

    //     await _repository.AddAsync(competition);
    //     return MapToGetDto(competition);
    // }

    public async Task<CompetitionGetDto?> UpdateAsync(int id, CompetitionUpdateDto dto)
    {
        var competition = await _repository.GetByIdTrackedAsync(id);
        if (competition is null)
            return null;

        competition.Name = dto.Name.Trim();
        competition.Description = dto.Description.Trim();
        competition.StartDate = dto.StartDate;
        competition.EndDate = dto.EndDate;
        competition.Status = dto.Status;

        await _repository.UpdateAsync(competition);
        return MapToGetDto(competition);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    private static CompetitionGetDto MapToGetDto(Competition competition) => new()
    {
        Id = competition.Id,
        Name = competition.Name,
        Description = competition.Description,
        StartDate = competition.StartDate,
        EndDate = competition.EndDate,
        Status = competition.Status,
        CreatedAt = competition.CreatedAt,
        IsOpen = competition.IsOpen,
        CreatedByAdminId = competition.CreatedByAdminId,
        CreatedByDormManagerId = competition.CreatedByDormManagerId,
        ApplicationCount = competition.Applications.Count
    };
}
