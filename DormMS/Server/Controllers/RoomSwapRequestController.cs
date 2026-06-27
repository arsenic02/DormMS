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
        var senderId = GetCurrentUserId();
        var result = await _roomSwapService.CreateAsync(senderId, dto);
        return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
    }

    [HttpPost("{id}/respond")]
    public async Task<ActionResult<RoomSwapRequestDto>> Respond(int id, [FromBody] RespondRoomSwapRequestDto dto)
    {
        var receiverId = GetCurrentUserId();
        var result = await _roomSwapService.RespondAsync(receiverId, id, dto);
        return Ok(result);
    }

    [HttpGet("incoming")]
    public async Task<ActionResult<List<RoomSwapRequestDto>>> GetIncoming()
    {
        var studentId = GetCurrentUserId();
        var result = await _roomSwapService.GetIncomingAsync(studentId);
        return Ok(result);
    }

    [HttpGet("outgoing")]
    public async Task<ActionResult<List<RoomSwapRequestDto>>> GetOutgoing()
    {
        var studentId = GetCurrentUserId();
        var result = await _roomSwapService.GetOutgoingAsync(studentId);
        return Ok(result);
    }
}

// // [ApiController]
// // [Route("api/room-swap-requests")]
// // [Authorize(Roles = "Student")]
// using DormMS.Server.Services.Interfaces;
// using Microsoft.AspNetCore.Mvc;

// [ApiController]
// [Route("api/[controller]")]
// public class RoomSwapRequestController : ControllerBase
// {
//     private readonly IRoomSwapRequestService _roomSwapService;

//     public RoomSwapRequestController(IRoomSwapRequestService roomSwapService)
//     {
//         _roomSwapService = roomSwapService;
//     }

//     private int GetCurrentUserId()
//     {
//         var sub = User.FindFirst("sub")?.Value
//             ?? throw new UnauthorizedAccessException("Token ne sadrži korisnički identifikator.");
//         return int.Parse(sub);
//     }

//     [HttpPost]
//     public async Task<ActionResult<RoomSwapRequestDto>> Create([FromBody] CreateRoomSwapRequestDto dto)
//     {
//         var senderId = GetCurrentUserId();
//         var result = await _roomSwapService.CreateAsync(senderId, dto);
//         return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
//     }

//     [HttpPost("{id}/respond")]
//     public async Task<ActionResult<RoomSwapRequestDto>> Respond(int id, [FromBody] RespondRoomSwapRequestDto dto)
//     {
//         var receiverId = GetCurrentUserId();
//         var result = await _roomSwapService.RespondAsync(receiverId, id, dto);
//         return Ok(result);
//     }

//     [HttpGet("incoming")]
//     public async Task<ActionResult<List<RoomSwapRequestDto>>> GetIncoming()
//     {
//         var studentId = GetCurrentUserId();
//         var result = await _roomSwapService.GetIncomingAsync(studentId);
//         return Ok(result);
//     }

//     [HttpGet("outgoing")]
//     public async Task<ActionResult<List<RoomSwapRequestDto>>> GetOutgoing()
//     {
//         var studentId = GetCurrentUserId();
//         var result = await _roomSwapService.GetOutgoingAsync(studentId);
//         return Ok(result);
//     }
// }