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
        //var distance = CosineDistance(queryEmbeddingArray, closest.Embedding.ToArray());

        // if (distance > SimilarityThreshold)
        //     return null; // nema dovoljno sličnog pitanja u bazi

        return new ChatbotAnswerResult
        {
            Question = closest.Question,
            Answer = closest.Answer,
            Similarity = similarity
        };
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

    // private static double CosineDistance(float[] a, float[] b)
    // {
    //     double dot = 0, normA = 0, normB = 0;
    //     for (int i = 0; i < a.Length; i++)
    //     {
    //         dot += a[i] * b[i];
    //         normA += a[i] * a[i];
    //         normB += b[i] * b[i];
    //     }
    //     return 1 - (dot / (Math.Sqrt(normA) * Math.Sqrt(normB)));
    // }


}

// using DormMS.Server.Models;
// using DormMS.Server.Services.Interfaces;
// using Microsoft.EntityFrameworkCore;
// using Pgvector;
// using Pgvector.EntityFrameworkCore;

// namespace DormMS.Server.Services.Implementations;

// public class KnowledgeBaseService : IKnowledgeBaseService
// {
//     private readonly AppDbContext _context;
//     private readonly IEmbeddingService _embeddingService;

//     public KnowledgeBaseService(AppDbContext context, IEmbeddingService embeddingService)
//     {
//         _context = context;
//         _embeddingService = embeddingService;
//     }

//     public async Task AddEntryAsync(string question, string answer)
//     {
//         var embeddingArray = await _embeddingService.GenerateEmbeddingAsync(question);

//         var entry = new KnowledgeBaseEntry
//         {
//             Question = question,
//             Answer = answer,
//             Embedding = new Vector(embeddingArray)
//         };

//         _context.KnowledgeBaseEntries.Add(entry);
//         await _context.SaveChangesAsync();
//     }

//     public async Task<KnowledgeBaseEntry?> FindClosestMatchAsync(string userQuestion)
//     {
//         var queryEmbeddingArray = await _embeddingService.GenerateEmbeddingAsync(userQuestion);
//         var queryVector = new Vector(queryEmbeddingArray);

//         return await _context.KnowledgeBaseEntries
//             .OrderBy(e => e.Embedding.CosineDistance(queryVector))
//             .FirstOrDefaultAsync();
//     }
// }