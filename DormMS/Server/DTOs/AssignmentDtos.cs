namespace DormMS.Server.DTOs;

using DormMS.Server.Models;

public class AssignmentRequestDto
{
    public int CompetitionId { get; set; }
}

public class StudentAssignmentDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentSurname { get; set; } = string.Empty;
    public string StudentIDNumber { get; set; } = string.Empty;
    public decimal AverageGrade { get; set; }
    public int ESPB { get; set; }
    public int YearsOfStudy { get; set; }
    public decimal CalculatedPoints { get; set; }
    public int? AssignedDormId { get; set; }
    public string? AssignedDormName { get; set; }
    public int? AssignedRoomId { get; set; }
    public AssignmentStatus Status { get; set; }
}

public class AssignmentResultDto
{
    public int CompetitionId { get; set; }
    public string CompetitionName { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int SuccessfullyAssigned { get; set; }
    public int Waitlisted { get; set; }
    public int Failed { get; set; }
    public List<StudentAssignmentDto> Assignments { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

public enum AssignmentStatus
{
    Assigned,
    Waitlisted,
    NotAssigned,
    Error
}
