namespace DormMS.Server.Controllers;

using DormMS.Server.Repositories.Interfaces;
using DormMS.Server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IStudentRepository _studentRepository;
    private readonly IDormManagerRepository _dormManagerRepository;

    public NotificationController(
        INotificationService notificationService,
        IStudentRepository studentRepository,
        IDormManagerRepository dormManagerRepository)
    {
        _notificationService = notificationService;
        _studentRepository = studentRepository;
        _dormManagerRepository = dormManagerRepository;
    }

    private int GetCurrentUserId()
    {
        var sub = User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("Token ne sadrži korisnički identifikator.");
        return int.Parse(sub);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyNotifications(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] bool unreadOnly = false)
    {
        var userId = GetCurrentUserId();

        var student = await _studentRepository.GetByUserIdAsync(userId);
        if (student is null)
            return NotFound("Vaš studentski profil nije pronađen.");

        var result = await _notificationService.GetForStudentPagedAsync(student.Id, pageNumber, pageSize, unreadOnly);
        return Ok(result);
    }

    [HttpGet("my-dorm")]
    [Authorize(Roles = "DormManager")]
    public async Task<IActionResult> GetMyDormNotifications(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] bool unreadOnly = false)
    {
        var userId = GetCurrentUserId();

        var dormManager = await _dormManagerRepository.GetByUserIdAsync(userId);
        if (dormManager is null)
            return NotFound("Vaš DormManager profil nije pronađen.");

        var result = await _notificationService.GetForDormPagedAsync(dormManager.DormId, pageNumber, pageSize, unreadOnly);
        return Ok(result);
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return NoContent();
    }
}
