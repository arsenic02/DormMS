using DormMS.Server.DTOs;

namespace DormMS.Server.Services.Interfaces;

public interface IKnowledgeBaseService
{
    Task AddEntryAsync(string question, string answer);
    Task<ChatbotAnswerResult?> AskAsync(string userQuestion);
    Task<List<ChatbotAnswerResult>> AskAsync(string userQuestion, int topN = 2);
    
}