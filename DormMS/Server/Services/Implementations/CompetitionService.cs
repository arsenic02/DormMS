using Microsoft.EntityFrameworkCore;
using DormMS.Server.Services.Interfaces;
using DormMS.Server.Repositories.Interfaces;

namespace DormMS.Server.Services.Implementations;

public class CompetitionService : ICompetitionService
{
    private readonly ICompetitionRepository _repository;
    private readonly AppDbContext _context;
    private readonly IStudentRepository _studentRepository;
    private readonly IDormRepository _dormitoryRepository;

    public CompetitionService(
        ICompetitionRepository repository,
        AppDbContext context,
        IStudentRepository studentRepository,
        IDormRepository dormitoryRepository)
    {
        _repository = repository;
        _context = context;
        _studentRepository = studentRepository;
        _dormitoryRepository = dormitoryRepository;
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

    public async Task<ApplyResult> ApplyAsync(int userId, CompetitionApplicationCreateDto dto)
    {
        var studentId = await _context.Users
        .Where(u => u.Id == userId)
        .Select(u => u.StudentId)
        .FirstOrDefaultAsync();

        if (studentId is null)
            return new(null, "Student not found");

        var competition = await _repository.GetByIdAsync(dto.CompetitionId);
        if (competition is null)
            return new(null, "Competition not found");

        if (!competition.IsOpen)
            return new(null, "Competition is not open");

        var student = await _studentRepository.GetByIdWithFinancialStatusAsync((int)studentId); //bilo je studentId
        if (student is null)
            return new(null, "Student not found");

        if (!student.IsEligibleToApply)
            return new(null, "Student is not eligible to apply");

        // Provera opstine
        if (competition.CreatedByDormManagerId.HasValue)
        {
            var dormManager = await _context.DormManagers
                .Include(dm => dm.Dorm)
                .FirstOrDefaultAsync(dm => dm.Id == competition.CreatedByDormManagerId.Value);

            if (dormManager?.Dorm != null &&
                student.MunicipalityOfResidence.Equals(
                    dormManager.Dorm.Municipality, StringComparison.OrdinalIgnoreCase))
                return new(null, "Student municipality matches dormitory municipality");
        }

        var existingApplication = await _repository
            .GetApplicationByStudentAndCompetitionAsync(studentId.Value, dto.CompetitionId);
        if (existingApplication is not null)
            return new(null, "Student has already applied to this competition");

        var application = new CompetitionApplication
        {
            StudentId = studentId.Value,
            CompetitionId = dto.CompetitionId,
            Notes = dto.Notes,
            Status = ApplicationStatus.Pending,
            AppliedAt = DateTime.UtcNow
        };

        await _repository.AddApplicationAsync(application);
        var full = await _repository.GetApplicationByIdAsync(application.Id);
        return new(MapToApplicationGetDto(full!), null);
    }
    // public async Task<CompetitionApplicationGetDto?> ApplyAsync(int studentId, CompetitionApplicationCreateDto dto)
    // {
    //     var competition = await _repository.GetByIdAsync(dto.CompetitionId);
    //     if (competition is null || !competition.IsOpen)
    //         return null;

    //     var student = await _studentRepository.GetByIdWithFinancialStatusAsync(studentId);
    //     if (student is null)
    //         return null;

    //     // Provera da li student ispunjava uslove
    //     if (!student.IsEligibleToApply)
    //         return null;

    //     // Provera opštine — ne sme biti ista kao opština doma
    //     Dorm? dormitory = null;
    //     if (competition.CreatedByDormManagerId.HasValue)
    //     {
    //         var dormManager = await _context.DormManagers
    //             .Include(dm => dm.Dorm)
    //             .FirstOrDefaultAsync(dm => dm.Id == competition.CreatedByDormManagerId.Value);
    //         dormitory = dormManager?.Dorm;
    //     }

    //     if (dormitory != null &&
    //         student.MunicipalityOfResidence.Equals(dormitory.Municipality, StringComparison.OrdinalIgnoreCase))
    //         return null;

    //     var existingApplication = await _repository.GetApplicationByStudentAndCompetitionAsync(studentId, dto.CompetitionId);
    //     if (existingApplication is not null)
    //         return null;

    //     var application = new CompetitionApplication
    //     {
    //         StudentId = studentId,
    //         CompetitionId = dto.CompetitionId,
    //         Notes = dto.Notes,
    //         Status = ApplicationStatus.Pending,
    //         AppliedAt = DateTime.UtcNow
    //     };

    //     await _repository.AddApplicationAsync(application);

    //     var full = await _repository.GetApplicationByIdAsync(application.Id);
    //     return MapToApplicationGetDto(full!);
    // }
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
