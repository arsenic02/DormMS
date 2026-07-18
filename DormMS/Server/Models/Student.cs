namespace DormMS.Server.Models;

public class Student
{
    public int Id { get; set; }
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
    public int FacultyId { get; set; }
    public string? FacultyName { get; set; }
    public int ESPB { get; set; }
    public decimal? AverageGrade { get; set; } 
    public decimal? HighSchoolAverageGrade { get; set; } 
    public bool IsBudgetStudent { get; set; }
    public int? DormitoryRoomId { get; set; } //FK
    public DormitoryRoom? DormitoryRoom { get; set; }

    public StudentProfileStatus ProfileStatus { get; set; } = StudentProfileStatus.Incomplete;
    public StudentFinancialStatus? FinancialStatus { get; set; }

    public bool IsEligibleToApply =>
        ProfileStatus != StudentProfileStatus.Incomplete &&
        FinancialStatus != null &&
        IsBudgetStudent &&
        DormitoryRoomId == null;

    public int CurrentYearOfStudy => DateTime.Now.Year - YearOfEnrollment + 1;
    public bool IsFreshman => CurrentYearOfStudy == 1;

    public ICollection<RoomSwapRequest> SentSwapRequests { get; set; }
    = new List<RoomSwapRequest>();

    public ICollection<RoomSwapRequest> ReceivedSwapRequests { get; set; }
    = new List<RoomSwapRequest>();

}

public enum StudyLevel
{
    Undergraduate = 0,
    Postgraduate = 1,
    Doctoral = 2,
    Integrated = 3
}

public enum StudentStatus
{
    Active = 0,
    Inactive = 1,
    Graduated = 2,
}

public enum Gender
{
    Male = 0,
    Female = 1
}

public enum StudentProfileStatus
{
    Incomplete = 0, 
    PendingVerification = 1,  
    Verified = 2, 
    Rejected = 3 
}