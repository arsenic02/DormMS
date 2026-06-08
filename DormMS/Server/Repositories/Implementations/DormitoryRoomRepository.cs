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
}
