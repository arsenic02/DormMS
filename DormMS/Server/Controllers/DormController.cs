using Microsoft.AspNetCore.Mvc;

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
    public async Task<ActionResult<IEnumerable<Dorm>>> GetDorms()
    {
        return Ok(await _dormService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Dorm>> GetDorm(int id)
    {
        var dorm = await _dormService.GetByIdAsync(id);
        if (dorm is null)
            return NotFound();

        return dorm;
    }

    [HttpPost]
    public async Task<ActionResult<Dorm>> CreateDorm(DormCreateUpdateDto dto)
    {
        var dorm = await _dormService.CreateAsync(dto);
        if (dorm is null)
            return BadRequest();

        return CreatedAtAction(nameof(GetDorm), new { id = dorm.Id }, dorm);
    }

    [HttpPut("{id}")]
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
    public async Task<IActionResult> DeleteDorm(int id)
    {
        var deleted = await _dormService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
