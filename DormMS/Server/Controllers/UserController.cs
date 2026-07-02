namespace DormMS.Server.Controllers;

using DormMS.Server.DTOs;
using DormMS.Server.Models;
using DormMS.Server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static DormMS.Server.Services.Interfaces.IUserService;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

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
}