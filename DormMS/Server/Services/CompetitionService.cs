using Microsoft.EntityFrameworkCore;

public class CompetitionService : ICompetitionService
{
    private readonly ICompetitionRepository _repository;
    private readonly AppDbContext _context;

    public CompetitionService(ICompetitionRepository repository, AppDbContext context)
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

    public async Task<CompetitionApplicationGetDto?> ApplyAsync(int studentId, CompetitionApplicationCreateDto dto)
    {
        var competition = await _repository.GetByIdAsync(dto.CompetitionId);
        if (competition is null)
            return null;

        if (!competition.IsOpen)
            return null;

        var existingApplication = await _repository.GetApplicationByStudentAndCompetitionAsync(studentId, dto.CompetitionId);
        if (existingApplication is not null)
            return null;

        var application = new CompetitionApplication
        {
            StudentId = studentId,
            CompetitionId = dto.CompetitionId,
            Notes = dto?.Notes,
            Status = ApplicationStatus.Pending
        };

        await _repository.AddApplicationAsync(application);
        return MapToApplicationGetDto(application);
    }

    public async Task<IEnumerable<CompetitionApplicationGetDto>> GetApplicationsByCompetitionIdAsync(int competitionId)
    {
        var applications = await _repository.GetApplicationsByCompetitionIdAsync(competitionId);
        return applications.Select(MapToApplicationGetDto);
    }

    public async Task<CompetitionApplicationGetDto?> UpdateApplicationAsync(int id, CompetitionApplicationUpdateDto dto)
    {
        var application = await _context.CompetitionApplications
            .Include(a => a.Student)
            .Include(a => a.Competition)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application is null)
            return null;

        application.Status = dto.Status;
        application.PriorityScore = dto.PriorityScore;
        application.Notes = dto.Notes ?? application.Notes;

        await _repository.UpdateApplicationAsync(application);
        return MapToApplicationGetDto(application);
    }

    public async Task<bool> DeleteApplicationAsync(int id)
    {
        return await _repository.DeleteApplicationAsync(id);
    }

    public async Task<CompetitionImportResultDto> BulkApplyAsync(BulkApplicationDto dto)
    {
        var result = new CompetitionImportResultDto();
        var competition = await _repository.GetByIdAsync(dto.CompetitionId);

        if (competition is null)
        {
            result.Errors.Add("Competition not found");
            return result;
        }

        if (!competition.IsOpen)
        {
            result.Errors.Add("Competition is not open for applications");
            return result;
        }

        result.TotalProcessed = dto.StudentIds.Count;

        foreach (var studentId in dto.StudentIds)
        {
            try
            {
                var existingApplication = await _repository.GetApplicationByStudentAndCompetitionAsync(studentId, dto.CompetitionId);
                if (existingApplication is not null)
                {
                    result.Errors.Add($"Student {studentId} has already applied to this competition");
                    result.Failed++;
                    continue;
                }

                var application = new CompetitionApplication
                {
                    StudentId = studentId,
                    CompetitionId = dto.CompetitionId,
                    Status = ApplicationStatus.Pending
                };

                await _repository.AddApplicationAsync(application);
                result.SuccessfullyApplied++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error applying student {studentId}: {ex.Message}");
                result.Failed++;
            }
        }

        return result;
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

    private static CompetitionApplicationGetDto MapToApplicationGetDto(CompetitionApplication application) => new()
    {
        Id = application.Id,
        StudentId = application.StudentId,
        StudentName = application.Student.Name,
        StudentSurname = application.Student.Surname,
        StudentEmail = application.Student.Email,
        StudentIDNumber = application.Student.StudentIDNumber,
        CompetitionId = application.CompetitionId,
        CompetitionName = application.Competition.Name,
        AppliedAt = application.AppliedAt,
        Status = application.Status,
        PriorityScore = application.PriorityScore,
        Notes = application.Notes
    };
}
