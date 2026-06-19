using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DormMS.Server.Services.Interfaces;
using DormMS.Server.Repositories.Interfaces;
using DormMS.Server.Models;
using DormMS.Server.DTOs;

namespace DormMS.Server.Services.Implementations;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _repository;
    private readonly AppDbContext _context;
    private readonly PasswordHasher<User> _passwordHasher;

    public StudentService(IStudentRepository repository, AppDbContext context)
    {
        _repository = repository;
        _context = context;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<IEnumerable<StudentGetDto>> GetAllAsync()
    {
        var students = await _repository.GetAllAsync();
        return students.Select(MapToGetDto);
    }

    public async Task<StudentGetDto?> GetByIdAsync(int id)
    {
        var student = await _repository.GetByIdAsync(id);
        return student is null ? null : MapToGetDto(student);
    }

    public async Task<StudentGetDto?> GetByUserIdAsync(int userId)
    {
        var student = await _repository.GetByUserIdAsync(userId);
        return student is null ? null : MapToGetDto(student);
    }

    public async Task<Student?> CreateAsync(StudentCreateUpdateDto dto)
    {
        if (!await IsDormitoryRoomValidAsync(dto.DormitoryRoomId))
            return null;

        var student = MapToEntity(dto);
        student.ProfileStatus = StudentProfileStatus.PendingVerification;
        await _repository.AddAsync(student);
        return student;
    }

    public async Task<bool?> UpdateAsync(int id, StudentCreateUpdateDto dto)
    {
        var student = await _repository.GetByIdTrackedAsync(id);
        if (student is null)
            return null;

        if (!await IsDormitoryRoomValidAsync(dto.DormitoryRoomId))
            return false;

        MapToEntity(dto, student);
        await _repository.UpdateAsync(student);
        return true;
    }

    public async Task<StudentGetDto?> CompleteProfileAsync(int userId, StudentProfileUpdateDto dto)
    {
        if (!IsProfileValid(dto))
            return null;

        if (dto.DormitoryRoomId.HasValue && !await _repository.DormitoryRoomExistsAsync(dto.DormitoryRoomId.Value))
            return null;

        var student = await _repository.GetByIdTrackedByUserIdAsync(userId);
        if (student is null)
            return null;

        ApplyProfile(dto, student);
        student.ProfileStatus = StudentProfileStatus.PendingVerification;
        await _repository.UpdateAsync(student);

        return MapToGetDto(student);
    }

    public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);

    public async Task<StudentImportResultDto> ImportFromCsvAsync(IFormFile csvFile)
    {
        var result = new StudentImportResultDto();
        var random = new Random();

        try
        {
            using var reader = new StreamReader(csvFile.OpenReadStream());
            using var csv = new CsvHelper.CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture);

            var records = csv.GetRecords<StudentCsvImportDto>().ToList();
            result.TotalProcessed = records.Count;

            foreach (var record in records)
            {
                try
                {
                    var normalizedEmail = record.Email.Trim().ToLowerInvariant();
                    var indexNumber = record.StudentIDNumber.Trim();

                    // Check if email already exists
                    var emailExists = await _context.Users.AnyAsync(u => u.Email == normalizedEmail);
                    if (emailExists)
                    {
                        result.Errors.Add($"Email {normalizedEmail} already exists");
                        result.Failed++;
                        continue;
                    }

                    // Check if student ID number already exists
                    var indexExists = await _context.Students.AnyAsync(s => s.StudentIDNumber == indexNumber);
                    if (indexExists)
                    {
                        result.Errors.Add($"Student ID {indexNumber} already exists");
                        result.Failed++;
                        continue;
                    }

                    // Create student
                    var student = new Student
                    {
                        Name = record.Name.Trim(),
                        Surname = record.Surname.Trim(),
                        StudentIDNumber = indexNumber,
                        Email = normalizedEmail,
                        Status = StudentStatus.Active,
                        ProfileStatus = StudentProfileStatus.Incomplete,
                        ESPB = record.ESPB,
                        AverageGrade = record.AverageGrade,
                        YearOfEnrollment = record.YearOfEnrollment > 0 ? record.YearOfEnrollment : DateTime.Now.Year - random.Next(1, 5),
                        StudyLevel = StudyLevel.Undergraduate,
                        FacultyId = 1,
                        HighSchoolAverageGrade = record.HighSchoolAverageGrade,
                        FacultyName = record.FacultyName.Trim(),
                    };

                    ApplyRealisticData(student, random);

                    _context.Students.Add(student);
                    await _context.SaveChangesAsync();

                    // Create user
                    var user = new User
                    {
                        Email = normalizedEmail,
                        Role = UserRole.Student,
                        IsActive = true,
                        StudentId = student.Id
                    };

                    user.PasswordHash = _passwordHasher.HashPassword(user, record.Password);

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    // Create financial status
                    if (record.TotalFamilyIncome > 0 && record.FamilyMemberCount > 0)
                    {
                        var financialStatus = new StudentFinancialStatus
                        {
                            StudentId = student.Id,
                            TotalFamilyIncome = record.TotalFamilyIncome,
                            FamilyMemberCount = record.FamilyMemberCount,
                            IsVerified = true,
                            VerifiedAt = DateTime.UtcNow
                        };
                        _context.StudentFinancialStatuses.Add(financialStatus);
                        await _context.SaveChangesAsync();
                    }

                    result.SuccessfullyImported++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error importing {record.Email}: {ex.Message}");
                    result.Failed++;
                }
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Import failed: {ex.Message}");
        }

        return result;
    }

    private void ApplyRealisticData(Student student, Random random)
    {
        var faculties = new[]
        {
            "Građevinsko-arhitektonski fakultet (GAF)",
            "Elektronski fakultet",
            "Mašinski fakultet",
            "Pravni fakultet",
            "Filozofski fakultet",
            "Fakultet sporta i fizičkog vaspitanja (DIF)",
            "Fakultet zaštite na radu",
            "Ekonomski fakultet",
            "Medicinski fakultet",
            "Prirodno-matematički fakultet (PMF)",
            "Fakultet umetnosti"
        };

        var cities = new[]
        {
            "Leskovac", "Vranje", "Pirot", "Aleksinac", "Prokuplje", "Knjaževac",
            "Zaječar", "Kruševac", "Jagodina", "Paraćin", "Ćuprija", "Bor",
            "Negotin", "Surdulica", "Vladičin Han", "Bela Palanka", "Svrljig",
            "Doljevac", "Merošina", "Gadžin Han", "Sokobanja"
        };

        var streets = new[]
        {
            "Njegoševa", "Knjaževačka", "Dušanova", "Nikole Pašića",
            "Skopska", "Sremska", "Vojvode Mišića", "Jadranska",
            "Prvomajska", "Kosovska"
        };

        var facultyIndex = -1;
        if (!string.IsNullOrWhiteSpace(student.FacultyName))
        {
            facultyIndex = Array.IndexOf(faculties, student.FacultyName.Trim());
        }

        if (facultyIndex == -1)
        {
            facultyIndex = random.Next(faculties.Length);
        }

        student.FacultyName = faculties[facultyIndex];
        student.FacultyId = facultyIndex + 1;

        // var facultyIndex = random.Next(faculties.Length);
        // student.FacultyName = faculties[facultyIndex];
        // student.FacultyId = facultyIndex + 1;

        if (int.TryParse(student.StudentIDNumber, out _))
        {
            if (student.FacultyName.Contains("Elektronski"))
                student.StudentIDNumber = random.Next(10000, 19999).ToString();
            else if (student.FacultyName.Contains("GAF"))
                student.StudentIDNumber = (random.Next(2) == 0 ? "pa-" : "rg-") + random.Next(1000, 9999);
            else if (student.FacultyName.Contains("Mašinski"))
                student.StudentIDNumber = random.Next(1000, 9999).ToString();
            else
                student.StudentIDNumber = random.Next(1000, 99999).ToString();
        }

        if (student.Age == 0) student.Age = random.Next(19, 27);
        student.Gender = (Gender)random.Next(2);

        var day = random.Next(1, 29).ToString("D2");
        var month = random.Next(1, 13).ToString("D2");
        var yearPart = (DateTime.Now.Year - student.Age).ToString().Substring(1);
        var genderPart = random.Next(student.Gender == Gender.Male ? 0 : 500, student.Gender == Gender.Male ? 500 : 1000).ToString("D3");
        student.JMBG = day + month + yearPart + "91" + genderPart + random.Next(0, 10);

        student.MunicipalityOfResidence = random.Next(100) < 90 ? cities[random.Next(cities.Length)] : "Niš";
        student.HomeAddress = $"{streets[random.Next(streets.Length)]} {random.Next(1, 150)}";

        //if (student.ESPB == 0) student.ESPB = random.Next(30, 241);
        if (student.ESPB == 0 && !student.IsFreshman) student.ESPB = random.Next(30, 241);
        student.StudyLevel = random.Next(100) < 90 ? StudyLevel.Undergraduate : StudyLevel.Postgraduate;
        if (student.AverageGrade == 0) student.AverageGrade = Math.Round((decimal)(random.NextDouble() * 3.5 + 6.5), 2);

        student.IsBudgetStudent = true;
        student.ProfileStatus = StudentProfileStatus.Verified;

        if (student.FinancialStatus != null)
        {
            student.FinancialStatus.IsVerified = true;
            student.FinancialStatus.VerifiedAt = DateTime.UtcNow;
        }
    }

    public async Task<int> FixBulkImportedStudentsAsync()
    {
        var students = await _context.Students
            .Include(s => s.FinancialStatus)
            .ToListAsync();
        var random = new Random();
        foreach (var student in students) ApplyRealisticData(student, random);
        await _context.SaveChangesAsync();
        return students.Count;
    }

    public async Task<int> ClearAllStudentsAsync()
    {
        var students = await _context.Students.ToListAsync();
        var users = await _context.Users.Where(u => u.Role == UserRole.Student).ToListAsync();
        _context.Users.RemoveRange(users);
        _context.Students.RemoveRange(students);
        await _context.SaveChangesAsync();
        return students.Count;
    }

    public async Task<int> BulkVerifyProfilesAsync(BulkVerifyProfilesDto dto)
    {
        var studentIds = GetIdsFromDto(dto.StudentIds, dto.StartId, dto.EndId);
        var students = await _context.Students.Where(s => studentIds.Contains(s.Id)).ToListAsync();
        foreach (var student in students) student.ProfileStatus = dto.Status;
        await _context.SaveChangesAsync();
        return students.Count;
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

    public async Task<StudentFinancialStatusGetDto?> SubmitFinancialStatusAsync(
    int userId,
    StudentFinancialStatusCreateDto dto)
    {
        if (dto.FamilyMemberCount <= 0)
            return null;

        if (dto.TotalFamilyIncome < 0)
            return null;

        var student = await _repository.GetByIdTrackedByUserIdAsync(userId);

        if (student is null)
            return null;

        var existingFinancialStatus =
            await _repository.GetFinancialStatusByStudentIdAsync(student.Id);

        if (existingFinancialStatus is not null)
        {
            existingFinancialStatus.TotalFamilyIncome = dto.TotalFamilyIncome;
            existingFinancialStatus.FamilyMemberCount = dto.FamilyMemberCount;
            existingFinancialStatus.IsVerified = false;
            existingFinancialStatus.SubmittedAt = DateTime.UtcNow;
            existingFinancialStatus.VerifiedAt = null;

            await _context.SaveChangesAsync();

            return new StudentFinancialStatusGetDto
            {
                TotalFamilyIncome = existingFinancialStatus.TotalFamilyIncome,
                FamilyMemberCount = existingFinancialStatus.FamilyMemberCount,
                IncomePerMember = existingFinancialStatus.IncomePerMember,
                IsVerified = existingFinancialStatus.IsVerified,
                SubmittedAt = existingFinancialStatus.SubmittedAt,
                VerifiedAt = existingFinancialStatus.VerifiedAt
            };
        }

        var financialStatus = new StudentFinancialStatus
        {
            StudentId = student.Id,
            TotalFamilyIncome = dto.TotalFamilyIncome,
            FamilyMemberCount = dto.FamilyMemberCount,
            IsVerified = false,
            SubmittedAt = DateTime.UtcNow
        };

        await _repository.AddFinancialStatusAsync(financialStatus);

        return new StudentFinancialStatusGetDto
        {
            TotalFamilyIncome = financialStatus.TotalFamilyIncome,
            FamilyMemberCount = financialStatus.FamilyMemberCount,
            IncomePerMember = financialStatus.IncomePerMember,
            IsVerified = financialStatus.IsVerified,
            SubmittedAt = financialStatus.SubmittedAt,
            VerifiedAt = financialStatus.VerifiedAt
        };
    }

    public async Task<BulkFinancialStatusImportResultDto> BulkImportFinancialStatusAsync(IFormFile csvFile)
    {
        var result = new BulkFinancialStatusImportResultDto();

        try
        {
            using var reader = new StreamReader(csvFile.OpenReadStream());
            using var csv = new CsvHelper.CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture);

            var records = csv.GetRecords<BulkFinancialStatusImportDto>().ToList();
            result.TotalProcessed = records.Count;

            var financialStatusesToAdd = new List<StudentFinancialStatus>();

            foreach (var record in records)
            {
                try
                {
                    var studentIdNumber = record.StudentIDNumber.Trim();

                    var student = await _context.Students
                        .FirstOrDefaultAsync(s => s.StudentIDNumber == studentIdNumber);

                    if (student is null)
                    {
                        result.Errors.Add($"Student with ID {studentIdNumber} not found");
                        result.Failed++;
                        continue;
                    }

                    var existingFinancialStatus = await _repository.GetFinancialStatusByStudentIdAsync(student.Id);
                    if (existingFinancialStatus is not null)
                    {
                        result.Errors.Add($"Student {studentIdNumber} already has financial status");
                        result.Failed++;
                        continue;
                    }

                    if (record.FamilyMemberCount <= 0)
                    {
                        result.Errors.Add($"Invalid family member count for student {studentIdNumber}");
                        result.Failed++;
                        continue;
                    }

                    if (record.TotalFamilyIncome < 0)
                    {
                        result.Errors.Add($"Invalid total family income for student {studentIdNumber}");
                        result.Failed++;
                        continue;
                    }

                    var financialStatus = new StudentFinancialStatus
                    {
                        StudentId = student.Id,
                        TotalFamilyIncome = record.TotalFamilyIncome,
                        FamilyMemberCount = record.FamilyMemberCount,
                        IsVerified = true,
                        VerifiedAt = DateTime.UtcNow
                    };

                    financialStatusesToAdd.Add(financialStatus);
                    result.SuccessfullyImported++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error processing student {record.StudentIDNumber}: {ex.Message}");
                    result.Failed++;
                }
            }

            if (financialStatusesToAdd.Any())
            {
                await _repository.BulkAddFinancialStatusesAsync(financialStatusesToAdd);
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Import failed: {ex.Message} | Inner: {ex.InnerException?.Message}");
        }

        return result;
    }
    private async Task<bool> IsDormitoryRoomValidAsync(int? dormitoryRoomId)
    {
        if (!dormitoryRoomId.HasValue)
            return true;

        return await _repository.DormitoryRoomExistsAsync(dormitoryRoomId.Value);
    }



    private static bool IsProfileValid(StudentProfileUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.HomeAddress))
            return false;

        if (dto.Age <= 0)
            return false;

        if (dto.YearOfEnrollment <= 0)
            return false;

        // if (dto.FacultyId <= 0)
        //     return false;

        if (dto.ESPB < 0)
            return false;

        return true;
    }

    private static void ApplyProfile(StudentProfileUpdateDto dto, Student student)
    {
        student.HomeAddress = dto.HomeAddress.Trim();
        student.MunicipalityOfResidence = dto.MunicipalityOfResidence.Trim();
        student.JMBG = dto.JMBG.Trim();
        student.Age = dto.Age;
        student.Gender = dto.Gender;
        student.YearOfEnrollment = dto.YearOfEnrollment;
        student.StudyLevel = dto.StudyLevel;
        student.FacultyName = dto.FacultyName.Trim();
        student.FacultyId = GetFacultyIdByName(student.FacultyName);
        student.ESPB = dto.ESPB;
        student.AverageGrade = dto.AverageGrade;
        student.HighSchoolAverageGrade = dto.HighSchoolAverageGrade;
        student.IsBudgetStudent = dto.IsBudgetStudent;
        student.DormitoryRoomId = dto.DormitoryRoomId;
    }

    private static int GetFacultyIdByName(string? facultyName)
    {
        if (string.IsNullOrWhiteSpace(facultyName)) return 0;

        var faculties = new[]
        {
            "Građevinsko-arhitektonski fakultet (GAF)",
            "Elektronski fakultet",
            "Mašinski fakultet",
            "Pravni fakultet",
            "Filozofski fakultet",
            "Fakultet sporta i fizičkog vaspitanja (DIF)",
            "Fakultet zaštite na radu",
            "Ekonomski fakultet",
            "Medicinski fakultet",
            "Prirodno-matematički fakultet (PMF)",
            "Fakultet umetnosti"
        };

        for (int i = 0; i < faculties.Length; i++)
        {
            if (faculties[i].Equals(facultyName, StringComparison.OrdinalIgnoreCase))
            {
                return i + 1;
            }
        }

        return 0;
    }

    private static string? GetFacultyNameById(int facultyId)
    {
        var faculties = new[]
        {
            "Građevinsko-arhitektonski fakultet (GAF)",
            "Elektronski fakultet",
            "Mašinski fakultet",
            "Pravni fakultet",
            "Filozofski fakultet",
            "Fakultet sporta i fizičkog vaspitanja (DIF)",
            "Fakultet zaštite na radu",
            "Ekonomski fakultet",
            "Medicinski fakultet",
            "Prirodno-matematički fakultet (PMF)",
            "Fakultet umetnosti"
        };

        if (facultyId >= 1 && facultyId <= faculties.Length)
        {
            return faculties[facultyId - 1];
        }

        return null;
    }

    public async Task<StudentGetDto?> VerifyProfileAsync(int studentId, StudentProfileStatus status)
    {
        var student = await _repository.GetByIdTrackedAsync(studentId);

        if (student is null)
        {

            var untrackedStudent = await _repository.GetByIdAsync(studentId);
            if (untrackedStudent is null) return null;

            student = untrackedStudent;
            _context.Students.Update(student);
        }

        student.ProfileStatus = status;
        await _repository.UpdateProfileAsync(student);

        var updated = await _repository.GetByIdAsync(studentId);
        return updated != null ? MapToGetDto(updated) : null;
    }

    public async Task<bool> VerifyFinancialStatusAsync(int studentId, bool isVerified)
    {
        var financialStatus = await _context.StudentFinancialStatuses
            .FirstOrDefaultAsync(fs => fs.StudentId == studentId);

        if (financialStatus == null) return false;

        financialStatus.IsVerified = isVerified;
        financialStatus.VerifiedAt = isVerified ? DateTime.UtcNow : null;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<StudentSummaryDto>> GetByFreshmanStatusAsync(bool? isFreshman)
    {
        var students = await _repository.GetAllAsync();

        var filtered = isFreshman.HasValue
            ? students.Where(s => s.IsFreshman == isFreshman.Value)
            : students;

        return filtered
            .Select(s => new StudentSummaryDto
            {
                Id = s.Id,
                Name = s.Name,
                Surname = s.Surname,
                FacultyName = s.FacultyName,
                IsFreshman = s.IsFreshman
            })
            .ToList();
    }

   
    private static Student MapToEntity(StudentCreateUpdateDto dto, Student? existing = null)
    {
        var student = existing ?? new Student();

        student.Name = dto.Name;
        student.Surname = dto.Surname;
        student.HomeAddress = dto.HomeAddress;
        student.MunicipalityOfResidence = dto.MunicipalityOfResidence;
        student.JMBG = dto.JMBG;
        student.StudentIDNumber = dto.StudentIDNumber;
        student.Email = dto.Email;
        student.Age = dto.Age;
        student.Gender = dto.Gender;
        student.YearOfEnrollment = dto.YearOfEnrollment;
        student.StudyLevel = dto.StudyLevel;
        student.Status = dto.Status;
        student.FacultyName = dto.FacultyName.Trim();
        student.FacultyId = GetFacultyIdByName(student.FacultyName);
        student.ESPB = dto.ESPB;
        student.AverageGrade = dto.AverageGrade;
        student.HighSchoolAverageGrade = dto.HighSchoolAverageGrade;
        student.IsBudgetStudent = dto.IsBudgetStudent;
        student.DormitoryRoomId = dto.DormitoryRoomId;

        return student;
    }

    private static StudentGetDto MapToGetDto(Student student) => new()
    {
        Id = student.Id,
        Name = student.Name,
        Surname = student.Surname,
        Email = student.Email,
        JMBG = student.JMBG,
        StudentIDNumber = student.StudentIDNumber,
        HomeAddress = student.HomeAddress,
        MunicipalityOfResidence = student.MunicipalityOfResidence,
        Age = student.Age,
        Gender = student.Gender,
        YearOfEnrollment = student.YearOfEnrollment,
        StudyLevel = student.StudyLevel,
        Status = student.Status,
        FacultyId = student.FacultyId,
        FacultyName = student.FacultyName,
        ESPB = student.ESPB,
        AverageGrade = student.AverageGrade,
        HighSchoolAverageGrade = student.HighSchoolAverageGrade,
        IsBudgetStudent = student.IsBudgetStudent,
        DormitoryRoomId = student.DormitoryRoomId,
        CurrentYearOfStudy = student.CurrentYearOfStudy,
        IsFreshman = student.IsFreshman,
        ProfileStatus = student.ProfileStatus,
        IsEligibleToApply = student.IsEligibleToApply,
        IsProfileComplete = student.ProfileStatus == StudentProfileStatus.Verified,
        FinancialStatus = student.FinancialStatus == null ? null : new StudentFinancialStatusGetDto
        {
            TotalFamilyIncome = student.FinancialStatus.TotalFamilyIncome,
            FamilyMemberCount = student.FinancialStatus.FamilyMemberCount,
            IncomePerMember = student.FinancialStatus.IncomePerMember,
            IsVerified = student.FinancialStatus.IsVerified,
            SubmittedAt = student.FinancialStatus.SubmittedAt,
            VerifiedAt = student.FinancialStatus.VerifiedAt
        }
    };
}
