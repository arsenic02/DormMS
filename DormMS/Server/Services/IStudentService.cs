public interface IStudentService
{
    Task<IEnumerable<Student>> GetAllAsync();
    Task<Student?> GetByIdAsync(int id);
    Task<(Student? Student, string? Error)> CreateAsync(StudentCreateUpdateDto dto);
    Task<(bool Found, string? Error)> UpdateAsync(int id, StudentCreateUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
