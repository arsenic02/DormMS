public class StudentService : IStudentService
{
    private readonly IStudentRepository _repository;

    public StudentService(IStudentRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Student>> GetAllAsync() => _repository.GetAllAsync();

    public Task<Student?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

    public async Task<(Student? Student, string? Error)> CreateAsync(StudentCreateUpdateDto dto)
    {
        var validationError = await ValidateDormitoryRoomAsync(dto.DormitoryRoomId);
        if (validationError is not null)
            return (null, validationError);

        var student = MapToEntity(dto);
        await _repository.AddAsync(student);
        return (student, null);
    }

    public async Task<(bool Found, string? Error)> UpdateAsync(int id, StudentCreateUpdateDto dto)
    {
        var student = await _repository.GetByIdTrackedAsync(id);
        if (student is null)
            return (false, null);

        var validationError = await ValidateDormitoryRoomAsync(dto.DormitoryRoomId);
        if (validationError is not null)
            return (true, validationError);

        MapToEntity(dto, student);
        await _repository.UpdateAsync(student);
        return (true, null);
    }

    public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);

    private async Task<string?> ValidateDormitoryRoomAsync(int? dormitoryRoomId)
    {
        if (!dormitoryRoomId.HasValue)
            return null;

        var exists = await _repository.DormitoryRoomExistsAsync(dormitoryRoomId.Value);
        if (!exists)
            return $"Soba sa ID {dormitoryRoomId.Value} ne postoji.";

        return null;
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
}
