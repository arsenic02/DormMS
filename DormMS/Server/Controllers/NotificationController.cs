namespace DormMS.Server.Controllers;

using DormMS.Server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private int GetCurrentUserId()
    {
        var sub = User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("Token ne sadrži korisnički identifikator.");
        return int.Parse(sub);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyNotifications([FromQuery] bool unreadOnly = false)
    {
        var studentId = GetCurrentUserId();
        var result = await _notificationService.GetForStudentAsync(studentId, unreadOnly);
        return Ok(result);
    }

    [HttpGet("dorm/{dormId}")]
    [Authorize(Roles = "DormManager")]
    public async Task<IActionResult> GetDormNotifications(int dormId, [FromQuery] bool unreadOnly = false)
    {
        var result = await _notificationService.GetForDormAsync(dormId, unreadOnly);
        return Ok(result);
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return NoContent();
    }
}