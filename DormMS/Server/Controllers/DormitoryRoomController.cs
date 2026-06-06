using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DormMS.Server.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class DormitoryRoomController : ControllerBase
{
    private readonly IDormitoryRoomService _dormitoryRoomService;

    public DormitoryRoomController(IDormitoryRoomService dormitoryRoomService)
    {
        _dormitoryRoomService = dormitoryRoomService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DormitoryRoom>>> GetDormitoryRooms()
    {
        return Ok(await _dormitoryRoomService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DormitoryRoom>> GetDormitoryRoom(int id)
    {
        var room = await _dormitoryRoomService.GetByIdAsync(id);
        if (room is null)
            return NotFound();

        return room;
    }

    [HttpPost]
    //[Authorize(Policy = "AdminOnly")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DormitoryRoom>> CreateDormitoryRoom(DormitoryRoomCreateUpdateDto dto)
    {
        var room = await _dormitoryRoomService.CreateAsync(dto);
        if (room is null)
            return BadRequest();

        return CreatedAtAction(nameof(GetDormitoryRoom), new { id = room.Id }, room);
    }

    [HttpPut("{id}")] 
    //[Authorize(Policy = "AdminOnly")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateDormitoryRoom(int id, DormitoryRoomCreateUpdateDto dto)
    {
        var result = await _dormitoryRoomService.UpdateAsync(id, dto);
        if (result is null)
            return NotFound();
        if (result is false)
            return BadRequest();

        return NoContent();
    }

    [HttpDelete("{id}")]    
    //[Authorize(Policy = "AdminOnly")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteDormitoryRoom(int id)
    {
        var deleted = await _dormitoryRoomService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
