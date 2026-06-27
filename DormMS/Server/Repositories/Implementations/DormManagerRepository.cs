namespace DormMS.Server.Repositories.Implementations;

using DormMS.Server.Models;
using DormMS.Server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

public class DormManagerRepository : IDormManagerRepository
{
    private readonly AppDbContext _context;

    public DormManagerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DormManager?> GetByUserIdAsync(int userId)
    {
        return await _context.DormManagers
            .AsNoTracking()
            .FirstOrDefaultAsync(dm => _context.Users
                .Any(u => u.Id == userId && u.DormManagerId == dm.Id));
    }
}