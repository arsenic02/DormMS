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
    public async Task<MyRoomGetDto> GetMyRoomAsync(int userId)
    {
        var student = await _repository.GetByUserIdAsync(userId);

        if (student is null || student.DormitoryRoomId is null)
        {
            return new MyRoomGetDto { HasRoom = false };
        }

        var roomId = student.DormitoryRoomId.Value;

        var room = await _context.DormitoryRooms
            .Include(r => r.Dorm)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == roomId);

        if (room is null)
        {
            return new MyRoomGetDto { HasRoom = false };
        }

        var roommates = await _context.Students
            .Where(s => s.DormitoryRoomId == roomId && s.Id != student.Id)
            .AsNoTracking()
            .Select(s => new RoommateSummaryDto
            {
                Id = s.Id,
                Name = s.Name,
                Surname = s.Surname,
                StudentIDNumber = s.StudentIDNumber,
                FacultyName = s.FacultyName,
                IsFreshman = s.YearOfEnrollment == DateTime.Now.Year
            })
            .ToListAsync();

        return new MyRoomGetDto
        {
            HasRoom = true,
            RoomId = room.Id,
            RoomNumber = room.RoomNumber,
            DormName = room.Dorm?.Name,
            Capacity = room.Capacity,
            CurrentOccupancy = room.CurrentOccupancy,
            Roommates = roommates
        };
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
        student.ProfileStatus = StudentProfileStatus.Verified;
        await _repository.UpdateAsync(student);

        // Handle financial data if provided
        if (dto.TotalFamilyIncome.HasValue && dto.FamilyMemberCount.HasValue)
        {
            var existingFinancialStatus = await _repository.GetFinancialStatusByStudentIdAsync(student.Id);

            if (existingFinancialStatus is not null)
            {
                existingFinancialStatus.TotalFamilyIncome = dto.TotalFamilyIncome.Value;
                existingFinancialStatus.FamilyMemberCount = dto.FamilyMemberCount.Value;
                existingFinancialStatus.SubmittedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
            }
            else
            {
                var financialStatus = new StudentFinancialStatus
                {
                    StudentId = student.Id,
                    TotalFamilyIncome = dto.TotalFamilyIncome.Value,
                    FamilyMemberCount = dto.FamilyMemberCount.Value,
                    SubmittedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)
                };
                await _repository.AddFinancialStatusAsync(financialStatus);
            }
            await _context.SaveChangesAsync();
        }

        return MapToGetDto(student);
    }

    // public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);

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

                    var emailExists = await _context.Users.AnyAsync(u => u.Email == normalizedEmail);
                    if (emailExists)
                    {
                        result.Errors.Add($"Email {normalizedEmail} already exists");
                        result.Failed++;
                        continue;
                    }

                    var indexExists = await _context.Students.AnyAsync(s => s.StudentIDNumber == indexNumber);
                    if (indexExists)
                    {
                        result.Errors.Add($"Student ID {indexNumber} already exists");
                        result.Failed++;
                        continue;
                    }

                    var student = new Student
                    {
                        Name = record.Name.Trim(),
                        Surname = record.Surname.Trim(),
                        StudentIDNumber = indexNumber,
                        Email = normalizedEmail,
                        Status = StudentStatus.Active,
                        ProfileStatus = StudentProfileStatus.Verified,
                        ESPB = record.ESPB,
                        AverageGrade = record.AverageGrade,
                        YearOfEnrollment = record.YearOfEnrollment > 0 ? record.YearOfEnrollment : DateTime.Now.Year - random.Next(1, 5),
                        StudyLevel = StudyLevel.Undergraduate,
                        FacultyId = 1,
                        HighSchoolAverageGrade = record.HighSchoolAverageGrade,
                        FacultyName = record.FacultyName.Trim(),
                        Gender = record.Gender, // CITA SE IZ CSV-A, ne pogadja se
                    };

                    ApplyRealisticData(student, random);

                    _context.Students.Add(student);
                    await _context.SaveChangesAsync();

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

                    var financialStatus = new StudentFinancialStatus
                    {
                        StudentId = student.Id,
                        TotalFamilyIncome = record.TotalFamilyIncome > 0 ? record.TotalFamilyIncome : 0,
                        FamilyMemberCount = record.FamilyMemberCount > 0 ? record.FamilyMemberCount : 1
                    };
                    _context.StudentFinancialStatuses.Add(financialStatus);
                    await _context.SaveChangesAsync();

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

        var day = random.Next(1, 29).ToString("D2");
        var month = random.Next(1, 13).ToString("D2");
        var yearPart = (DateTime.Now.Year - student.Age).ToString().Substring(1);
        var genderPart = random.Next(student.Gender == Gender.Male ? 0 : 500, student.Gender == Gender.Male ? 500 : 1000).ToString("D3");
        student.JMBG = day + month + yearPart + "91" + genderPart + random.Next(0, 10);

        student.MunicipalityOfResidence = random.Next(100) < 90 ? cities[random.Next(cities.Length)] : "Niš";
        student.HomeAddress = $"{streets[random.Next(streets.Length)]} {random.Next(1, 150)}";

        if (student.ESPB == 0 && !student.IsFreshman) student.ESPB = random.Next(30, 241);
        student.StudyLevel = random.Next(100) < 90 ? StudyLevel.Undergraduate : StudyLevel.Postgraduate;
        if (student.AverageGrade == 0) student.AverageGrade = Math.Round((decimal)(random.NextDouble() * 3.5 + 6.5), 2);

        student.IsBudgetStudent = true;
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
            existingFinancialStatus.SubmittedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

            await _context.SaveChangesAsync();

            return new StudentFinancialStatusGetDto
            {
                TotalFamilyIncome = existingFinancialStatus.TotalFamilyIncome,
                FamilyMemberCount = existingFinancialStatus.FamilyMemberCount,
                IncomePerMember = existingFinancialStatus.IncomePerMember,
                SubmittedAt = existingFinancialStatus.SubmittedAt
            };
        }

        var financialStatus = new StudentFinancialStatus
        {
            StudentId = student.Id,
            TotalFamilyIncome = dto.TotalFamilyIncome,
            FamilyMemberCount = dto.FamilyMemberCount,
            SubmittedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)
        };

        await _repository.AddFinancialStatusAsync(financialStatus);

        return new StudentFinancialStatusGetDto
        {
            TotalFamilyIncome = financialStatus.TotalFamilyIncome,
            FamilyMemberCount = financialStatus.FamilyMemberCount,
            IncomePerMember = financialStatus.IncomePerMember,
            SubmittedAt = financialStatus.SubmittedAt
        };
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

    public Task<PagedResultDto<StudentGetDto>> GetPagedAsync(
    int pageNumber, int pageSize, StudentProfileStatus? profileStatusFilter, string? facultyFilter, bool? isFreshmanFilter)
    => _repository.GetPagedAsync(pageNumber, pageSize, profileStatusFilter, facultyFilter, isFreshmanFilter);

    // public Task<PagedResultDto<StudentGetDto>> GetPagedAsync(
    //     int pageNumber, int pageSize, StudentProfileStatus? profileStatusFilter, string? facultyFilter, bool? isFreshmanFilter)
    //     => _repository.GetPagedAsync(pageNumber, pageSize, profileStatusFilter, facultyFilter, isFreshmanFilter);
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
            SubmittedAt = student.FinancialStatus.SubmittedAt
        }
    };
}
