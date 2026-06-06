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

        await _repository.UpdateAsync(application);
        return MapToApplicationGetDto(application);
    }

    public async Task<bool> DeleteApplicationAsync(int id)
    {
        return await _repository.DeleteAsync(id);
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
        result.TotalProcessed = dto.ApplicationIds.Count;

        foreach (var applicationId in dto.ApplicationIds)
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
