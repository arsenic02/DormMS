namespace DormMS.Server.Repositories;

using DormMS.Server.Models;
using DormMS.Server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

public class MoveOutRequestRepository : IMoveOutRequestRepository
{
    private readonly AppDbContext _context;

    public MoveOutRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MoveOutRequest?> GetByIdAsync(int id)
    {
        return await _context.MoveOutRequests
            .AsNoTracking()
            .Include(r => r.Student)
            .Include(r => r.Room)
            .ThenInclude(r => r.Dorm)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<MoveOutRequest?> GetByIdTrackedAsync(int id)
    {
        return await _context.MoveOutRequests
            .Include(r => r.Student)
            .Include(r => r.Room)
            .ThenInclude(r => r.Dorm)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<MoveOutRequest?> GetPendingForStudentAsync(int studentId)
    {
        return await _context.MoveOutRequests
            .AsNoTracking()
            .Include(r => r.Student)
            .Include(r => r.Room)
            .ThenInclude(r => r.Dorm)
            .FirstOrDefaultAsync(r => r.StudentId == studentId && r.Status == MoveOutStatus.Pending);
    }

    public async Task<MoveOutRequest?> GetLatestForStudentAsync(int studentId)
    {
        return await _context.MoveOutRequests
            .AsNoTracking()
            .Include(r => r.Student)
            .Include(r => r.Room)
            .ThenInclude(r => r.Dorm)
            .Where(r => r.StudentId == studentId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<MoveOutRequest>> GetPendingForDormAsync(int dormId)
    {
        return await _context.MoveOutRequests
            .AsNoTracking()
            .Include(r => r.Student)
            .Include(r => r.Room)
            .ThenInclude(r => r.Dorm)
            .Where(r => r.Status == MoveOutStatus.Pending && r.Room.DormId == dormId)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(MoveOutRequest request)
    {
        await _context.MoveOutRequests.AddAsync(request);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(MoveOutRequest request)
    {
        _context.MoveOutRequests.Update(request);
        await _context.SaveChangesAsync();
    }
}
