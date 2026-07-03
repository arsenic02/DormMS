namespace DormMS.Server.Controllers;

using DormMS.Server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/move-out-requests")]
[Authorize(Roles = "Student")]
public class MoveOutRequestController : ControllerBase
{
    private readonly IMoveOutRequestService _moveOutService;

    public MoveOutRequestController(IMoveOutRequestService moveOutService)
    {
        _moveOutService = moveOutService;
    }

    private int GetCurrentUserId()
    {
        var sub = User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("Token ne sadrži korisnički identifikator.");
        return int.Parse(sub);
    }

    [HttpPost]
    public async Task<ActionResult<MoveOutRequestDto>> Create()
    {
        try
        {
            var studentId = GetCurrentUserId();
            var result = await _moveOutService.CreateAsync(studentId);
            return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("my")]
    public async Task<ActionResult<MoveOutRequestDto>> GetMyRequest()
    {
        try
        {
            var studentId = GetCurrentUserId();
            var result = await _moveOutService.GetMyRequestAsync(studentId);
            if (result == null)
                return NotFound("Nemate aktivan zahtev za iseljenje.");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
