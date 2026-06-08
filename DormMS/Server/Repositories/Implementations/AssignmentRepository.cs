using Microsoft.EntityFrameworkCore;
using DormMS.Server.Repositories.Interfaces;
using DormMS.Server.Models;

namespace DormMS.Server.Repositories.Implementations;

public class AssignmentRepository : IAssignmentRepository
{
    private readonly AppDbContext _context;

    public AssignmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Competition?> GetCompetitionWithApplicationsAsync(int competitionId)
    {
        return await _context.Competitions
            .Include(c => c.Applications)
                .ThenInclude(a => a.Student)
                    .ThenInclude(s => s.FinancialStatus)
            .FirstOrDefaultAsync(c => c.Id == competitionId);
    }

    public async Task<Dorm?> GetDormWithRoomsAsync(int dormId)
    {
        return await _context.Dorms
            .Include(d => d.Rooms)
            .FirstOrDefaultAsync(d => d.Id == dormId);
    }

    public async Task<List<Dorm>> GetAllDormsWithRoomsAsync()
    {
        return await _context.Dorms
            .Include(d => d.Rooms)
            .ToListAsync();
    }

    public async Task<DormitoryRoom?> GetAvailableRoomAsync(int dormId)
    {
        return await _context.DormitoryRooms
            .Where(r => r.DormId == dormId && r.CurrentOccupancy < r.Capacity)
            .OrderBy(r => r.CurrentOccupancy)
            .FirstOrDefaultAsync();
    }

    public async Task AssignStudentToRoomAsync(int studentId, int roomId)
    {
        var student = await _context.Students.FindAsync(studentId);
        if (student != null)
        {
            student.DormitoryRoomId = roomId;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateApplicationStatusAsync(int applicationId, ApplicationStatus status)
    {
        var application = await _context.CompetitionApplications.FindAsync(applicationId);
        if (application != null)
        {
            application.Status = status;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int?> GetStudentCurrentRoomIdAsync(int studentId)
    {
        return await _context.Students
            .Where(s => s.Id == studentId)
            .Select(s => s.DormitoryRoomId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Student>> GetStudentsByIdsAsync(List<int> studentIds)
    {
        return await _context.Students
            .Where(s => studentIds.Contains(s.Id))
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
