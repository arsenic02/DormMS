namespace DormMS.Server.Models;

public class Competition
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public CompetitionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? CreatedByAdminId { get; set; }
    public Admin? CreatedByAdmin { get; set; }

    public int? CreatedByDormManagerId { get; set; }
    public DormManager? CreatedByDormManager { get; set; }

    public List<CompetitionApplication> Applications { get; set; } = new();

    public bool IsOpen => Status == CompetitionStatus.Open && DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
    public bool IsClosed => Status == CompetitionStatus.Closed || DateTime.UtcNow > EndDate;
}

public enum CompetitionStatus
{
    Draft=0,
    Open=1,
    Closed=2,
    Completed=3
}
