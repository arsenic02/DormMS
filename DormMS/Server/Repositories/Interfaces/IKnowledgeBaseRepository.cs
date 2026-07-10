using Pgvector;

namespace DormMS.Server.Repositories.Interfaces;

public interface IKnowledgeBaseRepository
{
    Task AddAsync(KnowledgeBaseEntry entry);
    Task<KnowledgeBaseEntry?> FindClosestAsync(Vector queryVector);
    Task<List<KnowledgeBaseEntry>> FindTopNAsync(Vector queryVector, int topN);
    Task<List<KnowledgeBaseEntry>> GetAllAsync();
}