using Microsoft.EntityFrameworkCore;
using DormMS.Server.Services.Interfaces;
using DormMS.Server.Repositories.Interfaces;
using DormMS.Server.Models;
using DormMS.Server.DTOs;

namespace DormMS.Server.Services.Implementations;

public class CompetitionApplicationService : ICompetitionApplicationService
{
    private readonly ICompetitionApplicationRepository _repository;
    private readonly ICompetitionRepository _competitionRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly AppDbContext _context;

    public CompetitionApplicationService(
        ICompetitionApplicationRepository repository,
        ICompetitionRepository competitionRepository,
        IStudentRepository studentRepository,
        AppDbContext context)
    {
        _repository = repository;
        _competitionRepository = competitionRepository;
        _studentRepository = studentRepository;
        _context = context;
    }

    public async Task<ApplyResult> ApplyAsync(int userId, CompetitionApplicationCreateDto dto)
    {
        var studentId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.StudentId)
            .FirstOrDefaultAsync();

        if (studentId is null)
            return new(null, "Student not found");

        var competition = await _competitionRepository.GetByIdAsync(dto.CompetitionId);
        if (competition is null)
            return new(null, "Competition not found");

        if (!competition.IsOpen)
            return new(null, "Competition is not open");

        var student = await _studentRepository.GetByIdWithFinancialStatusAsync((int)studentId);
        if (student is null)
            return new(null, "Student not found");

        if (!student.IsEligibleToApply)
            return new(null, "Student is not eligible to apply. Check profile verification, financial status and budget status.");

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
            DocumentUrl = dto.DocumentUrl,
            Status = ApplicationStatus.Pending,
            AppliedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(application);
        var full = await _repository.GetApplicationByIdAsync(application.Id);
        return new(MapToApplicationGetDto(full!), null);
    }

    public async Task<IEnumerable<CompetitionApplicationGetDto>> GetApplicationsByCompetitionIdAsync(int competitionId)
    {
        var applications = await _repository.GetApplicationsByCompetitionIdAsync(competitionId);
        return applications.Select(MapToApplicationGetDto);
    }

    public async Task<IEnumerable<CompetitionApplicationGetDto>> GetAllAsync()
    {
        var applications = await _repository.GetAllAsync();
        return applications.Select(MapToApplicationGetDto);
    }

    public async Task<IEnumerable<CompetitionApplicationGetDto>> GetVerifiedApplicationsAsync()
    {
        var applications = await _repository.GetByStatusAsync(ApplicationStatus.Approved);
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
        application.DocumentUrl = dto.DocumentUrl ?? application.DocumentUrl;

        await _repository.UpdateAsync(application);
        return MapToApplicationGetDto(application);
    }

    public async Task<bool> DeleteApplicationAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<int> DeleteApplicationsByCompetitionIdAsync(int competitionId)
    {
        return await _repository.DeleteByCompetitionIdAsync(competitionId);
    }

    public async Task<CompetitionImportResultDto> BulkApplyAsync(BulkApplicationDto dto)
    {
        var result = new CompetitionImportResultDto();
        var competition = await _competitionRepository.GetByIdAsync(dto.CompetitionId);

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

        var studentIds = GetIdsFromDto(dto.StudentIds, dto.StartId, dto.EndId);
        result.TotalProcessed = studentIds.Count;

        var students = await _context.Students
            .Include(s => s.FinancialStatus)
            .Where(s => studentIds.Contains(s.Id))
            .ToListAsync();

        var studentsDict = students.ToDictionary(s => s.Id);

        foreach (var studentId in studentIds)
        {
            try
            {
                if (!studentsDict.TryGetValue(studentId, out var student))
                {
                    result.Errors.Add($"Student {studentId} not found");
                    result.Failed++;
                    continue;
                }

                if (!student.IsEligibleToApply)
                {
                    result.Errors.Add($"Student {studentId} ({student.Name} {student.Surname}) is not eligible to apply. Check budget status, profile verification and financial status.");
                    result.Failed++;
                    continue;
                }

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

                await _repository.AddAsync(application);
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

    public async Task<BulkVerifyApplicationsResultDto> BulkVerifyApplicationsAsync(BulkVerifyApplicationsDto dto)
    {
        var result = new BulkVerifyApplicationsResultDto();
        var applicationIds = GetIdsFromDto(dto.ApplicationIds, dto.StartId, dto.EndId);
        result.TotalProcessed = applicationIds.Count;

        foreach (var applicationId in applicationIds)
        {
            try
            {
                var application = await _context.CompetitionApplications
                    .Include(a => a.Student)
                    .Include(a => a.Competition)
                    .FirstOrDefaultAsync(a => a.Id == applicationId);

                if (application is null)
                {
                    result.Errors.Add($"Application {applicationId} not found");
                    result.Failed++;
                    continue;
                }

                application.Status = dto.Status;
                await _repository.UpdateAsync(application);
                result.SuccessfullyVerified++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error verifying application {applicationId}: {ex.Message}");
                result.Failed++;
            }
        }

        return result;
    }

    private List<int> GetIdsFromDto(List<int>? ids, int? startId, int? endId)
    {
        if (ids != null && ids.Any()) return ids;
        if (startId.HasValue && endId.HasValue)
        {
            var start = Math.Min(startId.Value, endId.Value);
            var end = Math.Max(startId.Value, endId.Value);
            return Enumerable.Range(start, end - start + 1).ToList();
        }
        return new List<int>();
    }

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
        Notes = application.Notes,
        DocumentUrl = application.DocumentUrl
    };
}
