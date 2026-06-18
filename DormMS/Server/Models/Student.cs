namespace DormMS.Server.Models;

public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string HomeAddress { get; set; } = string.Empty;

    public string MunicipalityOfResidence { get; set; } = string.Empty;
    public string JMBG { get; set; } = string.Empty;
    public string StudentIDNumber { get; set; } = string.Empty;//broj indeksa
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public Gender Gender { get; set; }
    public int YearOfEnrollment { get; set; }
    public StudyLevel StudyLevel { get; set; }
    public StudentStatus Status { get; set; }
    public int FacultyId { get; set; }
    public string? FacultyName { get; set; }
    //public int YearOfStudy { get; set; }
    public int ESPB { get; set; }
    public decimal? AverageGrade { get; set; } // prosečna ocena na fakultetu
    public decimal? HighSchoolAverageGrade { get; set; } // prosecna ocena iz srednje skole (za brucose)
    public bool IsBudgetStudent { get; set; }
    public int? DormitoryRoomId { get; set; } //FK
    public DormitoryRoom? DormitoryRoom { get; set; }

    public StudentProfileStatus ProfileStatus { get; set; } = StudentProfileStatus.Incomplete; 
    public StudentFinancialStatus? FinancialStatus { get; set; }

    //public bool IsProfileComplete { get; set; }

    public bool IsEligibleToApply => 
        ProfileStatus == StudentProfileStatus.Verified && 
        FinancialStatus != null && 
        FinancialStatus.IsVerified &&
        IsBudgetStudent;

    //bolje da se racuna nego da se cuva u bazi, jer se menja svake godine
    public int CurrentYearOfStudy => DateTime.Now.Year - YearOfEnrollment + 1;
    public bool IsFreshman => CurrentYearOfStudy == 1;
}

public enum StudyLevel
{
    Undergraduate=0,   
    Postgraduate=1,    
    Doctoral=2,         
    Integrated=3         
}

public enum StudentStatus
{
    Active=0,
    Inactive=1,
    Graduated=2,
}

public enum Gender
{
    Male=0,
    Female=1
}

public enum StudentProfileStatus
{
    Incomplete=0, // Tek registrovan
    PendingVerification=1,  // Popunio podatke, čeka verifikaciju od admina
    Verified=2, // Admin potvrdio podatke
    Rejected=3 // Admin odbio podatke, student treba da ih ispravi
}