using Microsoft.EntityFrameworkCore;
using DormMS.Server.Repositories.Interfaces;
using DormMS.Server.Models;

namespace DormMS.Server.Repositories.Implementations;

public class DormitoryRoomRepository : IDormitoryRoomRepository
{
    private readonly AppDbContext _context;

    public DormitoryRoomRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DormitoryRoom>> GetAllAsync()
    {
        return await _context.DormitoryRooms.AsNoTracking().ToListAsync();
    }

    public async Task<DormitoryRoom?> GetByIdAsync(int id)
    {
        return await _context.DormitoryRooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<DormitoryRoom?> GetByIdTrackedAsync(int id)
    {
        return await _context.DormitoryRooms.FindAsync(id);
    }
    public async Task<DormitoryRoom?> GetByIdWithDormAndStudentsAsync(int roomId)
    {
        return await _context.DormitoryRooms
            .Include(r => r.Dorm)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == roomId);
    }

    public async Task AddAsync(DormitoryRoom room)
    {
        _context.DormitoryRooms.Add(room);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(DormitoryRoom room)
    {
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var room = await _context.DormitoryRooms.FindAsync(id);
        if (room is null)
            return false;

        _context.DormitoryRooms.Remove(room);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DormExistsAsync(int dormId)
    {
        return await _context.Dorms.AnyAsync(d => d.Id == dormId);
    }

    public async Task<bool> RoomNumberExistsAsync(int dormId, string roomNumber, int? excludeId = null)
    {
        var query = _context.DormitoryRooms
            .Where(r => r.DormId == dormId && r.RoomNumber == roomNumber);

        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<(List<DormitoryRoom> Rooms, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
    {
        var totalCount = await _context.DormitoryRooms.CountAsync();

        var rooms = await _context.DormitoryRooms
            .AsNoTracking()
            .OrderBy(r => r.DormId)
            .ThenBy(r => r.RoomNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (rooms, totalCount);
    }

    public async Task<List<Student>> GetStudentsByRoomIdsAsync(List<int> roomIds)
    {
        if (roomIds.Count == 0)
            return new List<Student>();

        return await _context.Students
            .AsNoTracking()
            .Where(s => s.DormitoryRoomId.HasValue && roomIds.Contains(s.DormitoryRoomId.Value))
            .ToListAsync();
    }

    public async Task<Dictionary<int, string>> GetDormNamesByIdsAsync(List<int> dormIds)
    {
        if (dormIds.Count == 0)
            return new Dictionary<int, string>();

        return await _context.Dorms
            .AsNoTracking()
            .Where(d => dormIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, d => d.Name);
    }
}
