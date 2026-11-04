using DormMS.Server.Models;
using DormMS.Server.Repositories.Interfaces;
using DormMS.Server.Services;
using DormMS.Server.Services.Interfaces;
using FluentAssertions;
using Moq;

namespace DormMS.Tests.Services;

public class RoomSwapRequestServiceTests
{
    private const int SenderUserId = 100;
    private const string ReceiverIndexNumber = "RA123/2024";

    private readonly Mock<IRoomSwapRequestRepository> _roomSwapRepositoryMock = new();
    private readonly Mock<IStudentRepository> _studentRepositoryMock = new();
    private readonly Mock<IDormManagerRepository> _dormManagerRepositoryMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();

    private readonly RoomSwapRequestService _service;

    public RoomSwapRequestServiceTests()
    {
        _service = new RoomSwapRequestService(
            _roomSwapRepositoryMock.Object,
            _studentRepositoryMock.Object,
            _dormManagerRepositoryMock.Object,
            _notificationServiceMock.Object,
            context: null!);
    }

    // ---------- Pomoćne metode ----------

    private static Student CreateStudent(
        int id,
        Gender gender = Gender.Male,
        int? roomId = null,
        string name = "Petar",
        string surname = "Petrović") => new()
    {
        Id = id,
        Name = name,
        Surname = surname,
        Gender = gender,
        DormitoryRoomId = roomId
    };

    private static CreateRoomSwapRequestDto CreateDto(
        string receiverIndexNumber = ReceiverIndexNumber) => new()
    {
        ReceiverStudentIdNumber = receiverIndexNumber
    };

    private void GivenSender(Student sender) =>
        _studentRepositoryMock
            .Setup(r => r.GetByUserIdAsync(SenderUserId))
            .ReturnsAsync(sender);

    private void GivenReceiver(Student receiver) =>
        _studentRepositoryMock
            .Setup(r => r.GetByStudentIdNumberAsync(It.IsAny<string>()))
            .ReturnsAsync(receiver);

    /// <summary>Sender (id=1, soba 10) i primalac (id=2, soba 20), isti pol — validan slučaj.</summary>
    private (Student Sender, Student Receiver) GivenValidPair()
    {
        var sender = CreateStudent(id: 1, roomId: 10);
        var receiver = CreateStudent(id: 2, roomId: 20, name: "Marko", surname: "Marković");
        GivenSender(sender);
        GivenReceiver(receiver);
        return (sender, receiver);
    }

    private void VerifyNothingWasSaved()
    {
        _roomSwapRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<RoomSwapRequest>()), Times.Never);
        _notificationServiceMock.Verify(
            n => n.NotifyStudentAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    // ---------- Happy path ----------

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsPendingRequestAndNotifiesReceiver()
    {
        // Arrange
        var (sender, receiver) = GivenValidPair();

        // Act
        var result = await _service.CreateAsync(SenderUserId, CreateDto());

        // Assert
        result.Status.Should().Be(RoomSwapStatus.Pending);
        result.SenderId.Should().Be(sender.Id);
        result.ReceiverId.Should().Be(receiver.Id);
        result.SenderName.Should().Be($"{sender.Name} {sender.Surname}");

        _roomSwapRepositoryMock.Verify(
            r => r.AddAsync(It.Is<RoomSwapRequest>(req =>
                req.SenderId == sender.Id &&
                req.ReceiverId == receiver.Id &&
                req.Status == RoomSwapStatus.Pending)), Times.Once);

        _notificationServiceMock.Verify(
            n => n.NotifyStudentAsync(receiver.Id, It.IsAny<string>()), Times.Once);
    }

    // ---------- Nepostojeći profili ----------

    [Fact]
    public async Task CreateAsync_WhenSenderProfileMissing_ThrowsKeyNotFound()
    {
        // Arrange
        _studentRepositoryMock
            .Setup(r => r.GetByUserIdAsync(SenderUserId))
            .ReturnsAsync((Student?)null);

        // Act
        var act = () => _service.CreateAsync(SenderUserId, CreateDto());

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Vaš studentski profil nije pronađen.");
        VerifyNothingWasSaved();
    }

    [Fact]
    public async Task CreateAsync_WhenReceiverIndexNumberDoesNotExist_ThrowsKeyNotFound()
    {
        // Arrange
        GivenSender(CreateStudent(id: 1, roomId: 10));
        GivenReceiver(null!); // mock vraća null

        // Act
        var act = () => _service.CreateAsync(SenderUserId, CreateDto());

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Student sa unetim brojem indeksa nije pronađen.");
        VerifyNothingWasSaved();
    }

    // ---------- Poslovna pravila ----------

    [Fact]
    public async Task CreateAsync_WhenSenderAndReceiverAreSameStudent_ThrowsInvalidOperation()
    {
        // Arrange - isti student (isti Id) i kao sender i kao primalac
        var sender = CreateStudent(id: 1, roomId: 10);
        GivenSender(sender);
        GivenReceiver(sender);

        // Act
        var act = () => _service.CreateAsync(SenderUserId, CreateDto());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Ne možete poslati zahtev samom sebi.");
        VerifyNothingWasSaved();
    }

    [Fact]
    public async Task CreateAsync_WhenSenderHasNoRoom_ThrowsInvalidOperation()
    {
        // Arrange
        GivenSender(CreateStudent(id: 1, roomId: null));
        GivenReceiver(CreateStudent(id: 2, roomId: 20));

        // Act
        var act = () => _service.CreateAsync(SenderUserId, CreateDto());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Ne možete zameniti sobu jer trenutno nemate dodeljenu sobu.");
        VerifyNothingWasSaved();
    }

    [Fact]
    public async Task CreateAsync_WhenReceiverHasNoRoom_ThrowsInvalidOperation()
    {
        // Arrange
        GivenSender(CreateStudent(id: 1, roomId: 10));
        GivenReceiver(CreateStudent(id: 2, roomId: null));

        // Act
        var act = () => _service.CreateAsync(SenderUserId, CreateDto());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Student kome šaljete zahtev nema dodeljenu sobu.");
        VerifyNothingWasSaved();
    }

    [Fact]
    public async Task CreateAsync_WhenAlreadyInSameRoom_ThrowsInvalidOperation()
    {
        // Arrange - oba studenta su u sobi 10
        GivenSender(CreateStudent(id: 1, roomId: 10));
        GivenReceiver(CreateStudent(id: 2, roomId: 10));

        // Act
        var act = () => _service.CreateAsync(SenderUserId, CreateDto());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Vi i primalac ste već u istoj sobi.");
        VerifyNothingWasSaved();
    }

    [Fact]
    public async Task CreateAsync_WhenGendersDiffer_ThrowsInvalidOperation()
    {
        // Arrange - različite sobe, ali različit pol
        GivenSender(CreateStudent(id: 1, roomId: 10, gender: Gender.Male));
        GivenReceiver(CreateStudent(id: 2, roomId: 20, gender: Gender.Female));

        // Act
        var act = () => _service.CreateAsync(SenderUserId, CreateDto());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Zamena sobe je moguća samo između studenata istog pola.");
        VerifyNothingWasSaved();
    }

    // ---------- Duplikati / aktivni zahtevi ----------

    [Fact]
    public async Task CreateAsync_WhenPendingRequestAlreadyExistsBetweenThem_ThrowsInvalidOperation()
    {
        // Arrange
        GivenValidPair();
        _roomSwapRepositoryMock
            .Setup(r => r.HasPendingRequestBetweenAsync(1, 2))
            .ReturnsAsync(true);

        // Act
        var act = () => _service.CreateAsync(SenderUserId, CreateDto());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Već postoji aktivan zahtev između vas i ovog studenta.");
        VerifyNothingWasSaved();
    }

    [Fact]
    public async Task CreateAsync_WhenSenderAlreadyHasActiveRequest_ThrowsInvalidOperation()
    {
        // Arrange - samo sender ima aktivan zahtev (receiver nema)
        GivenValidPair();
        _roomSwapRepositoryMock
            .Setup(r => r.HasAnyPendingRequestInvolvingAsync(1))
            .ReturnsAsync(true);

        // Act
        var act = () => _service.CreateAsync(SenderUserId, CreateDto());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Već imate aktivan zahtev za zamenu sobe.");
        VerifyNothingWasSaved();
    }

    [Fact]
    public async Task CreateAsync_WhenReceiverAlreadyHasActiveRequest_ThrowsInvalidOperation()
    {
        // Arrange - samo receiver ima aktivan zahtev (sender nema)
        GivenValidPair();
        _roomSwapRepositoryMock
            .Setup(r => r.HasAnyPendingRequestInvolvingAsync(2))
            .ReturnsAsync(true);

        // Act
        var act = () => _service.CreateAsync(SenderUserId, CreateDto());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Primalac već ima aktivan zahtev za zamenu sobe.");
        VerifyNothingWasSaved();
    }
}
