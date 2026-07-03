namespace DormMS.Server.Controllers;

using DormMS.Server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/room-swap-requests")]
[Authorize(Roles = "Student")]
public class RoomSwapRequestController : ControllerBase
{
    private readonly IRoomSwapRequestService _roomSwapService;

    public RoomSwapRequestController(IRoomSwapRequestService roomSwapService)
    {
        _roomSwapService = roomSwapService;
    }

    private int GetCurrentUserId()
    {
        var sub = User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("Token ne sadrži korisnički identifikator.");
        return int.Parse(sub);
    }

    [HttpPost]
    public async Task<ActionResult<RoomSwapRequestDto>> Create([FromBody] CreateRoomSwapRequestDto dto)
    {
        try
        {
            var senderId = GetCurrentUserId();
            var result = await _roomSwapService.CreateAsync(senderId, dto);
            return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/respond")]
    public async Task<ActionResult<RoomSwapRequestDto>> Respond(int id, [FromBody] RespondRoomSwapRequestDto dto)
    {
        try
        {
            var receiverId = GetCurrentUserId();
            var result = await _roomSwapService.RespondAsync(receiverId, id, dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("incoming")]
    public async Task<ActionResult<List<RoomSwapRequestDto>>> GetIncoming()
    {
        try
        {
            var studentId = GetCurrentUserId();
            var result = await _roomSwapService.GetIncomingAsync(studentId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("outgoing")]
    public async Task<ActionResult<List<RoomSwapRequestDto>>> GetOutgoing()
    {
        try
        {
            var studentId = GetCurrentUserId();
            var result = await _roomSwapService.GetOutgoingAsync(studentId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}