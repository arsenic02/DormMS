using DormMS.Server.DTOs;

namespace DormMS.Server.Models;

// Trajno sacuvan rezultat rangiranja za jednog studenta u okviru konkursa.
// Pravi se jednom kada se pokrene rangiranje (RankOlderStudentsAsync / RankFreshmenAsync)
// i kasnije se cita preko GET endpointa, bez ponovnog izvrsavanja algoritma.
public class RankingResult
{
    public int Id { get; set; }

    public int CompetitionId { get; set; }
    public Competition Competition { get; set; } = null!;

    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public string FacultyName { get; set; } = "Unknown";
    public bool IsFreshman { get; set; }

    public decimal CalculatedPoints { get; set; }
    public AssignmentStatus Status { get; set; } = AssignmentStatus.NotAssigned;

    // Polja za starije studente (null za brucose)
    public decimal? AverageGrade { get; set; }
    public int? ESPB { get; set; }
    public int? YearsOfStudy { get; set; }

    public decimal? HighSchoolAverageGrade { get; set; }
    public decimal? HighSchoolPoints { get; set; }
    public decimal? SocialBonus { get; set; }

    public decimal TotalFamilyIncome { get; set; }
    public int FamilyMemberCount { get; set; }
    public decimal IncomePerMember { get; set; }

    public DateTime RankedAt { get; set; } = DateTime.UtcNow;
}