namespace DormMS.Server.DTOs;

using DormMS.Server.Models;

public class CompetitionCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public CompetitionType Type { get; set; } = CompetitionType.General;
    public int? MaxStudents { get; set; }
}

public class CompetitionUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public CompetitionStatus Status { get; set; }
    public CompetitionType Type { get; set; } = CompetitionType.General;
    public int? MaxStudents { get; set; }   
}

public class CompetitionGetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public CompetitionStatus Status { get; set; }
    public CompetitionType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsOpen { get; set; }
    public int? CreatedByAdminId { get; set; }
    public int? CreatedByDormManagerId { get; set; }
    public int ApplicationCount { get; set; }
    public int? MaxStudents { get; set; }
    public bool HasApplied { get; set; } 
}

public class CompetitionApplicationCreateDto
{
    public int CompetitionId { get; set; }
    public string? Notes { get; set; }
    public string? DocumentUrl { get; set; }
}

public class CompetitionApplicationUpdateDto
{
    public ApplicationStatus Status { get; set; }
    public int? PriorityScore { get; set; }
    public string? Notes { get; set; }
    public string? DocumentUrl { get; set; }
}

public class CompetitionApplicationGetDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentSurname { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public string StudentIDNumber { get; set; } = string.Empty;
    public int CompetitionId { get; set; }
    public string CompetitionName { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
    public ApplicationStatus Status { get; set; }
    public int? PriorityScore { get; set; }
    public string? Notes { get; set; }
    public string? DocumentUrl { get; set; }
}

public class BulkApplicationDto
{
    public List<int>? StudentIds { get; set; }
    public int? StartId { get; set; }
    public int? EndId { get; set; }
    public int CompetitionId { get; set; }
}

public class BulkVerifyApplicationsDto
{
    public List<int>? ApplicationIds { get; set; }
    public int? StartId { get; set; }
    public int? EndId { get; set; }
    public ApplicationStatus Status { get; set; }
}

public class BulkVerifyApplicationsResultDto
{
    public int TotalProcessed { get; set; }
    public int SuccessfullyVerified { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();
}
