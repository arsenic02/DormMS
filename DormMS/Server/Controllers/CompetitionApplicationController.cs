using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DormMS.Server.Services.Interfaces;
using DormMS.Server.DTOs;

[ApiController]
[Route("api/[controller]")]
public class CompetitionApplicationController : ControllerBase
{
    private readonly ICompetitionApplicationService _applicationService;

    public CompetitionApplicationController(ICompetitionApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    [Authorize(Roles = "Student")]
    [HttpPost("{competitionId}/apply")]
    public async Task<ActionResult<CompetitionApplicationGetDto>> ApplyToCompetition(
        int competitionId, CompetitionApplicationCreateDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        dto.CompetitionId = competitionId;
        var result = await _applicationService.ApplyAsync(userId.Value, dto);

        if (result.Error is not null)
            return BadRequest(result.Error);

        return Ok(result.Application);
    }

    [Authorize(Roles = "Student")]
    [HttpPost("upload-document")]
    public async Task<IActionResult> UploadDocument(IFormFile file)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        if (file is null || file.Length == 0)
            return BadRequest("Fajl nije prosledjen");

        if (Path.GetExtension(file.FileName).ToLowerInvariant() != ".pdf")
            return BadRequest("Dozvoljen je samo PDF format");

        const long maxSize = 5 * 1024 * 1024; // 5MB
        if (file.Length > maxSize)
            return BadRequest("Fajl prevazilazi maksimalnu velicinu od 5MB");

        var result = await _applicationService.SaveDocumentAsync(file);
        return Ok(new { documentUrl = result });
    }

    [Authorize(Roles = "Admin,DormManager")]
    [HttpGet("{competitionId}/applications")]
    public async Task<ActionResult<IEnumerable<CompetitionApplicationGetDto>>> GetApplications(int competitionId)
    {
        return Ok(await _applicationService.GetApplicationsByCompetitionIdAsync(competitionId));
    }

    [Authorize(Roles = "Admin,DormManager")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CompetitionApplicationGetDto>>> GetAllApplications()
    {
        return Ok(await _applicationService.GetAllAsync());
    }

    [Authorize(Roles = "Admin,DormManager")]
    [HttpGet("verified")]
    public async Task<ActionResult<IEnumerable<CompetitionApplicationGetDto>>> GetVerifiedApplications()
    {
        return Ok(await _applicationService.GetVerifiedApplicationsAsync());
    }

    [Authorize(Roles = "Admin,DormManager")]
    [HttpPut("applications/{id}")]
    public async Task<ActionResult<CompetitionApplicationGetDto>> UpdateApplication(int id, CompetitionApplicationUpdateDto dto)
    {
        var application = await _applicationService.UpdateApplicationAsync(id, dto);
        if (application is null)
            return NotFound();

        return Ok(application);
    }

    [Authorize(Roles = "Admin,DormManager")]
    [HttpDelete("applications/{id}")]
    public async Task<IActionResult> DeleteApplication(int id)
    {
        var deleted = await _applicationService.DeleteApplicationAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [Authorize(Roles = "Admin,DormManager")]
    [HttpDelete("competition/{competitionId}/applications")]
    public async Task<ActionResult<int>> DeleteApplicationsByCompetition(int competitionId)
    {
        var count = await _applicationService.DeleteApplicationsByCompetitionIdAsync(competitionId);
        return Ok(new { Message = $"Successfully deleted {count} applications for competition {competitionId}.", Count = count });
    }

    [Authorize(Roles = "Admin,DormManager")]
    [HttpPost("bulk-apply")]
    public async Task<ActionResult<CompetitionImportResultDto>> BulkApply(BulkApplicationDto dto)
    {
        var result = await _applicationService.BulkApplyAsync(dto);
        return Ok(result);
    }

    [Authorize(Roles = "Admin,DormManager")]
    [HttpPost("bulk-verify")]
    public async Task<ActionResult<BulkVerifyApplicationsResultDto>> BulkVerifyApplications(BulkVerifyApplicationsDto dto)
    {
        var result = await _applicationService.BulkVerifyApplicationsAsync(dto);
        return Ok(result);
    }

    private int? GetCurrentUserId()
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return int.TryParse(idValue, out var userId) ? userId : null;
    }
}
