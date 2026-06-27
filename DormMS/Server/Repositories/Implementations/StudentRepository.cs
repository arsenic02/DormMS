using Microsoft.EntityFrameworkCore;
using DormMS.Server.Repositories.Interfaces;
using DormMS.Server.Models;

namespace DormMS.Server.Repositories.Implementations;

public class StudentRepository : IStudentRepository
{
    private readonly AppDbContext _context;

    public StudentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Student>> GetAllAsync()
    {
        return await _context.Students
        .Include(s => s.FinancialStatus)
        .AsNoTracking()
        .ToListAsync();
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        return await _context.Students
            .Include(s => s.FinancialStatus)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Student?> GetByUserIdAsync(int userId)
    {
        return await _context.Students
            .Include(s => s.FinancialStatus)
            .FirstOrDefaultAsync(s => _context.Users
                .Any(u => u.Id == userId && u.StudentId == s.Id));
    }

    public async Task<Student?> GetByIdTrackedAsync(int id)
    {
        return await _context.Students.FindAsync(id);
    }

    public async Task<Student?> GetByIdTrackedByUserIdAsync(int userId)
    {
        var studentId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.StudentId)
            .FirstOrDefaultAsync();

        if (!studentId.HasValue)
            return null;

        return await GetByIdTrackedAsync(studentId.Value);
    }

    public async Task<Student?> GetByIdWithFinancialStatusAsync(int id)
    {
        return await _context.Students
            .Include(s => s.FinancialStatus)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<StudentFinancialStatus?> GetFinancialStatusByStudentIdAsync(int studentId)
    {
        return await _context.StudentFinancialStatuses
            .FirstOrDefaultAsync(x => x.StudentId == studentId);
    }

    public async Task AddFinancialStatusAsync(StudentFinancialStatus financialStatus)
    {
        _context.StudentFinancialStatuses.Add(financialStatus);
        await _context.SaveChangesAsync();
    }

    public async Task BulkAddFinancialStatusesAsync(List<StudentFinancialStatus> financialStatuses)
    {
        await _context.StudentFinancialStatuses.AddRangeAsync(financialStatuses);
        await _context.SaveChangesAsync();
    }

    public async Task AddAsync(Student student)
    {
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Student student)
    {
        _context.Students.Update(student);//mozda bude problem u nekim metodama, ali 99% ne bi trebalo
        await _context.SaveChangesAsync();
    }
    public async Task UpdateProfileAsync(Student student)
    {
        _context.Students.Update(student);
        await _context.SaveChangesAsync();
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student is null)
            return false;

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DormitoryRoomExistsAsync(int roomId)
    {
        return await _context.DormitoryRooms.AnyAsync(r => r.Id == roomId);
    }

    public async Task<Student?> GetByStudentIdNumberAsync(string studentIdNumber)
    {
        return await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.StudentIDNumber == studentIdNumber);
    }

}
