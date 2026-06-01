public class StudentService : IStudentService
{
    private readonly IStudentRepository _repository;

    public StudentService(IStudentRepository repository)
    {
        _repository = repository;
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
