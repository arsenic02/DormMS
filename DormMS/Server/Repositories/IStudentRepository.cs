public interface IStudentRepository
{

    Task<IEnumerable<Student>> GetAllAsync();
    Task<Student?> GetByIdAsync(int id);
    Task<Student?> GetByUserIdAsync(int userId);
    Task<Student?> GetByIdTrackedAsync(int id);
    Task<Student?> GetByIdTrackedByUserIdAsync(int userId);
    Task<Student?> GetByIdWithFinancialStatusAsync(int id);
    Task<StudentFinancialStatus?> GetFinancialStatusByStudentIdAsync(int studentId);
    Task AddFinancialStatusAsync(StudentFinancialStatus financialStatus);
    Task AddAsync(Student student);
    Task UpdateAsync(Student student);
    Task<bool> DeleteAsync(int id);
    Task<bool> DormitoryRoomExistsAsync(int roomId);
}
