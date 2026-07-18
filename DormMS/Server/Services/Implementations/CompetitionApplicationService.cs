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

    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public CompetitionApplicationService(
        ICompetitionApplicationRepository repository,
        ICompetitionRepository competitionRepository,
        IStudentRepository studentRepository,
        AppDbContext context,
        IConfiguration configuration,
        IWebHostEnvironment environment
        )
    {
        _repository = repository;
        _competitionRepository = competitionRepository;
        _studentRepository = studentRepository;
        _context = context;
        _configuration = configuration;
        _environment = environment;
    }

    public async Task<ApplyResult> ApplyAsync(int userId, CompetitionApplicationCreateDto dto)
    {
        var studentId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.StudentId)
            .FirstOrDefaultAsync();

        if (studentId is null)
            return new(null, "Student nije pronađen");

        var competition = await _competitionRepository.GetByIdAsync(dto.CompetitionId);
        if (competition is null)
            return new(null, "Konkurs nije pronađen");

        if (!competition.IsOpen)
            return new(null, "Konkurs nije otvoren za prijave");

        var student = await _context.Students
            .Include(s => s.FinancialStatus)
            .FirstOrDefaultAsync(s => s.Id == (int)studentId);
        if (student is null)
            return new(null, "Student nije pronađen");

        if (competition.Type == CompetitionType.Freshmen && !student.IsFreshman)
            return new(null, "Ovaj konkurs je namenjen samo brucošima. Stariji studenti ne mogu da konkurišu.");

        if (competition.Type == CompetitionType.General && student.IsFreshman)
            return new(null, "Ovaj konkurs je namenjen samo starijim studentima. Brucoši ne mogu da konkurišu.");

        if (!string.IsNullOrWhiteSpace(dto.DocumentUrl) && !dto.DocumentUrl.StartsWith("/uploads/documents/"))
            return new(null, "Nevažeća putanja dokumenta");

        if (!student.IsBudgetStudent)
            return new(null, "Samo budžetski studenti mogu da konkurišu za smeštaj u domu.");

        if (student.ProfileStatus == StudentProfileStatus.Incomplete)
            return new(null, "Molim vas da popunite podatke o profilu pre prijave na konkurs.");

        // Use financial data from request if provided, otherwise use student's existing financial status
        decimal totalFamilyIncome;
        int familyMemberCount;

        if (dto.FinancialData is not null)
        {
            if (dto.FinancialData.TotalFamilyIncome <= 0)
                return new(null, "Ukupan prihod porodice mora biti veći od 0.");

            if (dto.FinancialData.FamilyMemberCount <= 0)
                return new(null, "Broj članova porodice mora biti veći od 0.");

            totalFamilyIncome = dto.FinancialData.TotalFamilyIncome;
            familyMemberCount = dto.FinancialData.FamilyMemberCount;
        }
        else
        {
            if (student.FinancialStatus is null)
                return new(null, "Nemate popunjene finansijske podatke u profilu. Molim vas da popunite finansijske podatke u svom profilu pre prijave na konkurs.");

            totalFamilyIncome = student.FinancialStatus.TotalFamilyIncome;
            familyMemberCount = student.FinancialStatus.FamilyMemberCount;
        }

        if (student.DormitoryRoomId != null)
            return new(null, "Već imate dodeljeno mesto u domu. Ne možete se prijaviti na konkurs.");

        // Provera opstine
        if (competition.CreatedByDormManagerId.HasValue)
        {
            var dormManager = await _context.DormManagers
                .Include(dm => dm.Dorm)
                .FirstOrDefaultAsync(dm => dm.Id == competition.CreatedByDormManagerId.Value);

            if (dormManager?.Dorm != null &&
                student.MunicipalityOfResidence.Equals(
                    dormManager.Dorm.Municipality, StringComparison.OrdinalIgnoreCase))
                return new(null, "Studenti iz opštine u kojoj se dom nalazi ne mogu konkurisati za smeštaj.");
        }

        var existingApplication = await _repository
            .GetApplicationByStudentAndCompetitionAsync(studentId.Value, dto.CompetitionId);
        if (existingApplication is not null)
            return new(null, "Već ste se prijavili na ovaj konkurs");

        var application = new CompetitionApplication
        {
            StudentId = studentId.Value,
            CompetitionId = dto.CompetitionId,
            Notes = dto.Notes,
            DocumentUrl = dto.DocumentUrl,
            Status = ApplicationStatus.Pending,
            // AppliedAt = DateTime.UtcNow
            AppliedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)
        };

        // Create or update financial status for this application only if new data was provided
        if (dto.FinancialData is not null)
        {
            var financialStatus = await _context.StudentFinancialStatuses
                .FirstOrDefaultAsync(fs => fs.StudentId == studentId.Value);

            if (financialStatus is null)
            {
                financialStatus = new StudentFinancialStatus
                {
                    StudentId = studentId.Value,
                    TotalFamilyIncome = totalFamilyIncome,
                    FamilyMemberCount = familyMemberCount,
                    SubmittedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)
                };
                _context.StudentFinancialStatuses.Add(financialStatus);
            }
            else
            {
                financialStatus.TotalFamilyIncome = totalFamilyIncome;
                financialStatus.FamilyMemberCount = familyMemberCount;
            }
        }

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

    public async Task<PagedResultDto<CompetitionApplicationGetDto>> GetPagedByCompetitionIdAsync(
    int competitionId, int pageNumber, int pageSize, ApplicationStatus? status)
    {
        var (applications, totalCount) = await _repository.GetPagedByCompetitionIdAsync(
            competitionId, pageNumber, pageSize, status);

        return new PagedResultDto<CompetitionApplicationGetDto>
        {
            Items = applications.Select(MapToApplicationGetDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<CompetitionApplicationGetDto?> UpdateApplicationAsync(int id, CompetitionApplicationUpdateDto dto)
    {
        var application = await _context.CompetitionApplications
            .Include(a => a.Student)
            .Include(a => a.Competition)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application is null)
            return null;

        var finalDocumentUrl = dto.DocumentUrl ?? application.DocumentUrl;

        if (dto.Status == ApplicationStatus.Approved && string.IsNullOrWhiteSpace(finalDocumentUrl))
            throw new InvalidOperationException("Prijava ne može biti odobrena bez priloženog dokumenta.");

        // if (dto.Status == ApplicationStatus.Approved && string.IsNullOrWhiteSpace(application.DocumentUrl))
        //     throw new InvalidOperationException("Prijava ne može biti odobrena bez priloženog dokumenta.");

        application.Status = dto.Status;
        application.PriorityScore = dto.PriorityScore;
        application.Notes = dto.Notes ?? application.Notes;
        //application.DocumentUrl = dto.DocumentUrl ?? application.DocumentUrl;
        application.DocumentUrl = finalDocumentUrl;

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

                if (competition.Type == CompetitionType.Freshmen && !student.IsFreshman)
                {
                    result.Errors.Add($"Student {studentId} ({student.Name} {student.Surname}) is not a freshman and cannot apply to this competition.");
                    result.Failed++;
                    continue;
                }

                if (competition.Type == CompetitionType.General && student.IsFreshman)
                {
                    result.Errors.Add($"Student {studentId} ({student.Name} {student.Surname}) is a freshman and cannot apply to this general competition.");
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

        Console.WriteLine($"[BULKVERIFY] dto.Status={dto.Status}, applicationIds.Count={applicationIds.Count}, first few ids: {string.Join(",", applicationIds.Take(5))}");
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

                if (dto.Status == ApplicationStatus.Approved && string.IsNullOrWhiteSpace(application.DocumentUrl))
                {
                    result.Errors.Add($"Application {applicationId} cannot be approved without an attached document");
                    result.Failed++;
                    continue;
                }
                var oldStatus = application.Status;//DODATO ZA TEST
                application.Status = dto.Status;

                //DODATO ZA TESTIRANJE
                var entry = _context.Entry(application);
                Console.WriteLine($"[BULKVERIFY] id={applicationId}, oldStatus={oldStatus}, newStatus={application.Status}, EF State BEFORE UpdateAsync={entry.State}");

                await _repository.UpdateAsync(application);

                //DODATO ZA TESTIRANJE
                Console.WriteLine($"[BULKVERIFY] id={applicationId}, EF State AFTER UpdateAsync={entry.State}");

                result.SuccessfullyVerified++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error verifying application {applicationId}: {ex.Message}");
                result.Failed++;

                //ZA TESTIRANJE
                // Console.WriteLine($"[BULKVERIFY] EXCEPTION for id={applicationId}: {ex}");
            }
        }

        return result;
    }

    public async Task<BulkVerifyApplicationsResultDto> BulkVerifyAllForCompetitionAsync(int competitionId, ApplicationStatus status)
    {
        var applicationIds = await _repository.GetApplicationIdsByCompetitionIdAsync(competitionId);

        var dto = new BulkVerifyApplicationsDto
        {
            ApplicationIds = applicationIds,
            Status = status
        };

        return await BulkVerifyApplicationsAsync(dto);
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

    public async Task<string> SaveDocumentAsync(IFormFile file)
    {
        var configuredPath = _configuration["FileStorage:UploadsPath"] ?? "../DormMS-Uploads";
        var uploadsFolder = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, configuredPath, "documents"));
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/documents/{fileName}";
    }

    // public async Task<string> SaveDocumentAsync(IFormFile file)
    // {
    //     var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documents");
    //     Directory.CreateDirectory(uploadsFolder);

    //     var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
    //     var filePath = Path.Combine(uploadsFolder, fileName);

    //     using (var stream = new FileStream(filePath, FileMode.Create))
    //     {
    //         await file.CopyToAsync(stream);
    //     }

    //     return $"/uploads/documents/{fileName}";
    // }

}
