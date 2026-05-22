public interface IStudentRepository
{
    Task<IEnumerable<Student>> GetAllAsync();
    Task<Student?> GetByIdAsync(int id);
    Task<Student?> GetByIdTrackedAsync(int id);
    Task AddAsync(Student student);
    Task UpdateAsync(Student student);
    Task<bool> DeleteAsync(int id);
    Task<bool> DormitoryRoomExistsAsync(int roomId);
}
