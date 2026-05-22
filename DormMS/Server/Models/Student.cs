public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string HomeAddress { get; set; } = string.Empty;
    public string StudentIDNumber { get; set; } = string.Empty;//broj indeksa
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public Gender Gender { get; set; }
    public int YearOfEnrollment { get; set; }
    public StudyLevel StudyLevel { get; set; }
    public StudentStatus Status { get; set; }
    public int FacultyId { get; set; }
    //public int YearOfStudy { get; set; }
    public int ESPB { get; set; }
    public bool IsBudgetStudent { get; set; }
    public int? DormitoryRoomId { get; set; } //FK
    public DormitoryRoom? DormitoryRoom { get; set; } 

    //bolje da se racuna nego da se cuva u bazi, jer se menja svake godine
    public int CurrentYearOfStudy => DateTime.Now.Year - YearOfEnrollment + 1;
    public bool IsFreshman => CurrentYearOfStudy == 1;
}

public enum StudyLevel
{
    Undergraduate,   // Osnovne studije
    Postgraduate,    // Master studije
    Doctoral         // Doktorske studije
}

public enum StudentStatus
{
    Active,
    Inactive,
    Graduated,
    //Suspended
}

public enum Gender
{
    Male,
    Female
}