namespace DormMS.Server.Controllers;

using DormMS.Server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/move-out-requests/manager")]
[Authorize(Roles = "DormManager")]
public class MoveOutManagerController : ControllerBase
{
    private readonly IMoveOutRequestService _moveOutService;

    public MoveOutManagerController(IMoveOutRequestService moveOutService)
    {
        _moveOutService = moveOutService;
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
    public async Task<ActionResult<List<MoveOutRequestDto>>> GetPending()
    {
        try
        {
            var managerId = GetCurrentUserId();
            var result = await _moveOutService.GetPendingForManagerAsync(managerId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/approve")]
    public async Task<ActionResult<MoveOutRequestDto>> Approve(int id)
    {
        try
        {
            var managerId = GetCurrentUserId();
            var result = await _moveOutService.ApproveAsync(managerId, id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/reject")]
    public async Task<ActionResult<MoveOutRequestDto>> Reject(int id)
    {
        try
        {
            var managerId = GetCurrentUserId();
            var result = await _moveOutService.RejectAsync(managerId, id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
