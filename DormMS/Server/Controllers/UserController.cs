namespace DormMS.Server.Controllers;

using DormMS.Server.DTOs;
using DormMS.Server.Models;
using DormMS.Server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static DormMS.Server.Services.Interfaces.IUserService;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("dorm-manager")]
    public async Task<IActionResult> CreateDormManager([FromBody] DormManagerCreateDto dto)
    {
        var user = await _userService.CreateDormManagerAsync(dto);
        return CreatedAtAction(nameof(GetPaged), new { id = user.Id }, new UserGetDto
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            ProfileName = null
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var result = await _userService.DeleteAsync(id);

        return result switch
        {
            UserDeleteResult.NotFound => NotFound(),
            UserDeleteResult.CannotDeleteAdmin => BadRequest(new { message = "Administratorski nalog se ne može obrisati." }),
            _ => NoContent()
        };
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] UserRole? role = null)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var result = await _userService.GetPagedAsync(pageNumber, pageSize, role);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/me")]
    public async Task<IActionResult> GetAdminProfile()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var profile = await _userService.GetAdminProfileAsync(userId.Value);
        if (profile is null)
            return NotFound();

        return Ok(profile);
    }

    [Authorize(Roles = "DormManager")]
    [HttpGet("dorm-manager/me")]
    public async Task<IActionResult> GetDormManagerProfile()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var profile = await _userService.GetDormManagerProfileAsync(userId.Value);
        if (profile is null)
            return NotFound();

        return Ok(profile);
    }

    private int? GetCurrentUserId()
    {
        var idValue = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        return int.TryParse(idValue, out var userId) ? userId : null;
    }
}