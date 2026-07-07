namespace DormMS.Server.Models;

public class Competition
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public CompetitionStatus Status { get; set; }
    public CompetitionType Type { get; set; } = CompetitionType.General;
    public DateTime CreatedAt { get; set; } = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

    public int? CreatedByAdminId { get; set; }
    public Admin? CreatedByAdmin { get; set; }

    public int? CreatedByDormManagerId { get; set; }
    public DormManager? CreatedByDormManager { get; set; }

    public List<CompetitionApplication> Applications { get; set; } = new();
    public int? MaxStudents{get;set;}

    public bool IsOpen => Status == CompetitionStatus.Open && DateTime.Now >= StartDate && DateTime.Now <= EndDate;
    public bool IsClosed => Status == CompetitionStatus.Closed || DateTime.Now > EndDate;
}

public enum CompetitionStatus
{
    Draft=0,
    Open=1,
    Closed=2,
    Completed=3
}

public enum CompetitionType
{
    General=0,    
    Freshmen=1    
}
