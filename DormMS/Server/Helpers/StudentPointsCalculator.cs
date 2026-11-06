namespace DormMS.Server.Helpers;

public static class StudentPointsCalculator
{
    /// <summary>Prosečna plata u Srbiji — osnova za socijalni bonus.</summary>
    private const decimal AverageSalary = 121650m;

    /// <summary>Granica prihoda po članu za socijalni bonus (50% prosečne plate).</summary>
    private const decimal SocialBonusIncomeThreshold = AverageSalary * 0.5m;

    // ---------- Stariji studenti ----------

    public static decimal CalculateOlderStudentPoints(
        decimal averageGrade,
        int espb,
        int yearsOfStudy,
        decimal incomePerMember)
    {
        if (yearsOfStudy <= 0)
            return 0;

        var academicPoints = (averageGrade * 5) + ((decimal)espb / yearsOfStudy * 0.8m);

        var espbBonus = espb switch
        {
            >= 120 and <= 240 => 1m,
            > 240 => 2m,
            _ => 0m
        };

        var financialBonus = HasSocialBonus(incomePerMember) ? 1m : 0m;

        return Math.Round(academicPoints + espbBonus + financialBonus, 2);
    }

    // ---------- Brucoši ----------

    public static (decimal HighSchoolPoints, decimal SocialBonus, decimal CalculatedPoints) CalculateFreshmanPoints(
        decimal highSchoolAverageGrade,
        decimal incomePerMember)
    {
        var rawHighSchoolPoints = highSchoolAverageGrade * 8m;
        var socialBonus = HasSocialBonus(incomePerMember) ? 1m : 0m;

        return (
            Math.Round(rawHighSchoolPoints, 2),
            socialBonus,
            Math.Round(rawHighSchoolPoints + socialBonus, 2));
    }

    private static bool HasSocialBonus(decimal incomePerMember) =>
        incomePerMember > 0 && incomePerMember < SocialBonusIncomeThreshold;
}
