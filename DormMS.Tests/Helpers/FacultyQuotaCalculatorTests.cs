using DormMS.Server.Helpers;
using FluentAssertions;

namespace DormMS.Tests.Helpers;

public class FacultyQuotaCalculatorTests
{
    private static Dictionary<string, int> Counts(params (string Faculty, int Count)[] items) =>
        items.ToDictionary(x => x.Faculty, x => x.Count);

    // ---------- Granični slučajevi ----------

    [Fact]
    public void Calculate_WithEmptyInput_ReturnsEmptyDictionary()
    {
        // Act
        var result = FacultyQuotaCalculator.Calculate(Counts(), maxStudents: 100);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Calculate_WhenNoApplicationsTotal_ReturnsEmptyDictionary()
    {
        // Arrange - fakulteti postoje, ali niko se nije prijavio (ukupno 0)
        var counts = Counts(("ETF", 0), ("FON", 0));

        // Act
        var result = FacultyQuotaCalculator.Calculate(counts, maxStudents: 50);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Calculate_WithZeroMaxStudents_GivesEveryoneZeroQuota()
    {
        // Arrange
        var counts = Counts(("ETF", 3), ("FON", 1));

        // Act
        var result = FacultyQuotaCalculator.Calculate(counts, maxStudents: 0);

        // Assert
        result.Should().BeEquivalentTo(Counts(("ETF", 0), ("FON", 0)));
    }

    // ---------- Proporcionalna podela ----------

    [Fact]
    public void Calculate_WithEvenSplit_DividesQuotaEqually()
    {
        // Arrange - 50/50 prijava, 10 mesta -> po 5
        var counts = Counts(("ETF", 50), ("FON", 50));

        // Act
        var result = FacultyQuotaCalculator.Calculate(counts, maxStudents: 10);

        // Assert
        result.Should().BeEquivalentTo(Counts(("ETF", 5), ("FON", 5)));
    }

    [Fact]
    public void Calculate_WithSingleFaculty_AssignsAllSpotsToIt()
    {
        // Arrange
        var counts = Counts(("ETF", 5));

        // Act
        var result = FacultyQuotaCalculator.Calculate(counts, maxStudents: 7);

        // Assert
        result.Should().BeEquivalentTo(Counts(("ETF", 7)));
    }

    // ---------- Raspodela ostatka (floor) ----------

    [Fact]
    public void Calculate_WithRemainder_GivesExtraSpotToLargestFaculty()
    {
        // Arrange - 3/2/1 prijava, 10 mesta.
        // Floor daje: ETF=5, FON=3, PRAF=1 -> dodeljeno 9, ostaje 1 mesto najvećem (ETF).
        var counts = Counts(("ETF", 3), ("FON", 2), ("PRAF", 1));

        // Act
        var result = FacultyQuotaCalculator.Calculate(counts, maxStudents: 10);

        // Assert
        result.Should().BeEquivalentTo(Counts(("ETF", 6), ("FON", 3), ("PRAF", 1)));
        result.Values.Sum().Should().Be(10);
    }

    [Fact]
    public void Calculate_WithLargerRemainder_DistributesToTopFacultiesInOrder()
    {
        // Arrange - 5/5/1 prijava, 10 mesta.
        // Floor daje: ETF=4, FON=4, PRAF=0 -> ostaju 2 mesta, idu najvećima (ETF pa FON).
        var counts = Counts(("ETF", 5), ("FON", 5), ("PRAF", 1));

        // Act
        var result = FacultyQuotaCalculator.Calculate(counts, maxStudents: 10);

        // Assert
        result.Should().BeEquivalentTo(Counts(("ETF", 5), ("FON", 5), ("PRAF", 0)));
        result.Values.Sum().Should().Be(10);
    }

    [Fact]
    public void Calculate_WithTiedFaculties_GivesRemainderToFirstInDictionaryOrder()
    {
        // Arrange - svi imaju po 2 prijave, 4 mesta.
        // Floor daje po 1 (ukupno 3), ostaje 1 mesto -> ide prvom po redu unosa (ETF).
        var counts = Counts(("ETF", 2), ("FON", 2), ("PRAF", 2));

        // Act
        var result = FacultyQuotaCalculator.Calculate(counts, maxStudents: 4);

        // Assert
        result.Should().BeEquivalentTo(Counts(("ETF", 2), ("FON", 1), ("PRAF", 1)));
    }

    // ---------- Invarijante ----------

    [Theory]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(37)]
    [InlineData(100)]
    public void Calculate_NeverAssignsMoreThanMaxStudents_AndUsesEverySpot(int maxStudents)
    {
        // Arrange - realnija raspodela prijava po fakultetima
        var counts = Counts(("ETF", 100), ("FON", 50), ("PRAF", 50), ("MF", 25), ("PF", 10));

        // Act
        var result = FacultyQuotaCalculator.Calculate(counts, maxStudents);

        // Assert - svaki fakultet prisutan, ukupno tačno maxStudents, nijedna kvota nije negativna
        result.Keys.Should().BeEquivalentTo(counts.Keys);
        result.Values.Sum().Should().Be(maxStudents);
        result.Values.Should().OnlyContain(q => q >= 0);
    }
}
