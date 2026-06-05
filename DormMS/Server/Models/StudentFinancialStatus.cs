public class StudentFinancialStatus
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public decimal TotalFamilyIncome { get; set; }   // Ukupna primanja porodice
    public int FamilyMemberCount { get; set; }        // Broj članova domaćinstva
    
    // Računa se automatski: TotalFamilyIncome / FamilyMemberCount
    public decimal IncomePerMember => FamilyMemberCount > 0 
        ? TotalFamilyIncome / FamilyMemberCount 
        : 0;

    // public bool IsVerified { get; set; } = false;     // Admin/referent verifikuje
    // public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    // public DateTime? VerifiedAt { get; set; }
    // public int? VerifiedByAdminId { get; set; }
    // public Admin? VerifiedByAdmin { get; set; }
}