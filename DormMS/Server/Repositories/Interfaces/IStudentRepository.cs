using DormMS.Server.DTOs;
using DormMS.Server.Models;

namespace DormMS.Server.Repositories.Interfaces;

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
    //Task BulkAddFinancialStatusesAsync(List<StudentFinancialStatus> financialStatuses);
    Task AddAsync(Student student);
    Task UpdateAsync(Student student);
    Task UpdateProfileAsync(Student student);
    //Task<bool> DeleteAsync(int id);
    Task<bool> DormitoryRoomExistsAsync(int roomId);
    Task<Student?> GetByStudentIdNumberAsync(string studentIdNumber);
    Task<List<Student>> SearchForRoomSwapAsync(string? idNumber,
    string? name, string? surname, string? faculty, string? room, int excludeStudentId);

   Task<PagedResultDto<StudentGetDto>> GetPagedAsync(
    int pageNumber,
    int pageSize,
    StudentProfileStatus? profileStatusFilter,
    string? facultyFilter,
    bool? isFreshmanFilter);
}
