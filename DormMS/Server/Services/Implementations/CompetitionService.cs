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
        return competitions.Select(c => MapToGetDto(c));
    }
    public async Task<PagedResultDto<CompetitionGetDto>> GetPagedAsync(
    int pageNumber, int pageSize, CompetitionType? type, CompetitionStatus? status)
    {
        var (competitions, totalCount) = await _repository.GetPagedAsync(pageNumber, pageSize, type, status);

        return new PagedResultDto<CompetitionGetDto>
        {
            Items = competitions.Select(c => MapToGetDto(c)).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<CompetitionGetDto?> GetByIdAsync(int id)
    {
        var competition = await _repository.GetByIdAsync(id);
        return competition is null ? null : MapToGetDto(competition);
    }

    public async Task<IEnumerable<CompetitionGetDto>> GetOpenCompetitionsAsync(int studentId)
    {
        var competitions = await _repository.GetOpenCompetitionsAsync();
        return competitions.Select(c => MapToGetDto(c, studentId));
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

    public async Task<CreateCompetitionResult> CreateAsync(CompetitionCreateDto dto, int userId, string role)
    {
        int? adminId = null;
        int? dormManagerId = null;

        if (string.IsNullOrWhiteSpace(dto.Name))
            return new(null, "Naziv konkursa je obavezan");

        if (string.IsNullOrWhiteSpace(dto.Description))
            return new(null, "Opis konkursa je obavezan");

        if (dto.EndDate <= dto.StartDate)
            return new(null, "Datum završetka mora biti posle datuma početka");

        if (dto.MaxStudents.HasValue && dto.MaxStudents.Value <= 0)
            return new(null, "Maksimalan broj studenata mora biti veći od 0");

        if (dto.MaxStudents.HasValue)
        {
            var dorm = await _context.Dorms
                .Include(d => d.Rooms)
                .FirstOrDefaultAsync();

            if (dorm is null)
                return new(null, "Dom nije pronađen u sistemu");

            if (dto.MaxStudents.Value > dorm.AvailableSpots)
                return new(null, $"Maksimalan broj studenata ({dto.MaxStudents.Value}) premašuje slobodna mesta u domu ({dorm.AvailableSpots})");
        }

        if (role == "Admin")
        {
            adminId = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.AdminId)
                .FirstOrDefaultAsync();

            if (adminId is null)
                return new(null, "Admin nalog nije pronađen");
        }
        else if (role == "DormManager")
        {
            dormManagerId = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.DormManagerId)
                .FirstOrDefaultAsync();

            if (dormManagerId is null)
                return new(null, "DormManager nalog nije pronađen");
        }
        else
        {
            return new(null, "Nemate dozvolu za kreiranje konkursa");
        }

        var competition = new Competition
        {
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            StartDate = dto.StartDate.ToUniversalTime(),
            EndDate = dto.EndDate.ToUniversalTime(),
            Status = CompetitionStatus.Draft,
            Type = dto.Type,
            CreatedByAdminId = adminId,
            CreatedByDormManagerId = dormManagerId,
            MaxStudents = dto.MaxStudents
        };

        await _repository.AddAsync(competition);
        return new(MapToGetDto(competition), null);
    }

    public async Task<CompetitionGetDto?> UpdateAsync(int id, CompetitionUpdateDto dto)
    {
        var competition = await _repository.GetByIdTrackedAsync(id);
        if (competition is null)
            return null;

        if (dto.MaxStudents.HasValue)
        {
            var dorm = await _context.Dorms
                .Include(d => d.Rooms)
                .FirstOrDefaultAsync();

            if (dorm != null && dto.MaxStudents.Value > dorm.AvailableSpots)
                return null;
        }

        competition.Name = dto.Name.Trim();
        competition.Description = dto.Description.Trim();
        competition.StartDate = dto.StartDate;
        competition.EndDate = dto.EndDate;
        competition.Status = dto.Status;
        competition.Type = dto.Type;
        competition.MaxStudents = dto.MaxStudents;
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
        Type = competition.Type,
        CreatedAt = competition.CreatedAt,
        IsOpen = competition.IsOpen,
        CreatedByAdminId = competition.CreatedByAdminId,
        CreatedByDormManagerId = competition.CreatedByDormManagerId,
        ApplicationCount = competition.Applications.Count,
        MaxStudents = competition.MaxStudents,
        HasApplied = false
    };

    private static CompetitionGetDto MapToGetDto(Competition competition, int studentId)
    {
        var dto = MapToGetDto(competition);
        dto.HasApplied = competition.Applications.Any(a => a.StudentId == studentId);
        return dto;
    }
}
