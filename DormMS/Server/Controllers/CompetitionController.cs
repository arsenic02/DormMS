using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DormMS.Server.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class CompetitionController : ControllerBase
{
    private readonly ICompetitionService _competitionService;

    public CompetitionController(ICompetitionService competitionService)
    {
        _competitionService = competitionService;
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
        return Ok(await _competitionService.GetOpenCompetitionsAsync());
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
    [Authorize(Roles = "Student")]
    [HttpPost("{competitionId}/apply")]
    public async Task<ActionResult<CompetitionApplicationGetDto>> ApplyToCompetition(
    int competitionId, CompetitionApplicationCreateDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();
        
        // var studentId = await _context.Users
        // .Where(u => u.Id == userId)
        // .Select(u => u.StudentId)
        // .FirstOrDefaultAsync();

        dto.CompetitionId = competitionId;
        var result = await _competitionService.ApplyAsync(userId.Value, dto);

        if (result.Error is not null)
            return BadRequest(result.Error);

        return Ok(result.Application);
    }

    // [Authorize(Roles = "Student")]
    // [HttpPost("{competitionId}/apply")]
    // public async Task<ActionResult<CompetitionApplicationGetDto>> ApplyToCompetition(int competitionId, CompetitionApplicationCreateDto dto)
    // {
    //     var userId = GetCurrentUserId();
    //     if (userId is null)
    //         return Unauthorized();

    //     dto.CompetitionId = competitionId;
    //     var application = await _competitionService.ApplyAsync(userId.Value, dto);
    //     if (application is null)
    //         return BadRequest("Failed to apply to competition");

    //     return Ok(application);
    // }

    [Authorize(Roles = "Admin,DormManager")]
    [HttpGet("{competitionId}/applications")]
    public async Task<ActionResult<IEnumerable<CompetitionApplicationGetDto>>> GetApplications(int competitionId)
    {
        return Ok(await _competitionService.GetApplicationsByCompetitionIdAsync(competitionId));
    }

    [Authorize(Roles = "Admin,DormManager")]
    [HttpPut("applications/{id}")]
    public async Task<ActionResult<CompetitionApplicationGetDto>> UpdateApplication(int id, CompetitionApplicationUpdateDto dto)
    {
        var application = await _competitionService.UpdateApplicationAsync(id, dto);
        if (application is null)
            return NotFound();

        return Ok(application);
    }

    [Authorize(Roles = "Admin,DormManager")]
    [HttpDelete("applications/{id}")]
    public async Task<IActionResult> DeleteApplication(int id)
    {
        var deleted = await _competitionService.DeleteApplicationAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [Authorize(Roles = "Admin,DormManager")]
    [HttpPost("bulk-apply")]
    public async Task<ActionResult<CompetitionImportResultDto>> BulkApply(BulkApplicationDto dto)
    {
        var result = await _competitionService.BulkApplyAsync(dto);
        return Ok(result);
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
