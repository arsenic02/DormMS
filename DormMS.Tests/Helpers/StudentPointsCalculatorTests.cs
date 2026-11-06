using DormMS.Server.Helpers;
using FluentAssertions;

namespace DormMS.Tests.Helpers;

public class StudentPointsCalculatorTests
{
    // 50% prosečne plate (121650 * 0.5) = 60825 — granica za socijalni bonus.
    private const decimal IncomeThreshold = 60825m;

    // =========================================================
    //  Stariji studenti
    // =========================================================

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void OlderPoints_WhenYearsOfStudyIsNotPositive_ReturnsZero(int yearsOfStudy)
    {
        // Čak i uz maksimalne ostale vrednosti, bez validne godine studija nema bodova.
        var result = StudentPointsCalculator.CalculateOlderStudentPoints(
            averageGrade: 10m, espb: 300, yearsOfStudy, incomePerMember: 0m);

        result.Should().Be(0m);
    }

    [Fact]
    public void OlderPoints_WithoutBonuses_ComputesAcademicPointsOnly()
    {
        // avg=9 -> 45; ESPB 45/1 * 0.8 = 36; ESPB<120 (bez bonusa), prihod 0 (bez bonusa) => 81
        var result = StudentPointsCalculator.CalculateOlderStudentPoints(
            averageGrade: 9m, espb: 45, yearsOfStudy: 1, incomePerMember: 0m);

        result.Should().Be(81m);
    }

    [Theory]
    [InlineData(0, 50)]      // bez ESPB bonusa
    [InlineData(100, 90)]    // bez ESPB bonusa (100 * 0.4 = 40)
    [InlineData(119, 97.6)]  // još bez bonusa (119 * 0.4 = 47.6)
    [InlineData(120, 99)]    // granica: bonus 1 (120 * 0.4 = 48)
    [InlineData(240, 147)]   // gornja granica: bonus 1 (240 * 0.4 = 96)
    [InlineData(241, 148.4)] // iznad granice: bonus 2 (241 * 0.4 = 96.4)
    [InlineData(300, 172)]   // bonus 2 (300 * 0.4 = 120)
    public void OlderPoints_AppliesEspbBonusAtCorrectThresholds(int espb, decimal expected)
    {
        // Fiksirano: avg=10, godine=2, prihod=0 => akadem = 50 + espb*0.4
        var result = StudentPointsCalculator.CalculateOlderStudentPoints(
            averageGrade: 10m, espb, yearsOfStudy: 2, incomePerMember: 0m);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, 60)]            // nema prihoda -> bez bonusa
    [InlineData(50000, 61)]        // ispod granice -> bonus
    [InlineData(60824, 61)]        // tik ispod granice -> bonus
    [InlineData(60825, 60)]        // tačno na granici -> bez bonusa (strogo "<")
    [InlineData(60826, 60)]        // iznad granice -> bez bonusa
    [InlineData(100000, 60)]       // iznad granice -> bez bonusa
    public void OlderPoints_AppliesSocialBonusStrictlyBelowHalfAverageSalary(
        decimal incomePerMember, decimal expected)
    {
        // Fiksirano: avg=8, ESPB=100, godine=4 => akadem = 40 + (100/4)*0.8 = 60 (bez ESPB bonusa)
        var result = StudentPointsCalculator.CalculateOlderStudentPoints(
            averageGrade: 8m, espb: 100, yearsOfStudy: 4, incomePerMember);

        result.Should().Be(expected);
    }

    [Fact]
    public void OlderPoints_WithEspbAndSocialBonus_AddsBothBonuses()
    {
        // avg=10 -> 50; (240/3)*0.8 = 64; ESPB bonus 1; socijalni bonus 1 => 116
        var result = StudentPointsCalculator.CalculateOlderStudentPoints(
            averageGrade: 10m, espb: 240, yearsOfStudy: 3, incomePerMember: 50000m);

        result.Should().Be(116m);
    }

    [Fact]
    public void OlderPoints_RoundsResultToTwoDecimals()
    {
        // 8.11*5 = 40.55; (10/3)*0.8 = 2.666...; ukupno 43.216... -> 43.22
        var result = StudentPointsCalculator.CalculateOlderStudentPoints(
            averageGrade: 8.11m, espb: 10, yearsOfStudy: 3, incomePerMember: 0m);

        result.Should().Be(43.22m);
    }

    // =========================================================
    //  Brucoši
    // =========================================================

    [Fact]
    public void FreshmanPoints_WithoutSocialBonus_MultipliesGradeByEight()
    {
        // 4.5 * 8 = 36
        var result = StudentPointsCalculator.CalculateFreshmanPoints(
            highSchoolAverageGrade: 4.5m, incomePerMember: 0m);

        result.HighSchoolPoints.Should().Be(36.00m);
        result.SocialBonus.Should().Be(0m);
        result.CalculatedPoints.Should().Be(36.00m);
    }

    [Fact]
    public void FreshmanPoints_WithSocialBonus_AddsOnePoint()
    {
        var result = StudentPointsCalculator.CalculateFreshmanPoints(
            highSchoolAverageGrade: 4.5m, incomePerMember: 50000m);

        result.HighSchoolPoints.Should().Be(36.00m);
        result.SocialBonus.Should().Be(1m);
        result.CalculatedPoints.Should().Be(37.00m);
    }

    [Theory]
    [InlineData(60824, 1)]  // ispod granice -> bonus
    [InlineData(60825, 0)]  // tačno na granici -> bez bonusa
    [InlineData(60826, 0)]  // iznad granice -> bez bonusa
    public void FreshmanPoints_AppliesSocialBonusStrictlyBelowHalfAverageSalary(
        decimal incomePerMember, decimal expectedBonus)
    {
        var result = StudentPointsCalculator.CalculateFreshmanPoints(
            highSchoolAverageGrade: 4m, incomePerMember);

        result.SocialBonus.Should().Be(expectedBonus);
        result.CalculatedPoints.Should().Be(32m + expectedBonus);
    }

    [Fact]
    public void FreshmanPoints_WithZeroGrade_StillGetsSocialBonus()
    {
        // Ocena 0 (npr. nedostaje u profilu) ne uklanja socijalni bonus.
        var result = StudentPointsCalculator.CalculateFreshmanPoints(
            highSchoolAverageGrade: 0m, incomePerMember: 50000m);

        result.HighSchoolPoints.Should().Be(0m);
        result.SocialBonus.Should().Be(1m);
        result.CalculatedPoints.Should().Be(1m);
    }

    [Fact]
    public void FreshmanPoints_RoundsPointsToTwoDecimals()
    {
        // 4.333 * 8 = 34.664 -> 34.66
        var result = StudentPointsCalculator.CalculateFreshmanPoints(
            highSchoolAverageGrade: 4.333m, incomePerMember: 0m);

        result.HighSchoolPoints.Should().Be(34.66m);
        result.CalculatedPoints.Should().Be(34.66m);
    }
}
