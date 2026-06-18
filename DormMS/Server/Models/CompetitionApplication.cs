namespace DormMS.Server.Models;

public class CompetitionApplication
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public int CompetitionId { get; set; }
    public Competition Competition { get; set; } = null!;

    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public int? PriorityScore { get; set; }
    public string? Notes { get; set; }
    public string? DocumentUrl { get; set; } // putanja do PDF dokumenta (svedocanstvo, potvrda, itd.)
}

public enum ApplicationStatus
{
    Pending=0,
    Approved=1,
    Rejected=2,
    Waitlisted=3
}
