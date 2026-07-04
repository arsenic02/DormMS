using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DormMS.Server.Services.Interfaces;
using DormMS.Server.Models;
using DormMS.Server.DTOs;

[ApiController]
[Route("api/[controller]")]
public class LaundryController : ControllerBase
{
    private readonly ILaundryService _laundryService;

    public LaundryController(ILaundryService laundryService)
    {
        _laundryService = laundryService;
    }

    [HttpGet("machines")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<LaundryMachineGetDto>>> GetMachines()
    {
        return Ok(await _laundryService.GetAllMachinesAsync());
    }

    [HttpGet("machines/dorm/{dormId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<LaundryMachineGetDto>>> GetMachinesByDorm(int dormId)
    {
        return Ok(await _laundryService.GetMachinesByDormAsync(dormId));
    }

    [HttpGet("machines/{id}")]
    [Authorize]
    public async Task<ActionResult<LaundryMachineGetDto>> GetMachine(int id)
    {
        var machine = await _laundryService.GetMachineByIdAsync(id);
        if (machine is null)
            return NotFound();
        return machine;
    }

    [HttpPost("machines")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LaundryMachine>> CreateMachine(LaundryMachineCreateUpdateDto dto)
    {
        var machine = await _laundryService.CreateMachineAsync(dto);
        if (machine is null)
            return BadRequest();
        return CreatedAtAction(nameof(GetMachine), new { id = machine.Id }, machine);
    }

    [HttpPut("machines/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateMachine(int id, LaundryMachineCreateUpdateDto dto)
    {
        var result = await _laundryService.UpdateMachineAsync(id, dto);
        if (result is null)
            return NotFound();
        if (result is false)
            return BadRequest();
        return NoContent();
    }

    [HttpDelete("machines/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteMachine(int id)
    {
        var deleted = await _laundryService.DeleteMachineAsync(id);
        if (!deleted)
            return NotFound();
        return NoContent();
    }

    [HttpPatch("machines/{id}/toggle-status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleMachineStatus(int id)
    {
        await _laundryService.ToggleMachineStatusAsync(id);
        return NoContent();
    }

    [HttpGet("machines/{machineId}/slots")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<LaundrySlotDto>>> GetAvailableSlots(int machineId, [FromQuery] DateTime date)
    {
        return Ok(await _laundryService.GetAvailableSlotsAsync(machineId, date));
    }

    [HttpPost("reservations")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<LaundryReservationGetDto>> CreateReservation(LaundryReservationCreateDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null)
            return Unauthorized();

        if (!int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized();

        var studentId = await GetStudentIdFromUserId(userId);
        if (studentId is null)
            return Unauthorized();

        var result = await _laundryService.CreateReservationAsync(dto, studentId.Value);
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetStudentReservations), result.Value);
    }

    [HttpDelete("reservations/{id}")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> CancelReservation(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null)
            return Unauthorized();

        if (!int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized();

        var studentId = await GetStudentIdFromUserId(userId);
        if (studentId is null)
            return Unauthorized();

        var result = await _laundryService.CancelReservationAsync(id, studentId.Value);
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return NoContent();
    }

    [HttpGet("reservations/student")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<IEnumerable<LaundryReservationGetDto>>> GetStudentReservations()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null)
            return Unauthorized();

        if (!int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized();

        var studentId = await GetStudentIdFromUserId(userId);
        if (studentId is null)
            return Unauthorized();

        return Ok(await _laundryService.GetStudentReservationsAsync(studentId.Value));
    }

    [HttpGet("reservations/machine/{machineId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<LaundryReservationGetDto>>> GetMachineReservations(int machineId)
    {
        return Ok(await _laundryService.GetMachineReservationsAsync(machineId));
    }

    private async Task<int?> GetStudentIdFromUserId(int userId)
    {
        using var scope = HttpContext.RequestServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.StudentId)
            .FirstOrDefaultAsync();
    }
}
