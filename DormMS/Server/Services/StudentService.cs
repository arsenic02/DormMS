using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

    public Task<IEnumerable<Student>> GetAllAsync() => _repository.GetAllAsync();

    public Task<Student?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

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
        student.IsProfileComplete = true;
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
        student.IsProfileComplete = true;
        await _repository.UpdateAsync(student);

        return MapToGetDto(student);
    }

    public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);

    public async Task<StudentImportResultDto> ImportFromCsvAsync(IFormFile csvFile)
    {
        var result = new StudentImportResultDto();

        try
        {
            using var reader = new StreamReader(csvFile.OpenReadStream());
            using var csv = new CsvHelper.CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture);

            /////za slucaj da nece
            // using var reader = new StreamReader(csvFile.OpenReadStream());

            // var config = new CsvHelper.Configuration.CsvConfiguration(
            //     System.Globalization.CultureInfo.InvariantCulture)
            // {
            //     HeaderValidated = null,
            //     MissingFieldFound = null,
            //     PrepareHeaderForMatch = args => args.Header.ToLower()
            // };

            // using var csv = new CsvHelper.CsvReader(reader, config);
            /////// za slucaj da nece
            /// 

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
                        IsProfileComplete = false
                    };

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

                    result.SuccessfullyImported++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error importing {record.Email}: {ex.Message} | Inner: {ex.InnerException?.Message}");
                    result.Failed++;
                }
            }
            //await _context.SaveChangesAsync(); // ovde ne treba save changesAsync

        }
        catch (Exception ex)
        {
            result.Errors.Add(
                $"Import failed: {ex.Message} | Inner: {ex.InnerException?.Message}"
    );
            //result.Errors.Add($"CSV parsing error: {ex.Message}");
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

        if (dto.FacultyId <= 0)
            return false;

        if (dto.ESPB < 0)
            return false;

        return true;
    }

    private static void ApplyProfile(StudentProfileUpdateDto dto, Student student)
    {
        student.HomeAddress = dto.HomeAddress.Trim();
        student.Age = dto.Age;
        student.Gender = dto.Gender;
        student.YearOfEnrollment = dto.YearOfEnrollment;
        student.StudyLevel = dto.StudyLevel;
        student.FacultyId = dto.FacultyId;
        student.ESPB = dto.ESPB;
        student.IsBudgetStudent = dto.IsBudgetStudent;
        student.DormitoryRoomId = dto.DormitoryRoomId;
    }

    private static Student MapToEntity(StudentCreateUpdateDto dto, Student? existing = null)
    {
        var student = existing ?? new Student();

        student.Name = dto.Name;
        student.Surname = dto.Surname;
        student.HomeAddress = dto.HomeAddress;
        student.StudentIDNumber = dto.StudentIDNumber;
        student.Email = dto.Email;
        student.Age = dto.Age;
        student.Gender = dto.Gender;
        student.YearOfEnrollment = dto.YearOfEnrollment;
        student.StudyLevel = dto.StudyLevel;
        student.Status = dto.Status;
        student.FacultyId = dto.FacultyId;
        student.ESPB = dto.ESPB;
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
        StudentIDNumber = student.StudentIDNumber,
        HomeAddress = student.HomeAddress,
        Age = student.Age,
        Gender = student.Gender,
        YearOfEnrollment = student.YearOfEnrollment,
        StudyLevel = student.StudyLevel,
        Status = student.Status,
        FacultyId = student.FacultyId,
        ESPB = student.ESPB,
        IsBudgetStudent = student.IsBudgetStudent,
        DormitoryRoomId = student.DormitoryRoomId,
        CurrentYearOfStudy = student.CurrentYearOfStudy,
        IsProfileComplete = student.IsProfileComplete
    };
}
