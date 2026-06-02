public interface IStudentService
{
    Task<IEnumerable<Student>> GetAllAsync();
    Task<Student?> GetByIdAsync(int id);
    Task<StudentGetDto?> GetByUserIdAsync(int userId);
    Task<Student?> CreateAsync(StudentCreateUpdateDto dto);
    Task<bool?> UpdateAsync(int id, StudentCreateUpdateDto dto);
    Task<StudentGetDto?> CompleteProfileAsync(int userId, StudentProfileUpdateDto dto);
    Task<bool> DeleteAsync(int id);
    Task<StudentImportResultDto> ImportFromCsvAsync(IFormFile csvFile);
}
