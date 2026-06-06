using Microsoft.EntityFrameworkCore;
using DormMS.Server.Repositories.Interfaces;
using DormMS.Server.Models;

namespace DormMS.Server.Repositories.Implementations;

public class CompetitionApplicationRepository : ICompetitionApplicationRepository
{
    private readonly AppDbContext _context;

    public CompetitionApplicationRepository(AppDbContext context)
    {
        _context = context;
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

    public async Task<IEnumerable<CompetitionApplication>> GetAllAsync()
    {
        return await _context.CompetitionApplications
            .Include(a => a.Student)
            .Include(a => a.Competition)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<CompetitionApplication>> GetByStatusAsync(ApplicationStatus status)
    {
        return await _context.CompetitionApplications
            .Include(a => a.Student)
            .Include(a => a.Competition)
            .Where(a => a.Status == status)
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

    public async Task AddAsync(CompetitionApplication application)
    {
        _context.CompetitionApplications.Add(application);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(CompetitionApplication application)
    {
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var application = await _context.CompetitionApplications.FindAsync(id);
        if (application is null)
            return false;

        _context.CompetitionApplications.Remove(application);
        await _context.SaveChangesAsync();
        return true;
    }
}
