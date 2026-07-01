using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DormMS.Server.Services.Interfaces;
using DormMS.Server.DTOs;
using DormMS.Server.Models;

[ApiController]
[Route("api/[controller]")]
public class CompetitionController : ControllerBase
{
    private readonly ICompetitionService _competitionService;
    private readonly IStudentService _studentService;

    public CompetitionController(ICompetitionService competitionService, IStudentService studentService)
    {
        _competitionService = competitionService;
        _studentService = studentService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CompetitionGetDto>>> GetCompetitions()
    {
        return Ok(await _competitionService.GetAllAsync());
    }

    [Authorize(Roles = "Admin,DormManager")]
    [HttpGet("paged")]
    public async Task<ActionResult<PagedResultDto<CompetitionGetDto>>> GetCompetitionsPaged(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] CompetitionType? type = null,
    [FromQuery] CompetitionStatus? status = null)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var result = await _competitionService.GetPagedAsync(pageNumber, pageSize, type, status);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,DormManager")]
    [HttpPut("{id}/toggle-status")]
    public async Task<ActionResult<CompetitionGetDto>> ToggleCompetitionStatus(int id)
    {
        var competition = await _competitionService.ToggleCompetitionStatusAsync(id);
        if (competition is null)
            return NotFound();

        return Ok(competition);
    }

    [HttpGet("open")]
    public async Task<ActionResult<IEnumerable<CompetitionGetDto>>> GetOpenCompetitions()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var student = await _studentService.GetByUserIdAsync(userId.Value);
        if (student is null)
            return NotFound("Student nije pronađen");

        return Ok(await _competitionService.GetOpenCompetitionsAsync(student.Id));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CompetitionGetDto>> GetCompetition(int id)
    {
        var competition = await _competitionService.GetByIdAsync(id);
        if (competition is null)
            return NotFound();

        return competition;
    }

    [Authorize(Roles = "Admin,DormManager")]
    [HttpPost]
    public async Task<ActionResult<CompetitionGetDto>> CreateCompetition(CompetitionCreateDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var userRole = GetUserRole();
        if (string.IsNullOrEmpty(userRole))
            return Unauthorized();

        var result = await _competitionService.CreateAsync(dto, userId.Value, userRole);
        if (result.Error is not null)
            return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetCompetition), new { id = result.Competition!.Id }, result.Competition);
    }

    [Authorize(Roles = "Admin,DormManager")]
    [HttpPut("{id}")]
    public async Task<ActionResult<CompetitionGetDto>> UpdateCompetition(int id, CompetitionUpdateDto dto)
    {
        var competition = await _competitionService.UpdateAsync(id, dto);
        if (competition is null)
            return NotFound();

        return Ok(competition);
    }

    [Authorize(Roles = "Admin,DormManager")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCompetition(int id)
    {
        var deleted = await _competitionService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    private int? GetCurrentUserId()
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return int.TryParse(idValue, out var userId) ? userId : null;
    }

    private string? GetUserRole()
    {
        return User.FindFirstValue(ClaimTypes.Role);
    }
}
