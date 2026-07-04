using Microsoft.EntityFrameworkCore;
using DormMS.Server.Repositories.Interfaces;
using DormMS.Server.Models;

namespace DormMS.Server.Repositories.Implementations;

public class CompetitionRepository : ICompetitionRepository
{
    private readonly AppDbContext _context;

    public CompetitionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Competition>> GetAllAsync()
    {
        return await _context.Competitions
            .Include(c => c.Applications)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<(List<Competition> Competitions, int TotalCount)> GetPagedAsync(
    int pageNumber, int pageSize, CompetitionType? type, CompetitionStatus? status)
    {
        var query = _context.Competitions.AsQueryable();

        if (type.HasValue)
            query = query.Where(c => c.Type == type.Value);

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        var totalCount = await query.CountAsync();

        var competitions = await query
            .Include(c => c.Applications)
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (competitions, totalCount);
    }

    public async Task<Competition?> GetByIdAsync(int id)
    {
        return await _context.Competitions
            .Include(c => c.Applications)
            .ThenInclude(a => a.Student)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Competition?> GetByIdTrackedAsync(int id)
    {
        return await _context.Competitions
            .Include(c => c.Applications)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Competition>> GetOpenCompetitionsAsync()
    {
        // var now = DateTime.UtcNow;
        var now=DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
        return await _context.Competitions
            .Where(c => c.Status == CompetitionStatus.Open
                && c.StartDate <= now
                && c.EndDate >= now)
            .Include(c => c.Applications)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Competition>> GetOpenAndAppliedCompetitionsAsync(int studentId, bool isFreshman)
    {
       // var now = DateTime.UtcNow;
        var now=DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
        var studentCompetitionType = isFreshman ? CompetitionType.Freshmen : CompetitionType.General;

        return await _context.Competitions
            .Where(c =>
                (c.Status == CompetitionStatus.Open
                    && c.StartDate <= now
                    && c.EndDate >= now
                    && c.Type == studentCompetitionType)
                || c.Applications.Any(a => a.StudentId == studentId))
            .Include(c => c.Applications)
            .AsNoTracking()
            .OrderByDescending(c => c.StartDate)
            .ToListAsync();
    }

    public async Task AddAsync(Competition competition)
    {
        _context.Competitions.Add(competition);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Competition competition)
    {
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var competition = await _context.Competitions.FindAsync(id);
        if (competition is null)
            return false;

        _context.Competitions.Remove(competition);
        await _context.SaveChangesAsync();
        return true;
    }
}
