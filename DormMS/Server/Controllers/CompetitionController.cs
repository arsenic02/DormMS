using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DormMS.Server.Services.Interfaces;
using DormMS.Server.DTOs;

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
    [HttpPut("{id}/open")]
    public async Task<ActionResult<CompetitionGetDto>> OpenCompetition(int id)
    {
        var competition = await _competitionService.OpenCompetitionAsync(id);
        if (competition is null)
            return BadRequest("Competition not found or cannot be opened");

        return Ok(competition);
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

        var competition = await _competitionService.CreateAsync(dto, userId.Value, userRole);
        if (competition is null)
            return BadRequest("Failed to create competition");

        return CreatedAtAction(nameof(GetCompetition), new { id = competition.Id }, competition);
    }
    // [Authorize(Roles = "Admin,DormManager")]
    // [HttpPost]
    // public async Task<ActionResult<CompetitionGetDto>> CreateCompetition(CompetitionCreateDto dto)
    // {
    //     var userId = GetCurrentUserId();
    //     if (userId is null)
    //         return Unauthorized();

    //     var userRole = GetUserRole();
    //     int? adminId = userRole == "Admin" ? userId : null;
    //     int? dormManagerId = userRole == "DormManager" ? userId : null;

    //     var competition = await _competitionService.CreateAsync(dto, adminId, dormManagerId);
    //     if (competition is null)
    //         return BadRequest("Failed to create competition");

    //     return CreatedAtAction(nameof(GetCompetition), new { id = competition.Id }, competition);
    // }

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
