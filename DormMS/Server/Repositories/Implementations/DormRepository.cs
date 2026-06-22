using Microsoft.EntityFrameworkCore;
using DormMS.Server.Repositories.Interfaces;
using DormMS.Server.Models;

namespace DormMS.Server.Repositories.Implementations;

public class DormRepository : IDormRepository
{
    private readonly AppDbContext _context;

    public DormRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Dorm>> GetAllAsync()
    {
        return await _context.Dorms
        .Include(d => d.Rooms)
        .AsNoTracking()
        .ToListAsync();
    }

    public async Task<Dorm?> GetByIdAsync(int id)
    {
        return await _context.Dorms
        .Include(d => d.Rooms)
        .AsNoTracking()
        .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Dorm?> GetByIdTrackedAsync(int id)
    {
        return await _context.Dorms.FindAsync(id);
    }

    public async Task AddAsync(Dorm dorm)
    {
        _context.Dorms.Add(dorm);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Dorm dorm)
    {
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var dorm = await _context.Dorms.FindAsync(id);
        if (dorm is null)
            return false;

        _context.Dorms.Remove(dorm);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Dorm?> GetByIdWithRoomsAsync(int id)
    {
        return await _context.Dorms
            .Include(d => d.Rooms)
            .FirstOrDefaultAsync(d => d.Id == id);
    }
    public async Task AddRoomsAsync(IEnumerable<DormitoryRoom> rooms)
    {
        await _context.DormitoryRooms.AddRangeAsync(rooms);
        await _context.SaveChangesAsync();
    }

    public async Task ClearStudentRoomAssignmentsAsync(List<int> roomIds)
    {
        var students = await _context.Students
            .Where(s => s.DormitoryRoomId != null && roomIds.Contains(s.DormitoryRoomId.Value))
            .ToListAsync();

        foreach (var student in students)
        {
            student.DormitoryRoomId = null;
        }

        await _context.SaveChangesAsync();
    }
    public async Task<int> DeleteRoomsByIdsAsync(List<int> roomIds)
    {
        var rooms = await _context.DormitoryRooms
            .Where(r => roomIds.Contains(r.Id))
            .ToListAsync();

        _context.DormitoryRooms.RemoveRange(rooms);
        await _context.SaveChangesAsync();

        return rooms.Count;
    }
}
