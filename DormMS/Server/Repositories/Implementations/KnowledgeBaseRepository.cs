using DormMS.Server.Models;
using DormMS.Server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace DormMS.Server.Repositories.Implementations;

public class KnowledgeBaseRepository : IKnowledgeBaseRepository
{
    private readonly AppDbContext _context;

    public KnowledgeBaseRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(KnowledgeBaseEntry entry)
    {
        _context.KnowledgeBaseEntries.Add(entry);
        await _context.SaveChangesAsync();
    }

    public async Task<KnowledgeBaseEntry?> FindClosestAsync(Vector queryVector)
    {
        return await _context.KnowledgeBaseEntries
            .OrderBy(e => e.Embedding.CosineDistance(queryVector))
            .FirstOrDefaultAsync();
    }
    public async Task<List<KnowledgeBaseEntry>> FindTopNAsync(Vector queryVector, int topN)
    {
        return await _context.KnowledgeBaseEntries
            .OrderBy(e => e.Embedding.CosineDistance(queryVector))
            .Take(topN)
            .ToListAsync();
    }

    public async Task<List<KnowledgeBaseEntry>> GetAllAsync()
    {
        return await _context.KnowledgeBaseEntries.ToListAsync();
    }
}