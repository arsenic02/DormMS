public class StudentCreateUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string HomeAddress { get; set; } = string.Empty;
    public string StudentIDNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public Gender Gender { get; set; }
    public int YearOfEnrollment { get; set; }
    public StudyLevel StudyLevel { get; set; }
    public StudentStatus Status { get; set; }
    public int FacultyId { get; set; }
    public int ESPB { get; set; }
    public bool IsBudgetStudent { get; set; }
    public int? DormitoryRoomId { get; set; }
}

public class StudentProfileUpdateDto
{
    public string HomeAddress { get; set; } = string.Empty;
    public int Age { get; set; }
    public Gender Gender { get; set; }
    public int YearOfEnrollment { get; set; }
    public StudyLevel StudyLevel { get; set; }
    public int FacultyId { get; set; }
    public int ESPB { get; set; }
    public bool IsBudgetStudent { get; set; }
    public int? DormitoryRoomId { get; set; }
}

public class StudentGetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string StudentIDNumber { get; set; } = string.Empty;
    public string HomeAddress { get; set; } = string.Empty;
    public int Age { get; set; }
    public Gender Gender { get; set; }
    public int YearOfEnrollment { get; set; }
    public StudyLevel StudyLevel { get; set; }
    public StudentStatus Status { get; set; }
    public int FacultyId { get; set; }
    public int ESPB { get; set; }
    public bool IsBudgetStudent { get; set; }
    public int? DormitoryRoomId { get; set; }
    public int CurrentYearOfStudy { get; set; }
    public bool IsProfileComplete { get; set; }
}
