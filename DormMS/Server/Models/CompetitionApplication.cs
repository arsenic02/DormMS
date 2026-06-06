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
}

public enum ApplicationStatus
{
    Pending,
    Approved,
    Rejected,
    Waitlisted
}
