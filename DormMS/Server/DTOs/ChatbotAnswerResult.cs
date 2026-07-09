namespace DormMS.Server.DTOs;

public class ChatbotAnswerResult
{
    public string Question { get; set; } = null!;
    public string Answer { get; set; } = null!;
    public double Distance { get; set; }
}