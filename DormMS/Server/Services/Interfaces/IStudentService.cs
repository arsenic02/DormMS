using DormMS.Server.Models;
using DormMS.Server.DTOs;

namespace DormMS.Server.Services.Interfaces;

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
    Task<StudentFinancialStatusGetDto?> SubmitFinancialStatusAsync(
    int userId,
    StudentFinancialStatusCreateDto dto);
    Task<BulkFinancialStatusImportResultDto> BulkImportFinancialStatusAsync(IFormFile csvFile);
    Task<StudentGetDto?> VerifyProfileAsync(int studentId, StudentProfileStatus status);
}
