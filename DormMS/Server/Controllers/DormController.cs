using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DormMS.Server.Services.Interfaces;
using DormMS.Server.Models;
using DormMS.Server.DTOs;

[ApiController]
[Route("api/[controller]")]
public class DormController : ControllerBase
{
    private readonly IDormService _dormService;

    public DormController(IDormService dormService)
    {
        _dormService = dormService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<DormGetDto>>> GetDorms()
    {
        return Ok(await _dormService.GetAllAsync());
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin, DormManager")]
    //[Authorize(Roles = "Manager")]
    public async Task<ActionResult<DormGetDto>> GetDorm(int id)
    {
        var dorm = await _dormService.GetByIdAsync(id);
        if (dorm is null)
            return NotFound();

        return dorm;
    }

    [HttpPost]
    //[Authorize(Policy = "AdminOnly")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Dorm>> CreateDorm(DormCreateUpdateDto dto)
    {
        var dorm = await _dormService.CreateAsync(dto);
        if (dorm is null)
            return BadRequest();

        return CreatedAtAction(nameof(GetDorm), new { id = dorm.Id }, dorm);
    }

    [HttpPut("{id}")]
    //[Authorize(Policy = "AdminOnly")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateDorm(int id, DormCreateUpdateDto dto)
    {
        var result = await _dormService.UpdateAsync(id, dto);
        if (result is null)
            return NotFound();
        if (result is false)
            return BadRequest();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteDorm(int id)
    {
        var deleted = await _dormService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpPost("{id}/generate-rooms")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GenerateRooms(int id, GenerateRoomsDto dto)
    {
        if (id != dto.DormId)
            return BadRequest("DormId mismatch");

        try
        {
            await _dormService.GenerateRoomsAsync(dto);
            return Ok(new { message = "Sobe su uspešno generisane." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}/rooms")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAllRooms(int id)
    {
        try
        {
            var deletedCount = await _dormService.DeleteAllRoomsForDormAsync(id);
            return Ok(new { message = $"Obrisano {deletedCount} soba.", deletedCount });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
