using DormMS.Server.DTOs;

namespace DormMS.Server.Services.Interfaces;

public interface IKnowledgeBaseService
{
    Task AddEntryAsync(string question, string answer);
    //Task<KnowledgeBaseEntry?> FindClosestMatchAsync(string userQuestion);
    Task<ChatbotAnswerResult?> AskAsync(string userQuestion);
}