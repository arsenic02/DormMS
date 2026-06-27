namespace DormMS.Server.Services;

using DormMS.Server.Models;
using DormMS.Server.Repositories.Interfaces;
using DormMS.Server.Services.Interfaces;

public class RoomSwapRequestService : IRoomSwapRequestService
{
    private readonly IRoomSwapRequestRepository _roomSwapRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly INotificationService _notificationService;
    private readonly AppDbContext _context;

    public RoomSwapRequestService(
        IRoomSwapRequestRepository roomSwapRepository,
        IStudentRepository studentRepository,
        INotificationService notificationService,
        AppDbContext context)
    {
        _roomSwapRepository = roomSwapRepository;
        _studentRepository = studentRepository;
        _notificationService = notificationService;
        _context = context;
    }

    public async Task<RoomSwapRequestDto> CreateAsync(int senderUserId, CreateRoomSwapRequestDto dto)
    {
        var sender = await _studentRepository.GetByUserIdAsync(senderUserId)
            ?? throw new KeyNotFoundException("Vaš studentski profil nije pronađen.");

        var receiver = await _studentRepository.GetByStudentIdNumberAsync(dto.ReceiverStudentIdNumber)
            ?? throw new KeyNotFoundException("Student sa unetim brojem indeksa nije pronađen.");

        if (sender.Id == receiver.Id)
            throw new InvalidOperationException("Ne možete poslati zahtev samom sebi.");

        if (sender.DormitoryRoomId == null)
            throw new InvalidOperationException("Ne možete zameniti sobu jer trenutno nemate dodeljenu sobu.");

        if (receiver.DormitoryRoomId == null)
            throw new InvalidOperationException("Student kome šaljete zahtev nema dodeljenu sobu.");

        if (sender.DormitoryRoomId == receiver.DormitoryRoomId)
            throw new InvalidOperationException("Vi i primalac ste već u istoj sobi.");

        if (sender.Gender != receiver.Gender)
            throw new InvalidOperationException("Zamena sobe je moguća samo između studenata istog pola.");

        var hasPending = await _roomSwapRepository.HasPendingRequestBetweenAsync(sender.Id, receiver.Id);
        if (hasPending)
            throw new InvalidOperationException("Već postoji aktivan zahtev između vas i ovog studenta.");

        var senderHasPending = await _roomSwapRepository.HasAnyPendingRequestInvolvingAsync(sender.Id);
        if (senderHasPending)
            throw new InvalidOperationException("Već imate aktivan zahtev za zamenu sobe.");

        var receiverHasPending = await _roomSwapRepository.HasAnyPendingRequestInvolvingAsync(receiver.Id);
        if (receiverHasPending)
            throw new InvalidOperationException("Primalac već ima aktivan zahtev za zamenu sobe.");

        var request = new RoomSwapRequest
        {
            SenderId = sender.Id,
            ReceiverId = receiver.Id,
            SenderRoomId = sender.DormitoryRoomId.Value,
            ReceiverRoomId = receiver.DormitoryRoomId.Value,
            Status = RoomSwapStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _roomSwapRepository.AddAsync(request);

        await _notificationService.NotifyStudentAsync(
            receiver.Id,
            $"{sender.Name} {sender.Surname} vam je poslao/la zahtev za zamenu sobe.");

        return MapToDto(request, sender, receiver);
    }

    public async Task<RoomSwapRequestDto> RespondAsync(int receiverUserId, int requestId, RespondRoomSwapRequestDto dto)
    {
        var receiverStudent = await _studentRepository.GetByUserIdAsync(receiverUserId)
            ?? throw new KeyNotFoundException("Vaš studentski profil nije pronađen.");

        var request = await _roomSwapRepository.GetByIdTrackedAsync(requestId)
            ?? throw new KeyNotFoundException("Zahtev nije pronađen.");

        if (request.ReceiverId != receiverStudent.Id)
            throw new UnauthorizedAccessException("Nemate dozvolu da odgovorite na ovaj zahtev.");

        if (request.Status != RoomSwapStatus.Pending)
            throw new InvalidOperationException("Na ovaj zahtev je već odgovoreno.");

        if (!dto.Accept)
        {
            request.Status = RoomSwapStatus.Rejected;
            request.RespondedAt = DateTime.UtcNow;
            await _roomSwapRepository.UpdateAsync(request);

            await _notificationService.NotifyStudentAsync(
                request.SenderId,
                $"{request.Receiver.Name} {request.Receiver.Surname} je odbio/la vaš zahtev za zamenu sobe.");

            return MapToDto(request, request.Sender, request.Receiver);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var sender = await _studentRepository.GetByIdTrackedAsync(request.SenderId)
                ?? throw new KeyNotFoundException("Sender više ne postoji.");
            var receiver = await _studentRepository.GetByIdTrackedAsync(request.ReceiverId)
                ?? throw new KeyNotFoundException("Receiver više ne postoji.");

            if (sender.DormitoryRoomId != request.SenderRoomId)
                throw new InvalidOperationException(
                    "Sender je u međuvremenu promenio sobu — zamena nije više validna.");

            if (receiver.DormitoryRoomId != request.ReceiverRoomId)
                throw new InvalidOperationException(
                    "Receiver je u međuvremenu promenio sobu — zamena nije više validna.");

            sender.DormitoryRoomId = request.ReceiverRoomId;
            receiver.DormitoryRoomId = request.SenderRoomId;

            await _studentRepository.UpdateAsync(sender);
            await _studentRepository.UpdateAsync(receiver);

            request.Status = RoomSwapStatus.Accepted;
            request.RespondedAt = DateTime.UtcNow;
            await _roomSwapRepository.UpdateAsync(request);

            await transaction.CommitAsync();

            await _notificationService.NotifyStudentAsync(
                request.SenderId,
                $"{receiver.Name} {receiver.Surname} je prihvatio/la vaš zahtev — zamena sobe je izvršena.");

            await _notificationService.NotifyDormAsync(
                request.SenderRoom.DormId,
                $"Izvršena je zamena sobe između {sender.Name} {sender.Surname} (soba {request.ReceiverRoom.RoomNumber}) i {receiver.Name} {receiver.Surname} (soba {request.SenderRoom.RoomNumber}).");

            return MapToDto(request, sender, receiver);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<RoomSwapRequestDto>> GetIncomingAsync(int userId)
    {
        var student = await _studentRepository.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Vaš studentski profil nije pronađen.");

        var requests = await _roomSwapRepository.GetIncomingForStudentAsync(student.Id);
        return requests.Select(r => MapToDto(r, r.Sender, r.Receiver)).ToList();
    }

    public async Task<List<RoomSwapRequestDto>> GetOutgoingAsync(int userId)
    {
        var student = await _studentRepository.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Vaš studentski profil nije pronađen.");

        var requests = await _roomSwapRepository.GetOutgoingForStudentAsync(student.Id);
        return requests.Select(r => MapToDto(r, r.Sender, r.Receiver)).ToList();
    }

    private static RoomSwapRequestDto MapToDto(RoomSwapRequest request, Student sender, Student receiver)
    {
        return new RoomSwapRequestDto
        {
            Id = request.Id,
            SenderId = sender.Id,
            SenderName = $"{sender.Name} {sender.Surname}",
            SenderRoomNumber = request.SenderRoom?.RoomNumber ?? string.Empty,
            ReceiverId = receiver.Id,
            ReceiverName = $"{receiver.Name} {receiver.Surname}",
            ReceiverRoomNumber = request.ReceiverRoom?.RoomNumber ?? string.Empty,
            Status = request.Status,
            CreatedAt = request.CreatedAt,
            RespondedAt = request.RespondedAt
        };
    }
}

// namespace DormMS.Server.Services;
// using DormMS.Server.Models;
// using DormMS.Server.Repositories.Interfaces;
// using DormMS.Server.Services.Interfaces;

// public class RoomSwapRequestService : IRoomSwapRequestService
// {
//     private readonly IRoomSwapRequestRepository _roomSwapRepository;
//     private readonly IStudentRepository _studentRepository;
//     private readonly INotificationService _notificationService;
//     private readonly AppDbContext _context;

//     public RoomSwapRequestService(
//         IRoomSwapRequestRepository roomSwapRepository,
//         IStudentRepository studentRepository,
//         INotificationService notificationService,
//         AppDbContext context)
//     {
//         _roomSwapRepository = roomSwapRepository;
//         _studentRepository = studentRepository;
//         _notificationService = notificationService;
//         _context = context;
//     }

//     public async Task<RoomSwapRequestDto> CreateAsync(int senderUserId, CreateRoomSwapRequestDto dto)
//     {
//         var sender = await _studentRepository.GetByUserIdAsync(senderUserId)
//             ?? throw new KeyNotFoundException("Vaš studentski profil nije pronađen.");

//         var receiver = await _studentRepository.GetByStudentIdNumberAsync(dto.ReceiverStudentIdNumber)
//             ?? throw new KeyNotFoundException("Student sa unetim brojem indeksa nije pronađen.");

//         if (sender.Id == receiver.Id)
//             throw new InvalidOperationException("Ne možete poslati zahtev samom sebi.");

//         if (sender.DormitoryRoomId == null)
//             throw new InvalidOperationException("Ne možete zameniti sobu jer trenutno nemate dodeljenu sobu.");

//         if (receiver.DormitoryRoomId == null)
//             throw new InvalidOperationException("Student kome šaljete zahtev nema dodeljenu sobu.");

//         if (sender.DormitoryRoomId == receiver.DormitoryRoomId)
//             throw new InvalidOperationException("Vi i primalac ste već u istoj sobi.");

//         if (sender.Gender != receiver.Gender)
//             throw new InvalidOperationException("Zamena sobe je moguća samo između studenata istog pola.");

//         var hasPending = await _roomSwapRepository.HasPendingRequestBetweenAsync(sender.Id, receiver.Id);
//         if (hasPending)
//             throw new InvalidOperationException("Već postoji aktivan zahtev između vas i ovog studenta.");

//         var senderHasPending = await _roomSwapRepository.HasAnyPendingRequestInvolvingAsync(sender.Id);
//         if (senderHasPending)
//             throw new InvalidOperationException("Već imate aktivan zahtev za zamenu sobe.");

//         var receiverHasPending = await _roomSwapRepository.HasAnyPendingRequestInvolvingAsync(receiver.Id);
//         if (receiverHasPending)
//             throw new InvalidOperationException("Primalac već ima aktivan zahtev za zamenu sobe.");

//         var request = new RoomSwapRequest
//         {
//             SenderId = sender.Id,
//             ReceiverId = receiver.Id,
//             SenderRoomId = sender.DormitoryRoomId.Value,
//             ReceiverRoomId = receiver.DormitoryRoomId.Value,
//             Status = RoomSwapStatus.Pending,
//             CreatedAt = DateTime.UtcNow
//         };

//         await _roomSwapRepository.AddAsync(request);

//         await _notificationService.NotifyStudentAsync(
//             receiver.Id,
//             $"{sender.Name} {sender.Surname} vam je poslao/la zahtev za zamenu sobe.");

//         return MapToDto(request, sender, receiver);
//     }

//     public async Task<RoomSwapRequestDto> RespondAsync(int receiverUserId, int requestId, RespondRoomSwapRequestDto dto)
//     {
//         var receiverStudent = await _studentRepository.GetByUserIdAsync(receiverUserId)
//             ?? throw new KeyNotFoundException("Vaš studentski profil nije pronađen.");

//         var request = await _roomSwapRepository.GetByIdTrackedAsync(requestId)
//             ?? throw new KeyNotFoundException("Zahtev nije pronađen.");

//         if (request.ReceiverId != receiverStudent.Id)
//             throw new UnauthorizedAccessException("Nemate dozvolu da odgovorite na ovaj zahtev.");

//         if (request.Status != RoomSwapStatus.Pending)
//             throw new InvalidOperationException("Na ovaj zahtev je već odgovoreno.");

//         if (!dto.Accept)
//         {
//             request.Status = RoomSwapStatus.Rejected;
//             request.RespondedAt = DateTime.UtcNow;
//             await _roomSwapRepository.UpdateAsync(request);

//             await _notificationService.NotifyStudentAsync(
//                 request.SenderId,
//                 $"{request.Receiver.Name} {request.Receiver.Surname} je odbio/la vaš zahtev za zamenu sobe.");

//             return MapToDto(request, request.Sender, request.Receiver);
//         }

//         await using var transaction = await _context.Database.BeginTransactionAsync();
//         try
//         {
//             var sender = await _studentRepository.GetByIdTrackedAsync(request.SenderId)
//                 ?? throw new KeyNotFoundException("Sender više ne postoji.");
//             var receiver = await _studentRepository.GetByIdTrackedAsync(request.ReceiverId)
//                 ?? throw new KeyNotFoundException("Receiver više ne postoji.");

//             if (sender.DormitoryRoomId != request.SenderRoomId)
//                 throw new InvalidOperationException(
//                     "Sender je u međuvremenu promenio sobu — zamena nije više validna.");

//             if (receiver.DormitoryRoomId != request.ReceiverRoomId)
//                 throw new InvalidOperationException(
//                     "Receiver je u međuvremenu promenio sobu — zamena nije više validna.");

//             sender.DormitoryRoomId = request.ReceiverRoomId;
//             receiver.DormitoryRoomId = request.SenderRoomId;

//             await _studentRepository.UpdateAsync(sender);
//             await _studentRepository.UpdateAsync(receiver);

//             request.Status = RoomSwapStatus.Accepted;
//             request.RespondedAt = DateTime.UtcNow;
//             await _roomSwapRepository.UpdateAsync(request);

//             await transaction.CommitAsync();

//             await _notificationService.NotifyStudentAsync(
//                 request.SenderId,
//                 $"{receiver.Name} {receiver.Surname} je prihvatio/la vaš zahtev — zamena sobe je izvršena.");

//             await _notificationService.NotifyDormAsync(
//                 request.SenderRoom.DormId,
//                 $"Izvršena je zamena sobe između {sender.Name} {sender.Surname} (soba {request.ReceiverRoom.RoomNumber}) i {receiver.Name} {receiver.Surname} (soba {request.SenderRoom.RoomNumber}).");

//             return MapToDto(request, sender, receiver);
//         }
//         catch
//         {
//             await transaction.RollbackAsync();
//             throw;
//         }
//     }
//      public async Task<List<RoomSwapRequestDto>> GetIncomingAsync(int userId)
//     {
//         var student = await _studentRepository.GetByUserIdAsync(userId)
//             ?? throw new KeyNotFoundException("Vaš studentski profil nije pronađen.");

//         var requests = await _roomSwapRepository.GetIncomingForStudentAsync(student.Id);
//         return requests.Select(r => MapToDto(r, r.Sender, r.Receiver)).ToList();
//     }

//     public async Task<List<RoomSwapRequestDto>> GetOutgoingAsync(int userId)
//     {
//         var student = await _studentRepository.GetByUserIdAsync(userId)
//             ?? throw new KeyNotFoundException("Vaš studentski profil nije pronađen.");

//         var requests = await _roomSwapRepository.GetOutgoingForStudentAsync(student.Id);
//         return requests.Select(r => MapToDto(r, r.Sender, r.Receiver)).ToList();
//     }
//     private static RoomSwapRequestDto MapToDto(RoomSwapRequest request, Student sender, Student receiver)
//     {
//         return new RoomSwapRequestDto
//         {
//             Id = request.Id,
//             SenderId = sender.Id,
//             SenderName = $"{sender.Name} {sender.Surname}",
//             SenderRoomNumber = request.SenderRoom?.RoomNumber ?? string.Empty,
//             ReceiverId = receiver.Id,
//             ReceiverName = $"{receiver.Name} {receiver.Surname}",
//             ReceiverRoomNumber = request.ReceiverRoom?.RoomNumber ?? string.Empty,
//             Status = request.Status,
//             CreatedAt = request.CreatedAt,
//             RespondedAt = request.RespondedAt
//         };
//     }
// }