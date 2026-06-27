namespace DormMS.Server.Services.Interfaces;

using DormMS.Server.DTOs;

public interface INotificationService
{
    Task NotifyStudentAsync(int studentId, string message);
    Task NotifyDormAsync(int dormId, string message);
    Task<List<NotificationDto>> GetForStudentAsync(int studentId, bool unreadOnly = false);
    //Task<List<NotificationDto>> GetForDormAsync(int dormId, bool unreadOnly = false);
     Task<PagedResultDto<NotificationDto>> GetForDormPagedAsync(int dormId, int pageNumber, int pageSize, bool unreadOnly = false);
    Task MarkAsReadAsync(int notificationId);
}