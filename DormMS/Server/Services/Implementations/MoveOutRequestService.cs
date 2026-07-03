namespace DormMS.Server.Services;

using DormMS.Server.Models;
using DormMS.Server.Repositories.Interfaces;
using DormMS.Server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class MoveOutRequestService : IMoveOutRequestService
{
    private readonly IMoveOutRequestRepository _moveOutRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IDormManagerRepository _dormManagerRepository;
    private readonly INotificationService _notificationService;
    private readonly AppDbContext _context;

    public MoveOutRequestService(
        IMoveOutRequestRepository moveOutRepository,
        IStudentRepository studentRepository,
        IDormManagerRepository dormManagerRepository,
        INotificationService notificationService,
        AppDbContext context)
    {
        _moveOutRepository = moveOutRepository;
        _studentRepository = studentRepository;
        _dormManagerRepository = dormManagerRepository;
        _notificationService = notificationService;
        _context = context;
    }

    public async Task<MoveOutRequestDto> CreateAsync(int studentUserId)
    {
        var student = await _studentRepository.GetByUserIdAsync(studentUserId)
            ?? throw new KeyNotFoundException("Vaš studentski profil nije pronađen.");

        if (student.DormitoryRoomId == null)
            throw new InvalidOperationException("Ne možete zatražiti iseljenje jer trenutno nemate dodeljenu sobu.");

        var existingRequest = await _moveOutRepository.GetPendingForStudentAsync(student.Id);
        if (existingRequest != null)
            throw new InvalidOperationException("Već imate aktivan zahtev za iseljenje.");

        var room = await _context.DormitoryRooms
            .Include(r => r.Dorm)
            .FirstOrDefaultAsync(r => r.Id == student.DormitoryRoomId.Value)
            ?? throw new InvalidOperationException("Soba nije pronađena.");

        var request = new MoveOutRequest
        {
            StudentId = student.Id,
            RoomId = student.DormitoryRoomId.Value,
            RoomNumberSnapshot = room.RoomNumber,
            DormNameSnapshot = room.Dorm?.Name ?? "",
            Status = MoveOutStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _moveOutRepository.AddAsync(request);

        await _notificationService.NotifyDormAsync(
            room.DormId,
            $"{student.Name} {student.Surname} je zatražio/la iseljenje iz sobe {room.RoomNumber}.");

        return MapToDto(request, student, room);
    }

    public async Task<MoveOutRequestDto?> GetMyRequestAsync(int studentUserId)
    {
        var student = await _studentRepository.GetByUserIdAsync(studentUserId)
            ?? throw new KeyNotFoundException("Vaš studentski profil nije pronađen.");

        // var request = await _moveOutRepository.GetPendingForStudentAsync(student.Id);
        var request = await _moveOutRepository.GetLatestForStudentAsync(student.Id);
        if (request == null)
            return null;

        return MapToDto(request, student, request.Room);
    }

    public async Task<MoveOutRequestDto?> GetMyCompletedRequestAsync(int studentUserId)
    {
        var student = await _studentRepository.GetByUserIdAsync(studentUserId)
            ?? throw new KeyNotFoundException("Vaš studentski profil nije pronađen.");

        var request = await _moveOutRepository.GetLatestForStudentAsync(student.Id);
        if (request == null || request.Status == MoveOutStatus.Pending)
            return null;

        return MapToDto(request, student, request.Room);
    }

    public async Task<MoveOutRequestDto> ApproveAsync(int managerUserId, int requestId)
    {
        var request = await _moveOutRepository.GetByIdTrackedAsync(requestId)
            ?? throw new KeyNotFoundException("Zahtev nije pronađen.");

        if (request.Status != MoveOutStatus.Pending)
            throw new InvalidOperationException("Ovaj zahtev ne čeka odobrenje.");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var student = await _studentRepository.GetByIdTrackedAsync(request.StudentId)
                ?? throw new KeyNotFoundException("Student više ne postoji.");

            if (student.DormitoryRoomId != request.RoomId)
                throw new InvalidOperationException(
                    "Student je u međuvremenu promenio sobu — zahtev nije više validan.");

            student.DormitoryRoomId = null;

            await _studentRepository.UpdateAsync(student);

            request.Status = MoveOutStatus.Approved;
            request.RespondedAt = DateTime.UtcNow;
            await _moveOutRepository.UpdateAsync(request);

            await transaction.CommitAsync();

            await _notificationService.NotifyStudentAsync(
                request.StudentId,
                $"Referent je odobrio vaš zahtev za iseljenje iz sobe {request.Room.RoomNumber}.");

            return MapToDto(request, student, request.Room);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<MoveOutRequestDto> RejectAsync(int managerUserId, int requestId)
    {
        var request = await _moveOutRepository.GetByIdTrackedAsync(requestId)
            ?? throw new KeyNotFoundException("Zahtev nije pronađen.");

        if (request.Status != MoveOutStatus.Pending)
            throw new InvalidOperationException("Ovaj zahtev ne čeka odobrenje.");

        request.Status = MoveOutStatus.Rejected;
        request.RespondedAt = DateTime.UtcNow;
        await _moveOutRepository.UpdateAsync(request);

        await _notificationService.NotifyStudentAsync(
            request.StudentId,
            "Referent je odbio vaš zahtev za iseljenje.");

        return MapToDto(request, request.Student, request.Room);
    }

    public async Task<List<MoveOutRequestDto>> GetPendingForManagerAsync(int managerUserId)
    {
        var manager = await _dormManagerRepository.GetByUserIdAsync(managerUserId)
            ?? throw new KeyNotFoundException("Vaš profil referenta nije pronađen.");

        var requests = await _moveOutRepository.GetPendingForDormAsync(manager.DormId);
        return requests.Select(r => MapToDto(r, r.Student, r.Room)).ToList();
    }

    private static MoveOutRequestDto MapToDto(MoveOutRequest request, Student student, DormitoryRoom room)
    {
        return new MoveOutRequestDto
        {
            Id = request.Id,
            StudentId = student.Id,
            StudentName = $"{student.Name} {student.Surname}",
            StudentIdNumber = student.StudentIDNumber,
            RoomId = room.Id,
            RoomNumber = room.RoomNumber,
            DormName = room.Dorm?.Name ?? string.Empty,
            RoomNumberSnapshot = request.RoomNumberSnapshot,
            DormNameSnapshot = request.DormNameSnapshot,
            Status = request.Status,
            CreatedAt = request.CreatedAt,
            RespondedAt = request.RespondedAt
        };
    }
}
