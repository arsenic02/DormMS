using DormMS.Server.Models;
using DormMS.Server.DTOs;
using DormMS.Server.Repositories.Interfaces;
using DormMS.Server.Services.Interfaces;
using Pgvector;

namespace DormMS.Server.Services.Implementations;

public class KnowledgeBaseService : IKnowledgeBaseService
{
    private const double SimilarityThreshold = 0.5;

    private readonly IKnowledgeBaseRepository _repository;
    private readonly IEmbeddingService _embeddingService;

    public KnowledgeBaseService(IKnowledgeBaseRepository repository, IEmbeddingService embeddingService)
    {
        _repository = repository;
        _embeddingService = embeddingService;
    }

    public async Task AddEntryAsync(string question, string answer)
    {
        var embeddingArray = await _embeddingService.GenerateEmbeddingAsync(question);

        var entry = new KnowledgeBaseEntry
        {
            Question = question,
            Answer = answer,
            Embedding = new Vector(embeddingArray)
        };

        await _repository.AddAsync(entry);
    }

    public async Task<ChatbotAnswerResult?> AskAsync(string userQuestion)
    {
        var queryEmbeddingArray = await _embeddingService.GenerateEmbeddingAsync(userQuestion);
        var queryVector = new Vector(queryEmbeddingArray);

        var closest = await _repository.FindClosestAsync(queryVector);
        if (closest is null)
            return null;

        var similarity = CosineSimilarity(queryEmbeddingArray, closest.Embedding.ToArray());

        if (similarity < SimilarityThreshold)
            return null;

        return new ChatbotAnswerResult
        {
            Question = closest.Question,
            Answer = closest.Answer,
            Similarity = similarity
        };
    }

    public async Task<List<ChatbotAnswerResult>> AskAsync(string userQuestion, int topN = 2)
    {
        var queryEmbeddingArray = await _embeddingService.GenerateEmbeddingAsync(userQuestion);
        var queryVector = new Vector(queryEmbeddingArray);

        var candidates = await _repository.FindTopNAsync(queryVector, topN);

        return candidates.Select(entry => new ChatbotAnswerResult
        {
            Question = entry.Question,
            Answer = entry.Answer,
            Similarity = CosineSimilarity(queryEmbeddingArray, entry.Embedding.ToArray())
        })
        .Where(result => result.Similarity >= SimilarityThreshold)
        .ToList();
    }

    private static double CosineSimilarity(float[] a, float[] b)
    {
        double dot = 0, normA = 0, normB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }
        return dot / (Math.Sqrt(normA) * Math.Sqrt(normB)); // 1 - cosine distance
    }
}

