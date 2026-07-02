namespace DormMS.Server.DTOs;

using DormMS.Server.Models;

public class StudentCreateUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string HomeAddress { get; set; } = string.Empty;
    public string MunicipalityOfResidence { get; set; } = string.Empty;
    public string JMBG { get; set; } = string.Empty;
    public string StudentIDNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public Gender Gender { get; set; }
    public int YearOfEnrollment { get; set; }
    public StudyLevel StudyLevel { get; set; }
    public StudentStatus Status { get; set; }
    public string FacultyName { get; set; } = string.Empty;
    public int ESPB { get; set; }
    public decimal? AverageGrade { get; set; }
    public decimal? HighSchoolAverageGrade { get; set; }
    public bool IsBudgetStudent { get; set; }
    public int? DormitoryRoomId { get; set; }
}

public class StudentProfileUpdateDto
{
    public string HomeAddress { get; set; } = string.Empty;
    public string MunicipalityOfResidence { get; set; } = string.Empty;
    public string JMBG { get; set; } = string.Empty;
    public int Age { get; set; }
    public Gender Gender { get; set; }
    public int YearOfEnrollment { get; set; }
    public StudyLevel StudyLevel { get; set; }
    public string FacultyName { get; set; } = string.Empty;
    public int ESPB { get; set; }
    public decimal? AverageGrade { get; set; }
    public decimal? HighSchoolAverageGrade { get; set; }
    public bool IsBudgetStudent { get; set; }
    public int? DormitoryRoomId { get; set; }
}

public class StudentFinancialStatusCreateDto
{
    public decimal TotalFamilyIncome { get; set; }
    public int FamilyMemberCount { get; set; }
}

public class StudentFinancialStatusGetDto
{
    public decimal TotalFamilyIncome { get; set; }
    public int FamilyMemberCount { get; set; }
    public decimal IncomePerMember { get; set; }
    public FinancialVerificationStatus VerificationStatus { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
}

public class StudentGetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string JMBG { get; set; } = string.Empty;
    public string StudentIDNumber { get; set; } = string.Empty;
    public string HomeAddress { get; set; } = string.Empty;
    public string MunicipalityOfResidence { get; set; } = string.Empty;
    public int Age { get; set; }
    public Gender Gender { get; set; }
    public int YearOfEnrollment { get; set; }
    public StudyLevel StudyLevel { get; set; }
    public StudentStatus Status { get; set; }
    public int FacultyId { get; set; }
    public string? FacultyName { get; set; }
    public int ESPB { get; set; }
    public decimal? AverageGrade { get; set; }
    public decimal? HighSchoolAverageGrade { get; set; }
    public bool IsBudgetStudent { get; set; }
    public int? DormitoryRoomId { get; set; }
    public int CurrentYearOfStudy { get; set; }
    public bool IsProfileComplete { get; set; }
    public bool IsFreshman { get; set; }

    public StudentProfileStatus ProfileStatus { get; set; }
    public bool IsEligibleToApply { get; set; }
    public StudentFinancialStatusGetDto? FinancialStatus { get; set; }
}

public class StudentCsvImportDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public string StudentIDNumber { get; set; } = string.Empty;
    public decimal TotalFamilyIncome { get; set; }
    public int FamilyMemberCount { get; set; }
    public int ESPB { get; set; }
    public decimal? AverageGrade { get; set; }
    public int YearOfEnrollment { get; set; }
    public decimal? HighSchoolAverageGrade { get; set; }
    public string FacultyName { get; set; } = string.Empty;
}

public class StudentImportResultDto
{
    public int TotalProcessed { get; set; }
    public int SuccessfullyImported { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class BulkVerifyProfilesDto
{
    public List<int>? StudentIds { get; set; }
    public int? StartId { get; set; }
    public int? EndId { get; set; }
    public StudentProfileStatus Status { get; set; }
}

public record StudentProfileVerificationDto(StudentProfileStatus Status); // prima Verified ili Rejected

public class BulkFinancialStatusImportDto
{
    public string StudentIDNumber { get; set; } = string.Empty;
    public decimal TotalFamilyIncome { get; set; }
    public int FamilyMemberCount { get; set; }
}

public class BulkFinancialStatusImportResultDto
{
    public int TotalProcessed { get; set; }
    public int SuccessfullyImported { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class StudentSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string? FacultyName { get; set; }
    public bool IsFreshman { get; set; }
}

// public class ApplyByFreshmanStatusDto
// {
//     public int CompetitionId { get; set; }
//     public bool IsFreshman { get; set; }
// }