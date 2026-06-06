using Microsoft.EntityFrameworkCore;
using DormMS.Server.Repositories.Interfaces;

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
        var now = DateTime.UtcNow;
        return await _context.Competitions
            .Where(c => c.Status == CompetitionStatus.Open 
                && c.StartDate <= now 
                && c.EndDate >= now)
            .Include(c => c.Applications)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<CompetitionApplication>> GetApplicationsByCompetitionIdAsync(int competitionId)
    {
        return await _context.CompetitionApplications
            .Include(a => a.Student)
            .Include(a => a.Competition)
            .Where(a => a.CompetitionId == competitionId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<CompetitionApplication?> GetApplicationByIdAsync(int id)
    {
        return await _context.CompetitionApplications
            .Include(a => a.Student)
            .Include(a => a.Competition)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<CompetitionApplication?> GetApplicationByStudentAndCompetitionAsync(int studentId, int competitionId)
    {
        return await _context.CompetitionApplications
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.StudentId == studentId && a.CompetitionId == competitionId);
    }

    public async Task AddAsync(Competition competition)
    {
        _context.Competitions.Add(competition);
        await _context.SaveChangesAsync();
    }

    public async Task AddApplicationAsync(CompetitionApplication application)
    {
        _context.CompetitionApplications.Add(application);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Competition competition)
    {
        await _context.SaveChangesAsync();
    }

    public async Task UpdateApplicationAsync(CompetitionApplication application)
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

    public async Task<bool> DeleteApplicationAsync(int id)
    {
        var application = await _context.CompetitionApplications.FindAsync(id);
        if (application is null)
            return false;

        _context.CompetitionApplications.Remove(application);
        await _context.SaveChangesAsync();
        return true;
    }
}
