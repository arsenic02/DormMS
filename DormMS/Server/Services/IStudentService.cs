public interface IStudentService
{
    Task<IEnumerable<Student>> GetAllAsync();
    Task<Student?> GetByIdAsync(int id);
    Task<Student?> CreateAsync(StudentCreateUpdateDto dto);
    Task<bool?> UpdateAsync(int id, StudentCreateUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
