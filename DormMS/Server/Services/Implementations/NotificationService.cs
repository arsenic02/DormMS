namespace DormMS.Server.Services;

using DormMS.Server.DTOs;
using DormMS.Server.Models;
using DormMS.Server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;

    public NotificationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task NotifyStudentAsync(int studentId, string message)
    {
        var notification = new Notification
        {
            StudentId = studentId,
            Message = message,
            // CreatedAt = DateTime.UtcNow
            CreatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)
        };

        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
    }

    public async Task NotifyDormAsync(int dormId, string message)
    {
        var notification = new Notification
        {
            DormId = dormId,
            Message = message,
            // CreatedAt = DateTime.UtcNow
            CreatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)
        };

        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
    }

    public async Task<List<NotificationDto>> GetForStudentAsync(int studentId, bool unreadOnly = false)
    {
        var query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.StudentId == studentId);

        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<PagedResultDto<NotificationDto>> GetForDormPagedAsync(int dormId, int pageNumber, int pageSize, bool unreadOnly = false)
    {
        var query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.DormId == dormId);

        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();

        return new PagedResultDto<NotificationDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId)
            ?? throw new KeyNotFoundException("Notifikacija nije pronađena.");

        notification.IsRead = true;
        await _context.SaveChangesAsync();
    }
}