namespace DormMS.Server.Models;

public class StudentFinancialStatus
{
    public int Id { get; set; }
    public int StudentId { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public Student Student { get; set; } = null!;

    public decimal TotalFamilyIncome { get; set; }
    public int FamilyMemberCount { get; set; }
    public decimal IncomePerMember => FamilyMemberCount > 0
        ? Math.Round(TotalFamilyIncome / FamilyMemberCount, 2)
        : 0;

    public FinancialVerificationStatus VerificationStatus { get; set; } = FinancialVerificationStatus.Pending;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? VerifiedAt { get; set; }
}

public enum FinancialVerificationStatus
{
    Pending = 0,
    Verified = 1,
    Rejected = 2
}