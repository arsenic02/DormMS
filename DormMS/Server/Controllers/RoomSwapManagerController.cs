namespace DormMS.Server.Controllers;

using DormMS.Server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/room-swap-requests/manager")]
[Authorize(Roles = "DormManager")]
public class RoomSwapManagerController : ControllerBase
{
    private readonly IRoomSwapRequestService _roomSwapService;

    public RoomSwapManagerController(IRoomSwapRequestService roomSwapService)
    {
        _roomSwapService = roomSwapService;
    }

    private int GetCurrentUserId()
    {
        try
        {
            var sub = User.FindFirst("sub")?.Value
                ?? throw new UnauthorizedAccessException("Token ne sadrži korisnički identifikator.");
            return int.Parse(sub);
        }
        catch (Exception ex)
        {
            throw new UnauthorizedAccessException("Neuspešno dobijanje korisničkog identifikatora iz tokena.", ex);
        }
    }

    [HttpGet("pending")]
    public async Task<ActionResult<List<RoomSwapRequestDto>>> GetPending()
    {
        try
        {
            var managerId = GetCurrentUserId();
            var result = await _roomSwapService.GetPendingForManagerAsync(managerId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/approve")]
    public async Task<ActionResult<RoomSwapRequestDto>> Approve(int id)
    {
        try
        {
            var managerId = GetCurrentUserId();
            var result = await _roomSwapService.ApproveAsync(managerId, id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }   
    }

    [HttpPut("{id}/reject")]
    public async Task<ActionResult<RoomSwapRequestDto>> Reject(int id)
    {
        try
        {
            var managerId = GetCurrentUserId();
            var result = await _roomSwapService.RejectAsync(managerId, id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}