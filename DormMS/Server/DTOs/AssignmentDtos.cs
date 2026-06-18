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
    public decimal TotalFamilyIncome { get; set; }
    public int FamilyMemberCount { get; set; }
    public decimal IncomePerMember { get; set; }
    public int? AssignedDormId { get; set; }
    public string? AssignedDormName { get; set; }
    public int? AssignedRoomId { get; set; }
    public AssignmentStatus Status { get; set; }
    public string? FacultyName { get; set; } // dodato za grupisanje po fakultetima
}

// DTO za rangiranje brucosa
public class FreshmanAssignmentDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentSurname { get; set; } = string.Empty;
    public string StudentIDNumber { get; set; } = string.Empty;
    public decimal? HighSchoolAverageGrade { get; set; }
    public string? FacultyName { get; set; }
    public decimal CalculatedPoints { get; set; }
    public decimal HighSchoolPoints { get; set; } // bb = po * 8
    public decimal SocialBonus { get; set; } // 1 ili 0 bodova
    public decimal TotalFamilyIncome { get; set; }
    public int FamilyMemberCount { get; set; }
    public decimal IncomePerMember { get; set; }
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

// Result za rangiranje brucosa - grupisano po fakultetima
public class FreshmanRankingResultDto
{
    public int CompetitionId { get; set; }
    public string CompetitionName { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public Dictionary<string, List<FreshmanAssignmentDto>> RankingsByFaculty { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

// Result za rangiranje starijih studenata - grupisano po fakultetima
public class OlderStudentsRankingResultDto
{
    public int CompetitionId { get; set; }
    public string CompetitionName { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public Dictionary<string, List<StudentAssignmentDto>> RankingsByFaculty { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

public enum AssignmentStatus
{
    Assigned,
    Waitlisted,
    NotAssigned,
    Error
}
