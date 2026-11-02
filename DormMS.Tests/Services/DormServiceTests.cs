using DormMS.Server.DTOs;
using DormMS.Server.Models;
using DormMS.Server.Repositories.Interfaces;
using DormMS.Server.Services.Implementations;
using FluentAssertions;
using Moq;

namespace DormMS.Tests.Services;

/// <summary>
/// Jedinični testovi za <see cref="DormService.GenerateRoomsAsync"/>.
///
/// Testiramo samo logiku servisa (podelu i numeraciju soba), bez prave baze:
/// zavisnost <see cref="IDormRepository"/> zamenjujemo lažnim objektom (mock-om).
///
/// Moq pojmovi koji se koriste:
///   new Mock&lt;IDormRepository&gt;()  -> lažni objekat koji implementira interfejs
///   .Setup(r => r.Metod(...))       -> "kada se pozove ovaj metod..."
///   .ReturnsAsync(vrednost)         -> "...vrati ovu vrednost" (za async metode)
///   It.IsAny&lt;T&gt;()               -> "bilo koji argument ovog tipa"
///   .Callback(...)                  -> hvata ono što servis pošalje repozitorijumu
///   .Object                         -> stvarna instanca mock-a koja se predaje servisu
///   .Verify(..., Times.Never)       -> provera da metod nije pozvan
///
/// xUnit pravi novu instancu klase za svaki test, pa je stanje iz konstruktora uvek sveže.
/// </summary>
public class DormServiceTests
{
    private const int DefaultDormId = 1;

    private readonly Mock<IDormRepository> _repositoryMock = new();
    private readonly DormService _service;

    // Sobe koje je servis poslao repozitorijumu (popunjava se u Callback-u).
    private List<DormitoryRoom> _createdRooms = new();

    public DormServiceTests()
    {
        // xUnit pravi novu instancu klase za SVAKI test, pa je ovo svež setup za svaki test.
        _service = new DormService(_repositoryMock.Object);

        _repositoryMock
            .Setup(r => r.AddRoomsAsync(It.IsAny<IEnumerable<DormitoryRoom>>()))
            .Callback<IEnumerable<DormitoryRoom>>(rooms => _createdRooms = rooms.ToList());
    }

    // ---------- Pomoćne metode ----------

    private static Dorm CreateDorm(int id = DefaultDormId, int existingRooms = 0)
    {
        var dorm = new Dorm { Id = id, Name = "Dom", Location = "Niš", Municipality = "Medijana" };

        for (var i = 1; i <= existingRooms; i++)
        {
            dorm.Rooms.Add(new DormitoryRoom
            {
                Id = i,
                RoomNumber = i.ToString("D3"),
                Capacity = 2,
                DormId = id
            });
        }

        return dorm;
    }

    private void GivenDormExists(Dorm dorm) =>
        _repositoryMock.Setup(r => r.GetByIdWithRoomsAsync(dorm.Id)).ReturnsAsync(dorm);

    private static GenerateRoomsDto CreateDto(
        int dormId = DefaultDormId,
        int doubleRooms = 2,
        int tripleRooms = 2,
        int malePercentage = 50) => new()
    {
        DormId = dormId,
        DoubleRooms = doubleRooms,
        TripleRooms = tripleRooms,
        MalePercentage = malePercentage
    };

    // ---------- Testovi ----------

    [Theory]
    [InlineData(100, 6, 0)]
    [InlineData(0, 0, 6)]
    [InlineData(50, 3, 3)]
    public async Task GenerateRoomsAsync_SplitsRoomsByGender_AccordingToMalePercentage(
        int malePercentage, int expectedMale, int expectedFemale)
    {
        // Arrange
        GivenDormExists(CreateDorm());
        var dto = CreateDto(doubleRooms: 3, tripleRooms: 3, malePercentage: malePercentage);

        // Act
        await _service.GenerateRoomsAsync(dto);

        // Assert
        _createdRooms.Should().HaveCount(6);
        _createdRooms.Count(r => r.Gender == GenderType.Male).Should().Be(expectedMale);
        _createdRooms.Count(r => r.Gender == GenderType.Female).Should().Be(expectedFemale);
    }

    [Fact]
    public async Task GenerateRoomsAsync_CreatesRequestedNumberOfRoomsPerCapacity()
    {
        // Arrange
        GivenDormExists(CreateDorm());
        var dto = CreateDto(doubleRooms: 4, tripleRooms: 2);

        // Act
        await _service.GenerateRoomsAsync(dto);

        // Assert
        _createdRooms.Count(r => r.Capacity == 2).Should().Be(4);
        _createdRooms.Count(r => r.Capacity == 3).Should().Be(2);
    }

    [Fact]
    public async Task GenerateRoomsAsync_WhenDormAlreadyHasRooms_ContinuesRoomNumbering()
    {
        // Arrange - dom već ima sobe 001, 002, 003
        GivenDormExists(CreateDorm(existingRooms: 3));
        var dto = CreateDto(doubleRooms: 1, tripleRooms: 1);

        // Act
        await _service.GenerateRoomsAsync(dto);

        // Assert - Equal proverava i redosled i tačan broj elemenata
        _createdRooms.Select(r => r.RoomNumber).Should().Equal("004", "005");
    }

    [Fact]
    public async Task GenerateRoomsAsync_WhenDormDoesNotExist_ThrowsAndSavesNothing()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdWithRoomsAsync(It.IsAny<int>()))
            .ReturnsAsync((Dorm?)null);

        // Act
        var act = () => _service.GenerateRoomsAsync(CreateDto(dormId: 999));

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Dom nije pronađen");
        _repositoryMock.Verify(
            r => r.AddRoomsAsync(It.IsAny<IEnumerable<DormitoryRoom>>()), Times.Never);
    }

    [Theory]
    [InlineData(-1, 2, 50)]
    [InlineData(2, -1, 50)]
    [InlineData(2, 2, -1)]
    [InlineData(2, 2, 101)]
    public async Task GenerateRoomsAsync_WithInvalidInput_ThrowsArgumentExceptionBeforeTouchingRepository(
        int doubleRooms, int tripleRooms, int malePercentage)
    {
        // Arrange
        var dto = CreateDto(doubleRooms: doubleRooms, tripleRooms: tripleRooms, malePercentage: malePercentage);

        // Act
        var act = () => _service.GenerateRoomsAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
        _repositoryMock.Verify(r => r.GetByIdWithRoomsAsync(It.IsAny<int>()), Times.Never);
        _repositoryMock.Verify(
            r => r.AddRoomsAsync(It.IsAny<IEnumerable<DormitoryRoom>>()), Times.Never);
    }
}
