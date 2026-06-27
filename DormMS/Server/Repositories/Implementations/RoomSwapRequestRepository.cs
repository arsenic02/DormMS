namespace DormMS.Server.Repositories;

using DormMS.Server.Models;
using DormMS.Server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

public class RoomSwapRequestRepository : IRoomSwapRequestRepository
{
    private readonly AppDbContext _context;

    public RoomSwapRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RoomSwapRequest?> GetByIdAsync(int id)
    {
        return await _context.RoomSwapRequests
            .AsNoTracking()
            .Include(r => r.Sender)
            .Include(r => r.Receiver)
            .Include(r => r.SenderRoom)
            .Include(r => r.ReceiverRoom)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<RoomSwapRequest?> GetByIdTrackedAsync(int id)
    {
        return await _context.RoomSwapRequests
            .Include(r => r.Sender)
            .Include(r => r.Receiver)
            .Include(r => r.SenderRoom)
            .Include(r => r.ReceiverRoom)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<RoomSwapRequest>> GetIncomingForStudentAsync(int studentId)
    {
        return await _context.RoomSwapRequests
            .AsNoTracking()
            .Include(r => r.Sender)
            .Include(r => r.Receiver)
            .Include(r => r.SenderRoom)
            .Include(r => r.ReceiverRoom)
            .Where(r => r.ReceiverId == studentId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<RoomSwapRequest>> GetOutgoingForStudentAsync(int studentId)
    {
        return await _context.RoomSwapRequests
            .AsNoTracking()
            .Include(r => r.Sender)
            .Include(r => r.Receiver)
            .Include(r => r.SenderRoom)
            .Include(r => r.ReceiverRoom)
            .Where(r => r.SenderId == studentId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> HasPendingRequestBetweenAsync(int senderId, int receiverId)
    {
        return await _context.RoomSwapRequests
            .AsNoTracking()
            .AnyAsync(r =>
                r.Status == RoomSwapStatus.Pending &&
                ((r.SenderId == senderId && r.ReceiverId == receiverId) ||
                 (r.SenderId == receiverId && r.ReceiverId == senderId)));
    }

    public async Task<bool> HasAnyPendingRequestInvolvingAsync(int studentId)
    {
        return await _context.RoomSwapRequests
            .AsNoTracking()
            .AnyAsync(r =>
                r.Status == RoomSwapStatus.Pending &&
                (r.SenderId == studentId || r.ReceiverId == studentId));
    }

    public async Task AddAsync(RoomSwapRequest request)
    {
        await _context.RoomSwapRequests.AddAsync(request);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(RoomSwapRequest request)
    {
        _context.RoomSwapRequests.Update(request);
        await _context.SaveChangesAsync();
    }
}

// using DormMS.Server.Models;
// using DormMS.Server.Repositories.Interfaces;
// using Microsoft.EntityFrameworkCore;

// public class RoomSwapRequestRepository : IRoomSwapRequestRepository
// {
//     private readonly AppDbContext _context;

//     public RoomSwapRequestRepository(AppDbContext context)
//     {
//         _context = context;
//     }

//     public async Task<RoomSwapRequest?> GetByIdAsync(int id)
//     {
//         return await _context.RoomSwapRequests
//             .AsNoTracking()
//             .Include(r => r.Sender)
//             .Include(r => r.Receiver)
//             .Include(r => r.SenderRoom)
//             .Include(r => r.ReceiverRoom)
//             .FirstOrDefaultAsync(r => r.Id == id);
//     }

//     // Trackable verzija — koristi se kad servis treba da menja status (RespondAsync)
//     public async Task<RoomSwapRequest?> GetByIdTrackedAsync(int id)
//     {
//         return await _context.RoomSwapRequests
//             .Include(r => r.Sender)
//             .Include(r => r.Receiver)
//             .Include(r => r.SenderRoom)
//             .Include(r => r.ReceiverRoom)
//             .FirstOrDefaultAsync(r => r.Id == id);
//     }

//     public async Task<List<RoomSwapRequest>> GetIncomingForStudentAsync(int studentId)
//     {
//         return await _context.RoomSwapRequests
//             .AsNoTracking()
//             .Include(r => r.Sender)
//             .Include(r => r.Receiver)
//             .Include(r => r.SenderRoom)
//             .Include(r => r.ReceiverRoom)
//             .Where(r => r.ReceiverId == studentId)
//             .OrderByDescending(r => r.CreatedAt)
//             .ToListAsync();
//     }

//     public async Task<List<RoomSwapRequest>> GetOutgoingForStudentAsync(int studentId)
//     {
//         return await _context.RoomSwapRequests
//             .AsNoTracking()
//             .Include(r => r.Sender)
//             .Include(r => r.Receiver)
//             .Include(r => r.SenderRoom)
//             .Include(r => r.ReceiverRoom)
//             .Where(r => r.SenderId == studentId)
//             .OrderByDescending(r => r.CreatedAt)
//             .ToListAsync();
//     }

//     public async Task<bool> HasPendingRequestBetweenAsync(int senderId, int receiverId)
//     {
//         return await _context.RoomSwapRequests
//             .AsNoTracking()
//             .AnyAsync(r =>
//                 r.Status == RoomSwapStatus.Pending &&
//                 ((r.SenderId == senderId && r.ReceiverId == receiverId) ||
//                  (r.SenderId == receiverId && r.ReceiverId == senderId)));
//     }

//     public async Task<bool> HasAnyPendingRequestInvolvingAsync(int studentId)
//     {
//         return await _context.RoomSwapRequests
//             .AsNoTracking()
//             .AnyAsync(r =>
//                 r.Status == RoomSwapStatus.Pending &&
//                 (r.SenderId == studentId || r.ReceiverId == studentId));
//     }

//     public async Task AddAsync(RoomSwapRequest request)
//     {
//         await _context.RoomSwapRequests.AddAsync(request);
//         await _context.SaveChangesAsync();
//     }

//     public async Task UpdateAsync(RoomSwapRequest request)
//     {
//         _context.RoomSwapRequests.Update(request);
//         await _context.SaveChangesAsync();
//     }
// }