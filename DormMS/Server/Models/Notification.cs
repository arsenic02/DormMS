namespace DormMS.Server.Models;

public class Notification
{
    public int Id { get; set; }

    public int? StudentId { get; set; }
    public Student? Student { get; set; }

    public int? DormId { get; set; }
    public Dorm? Dorm { get; set; }

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; }
}