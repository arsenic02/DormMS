using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DormMS.Server.Services.Interfaces;
using DormMS.Server.DTOs;

[ApiController]
[Route("api/[controller]")]
public class AssignmentController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;

    public AssignmentController(IAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    [Authorize(Roles = "Admin,DormManager")]
    [HttpPost("assign")]
    public async Task<ActionResult<AssignmentResultDto>> AssignStudentsToDorms(AssignmentRequestDto dto)
    {
        var result = await _assignmentService.AssignStudentsToDormsAsync(dto.CompetitionId);
        
        if (result.Errors.Any() && result.SuccessfullyAssigned == 0)
            return BadRequest(result);

        return Ok(result);
    }

//mislim da mi nece trebati, stara metoda, jer sam napravio novu za starije i brucose
    // [Authorize(Roles = "Admin,DormManager")]
    // [HttpPost("rank")]
    // public async Task<ActionResult<List<StudentAssignmentDto>>> RankStudents(AssignmentRequestDto dto)
    // {
    //     var rankedStudents = await _assignmentService.RankStudentsAsync(dto.CompetitionId);
    //     return Ok(rankedStudents);
    // }

    /// <summary>
    /// Rangira starije studente (ne brucose) za konkurs - grupisano po fakultetima
    /// </summary>
    [Authorize(Roles = "Admin,DormManager")]
    [HttpPost("rank-older")]
    public async Task<ActionResult<OlderStudentsRankingResultDto>> RankOlderStudents(AssignmentRequestDto dto)
    {
        var result = await _assignmentService.RankOlderStudentsAsync(dto.CompetitionId);

        if (result.Errors.Any())
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Rangira brucose za konkurs - grupisano po fakultetima
    /// Formula: bb = po * 8 + socijalni bonus (1 bod ako je prihod < 50% prosecne plate)
    /// </summary>
    [Authorize(Roles = "Admin,DormManager")]
    [HttpPost("rank-freshmen")]
    public async Task<ActionResult<FreshmanRankingResultDto>> RankFreshmen(AssignmentRequestDto dto)
    {
        var result = await _assignmentService.RankFreshmenAsync(dto.CompetitionId);

        if (result.Errors.Any())
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "Admin,DormManager")]
    [HttpPost("assign-ranked")]
    public async Task<ActionResult<AssignmentResultDto>> AssignRankedStudents([FromBody] List<StudentAssignmentDto> rankedStudents)
    {
        var result = await _assignmentService.AssignRankedStudentsAsync(rankedStudents);
        
        if (result.Errors.Any() && result.SuccessfullyAssigned == 0)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "Admin,DormManager")]
    [HttpPost("bulk-assign-ranked")]
    public async Task<ActionResult<AssignmentResultDto>> BulkAssignRankedStudents([FromBody] List<StudentAssignmentDto> rankedStudents)
    {
        var result = await _assignmentService.BulkAssignRankedStudentsAsync(rankedStudents);
        
        if (result.Errors.Any() && result.SuccessfullyAssigned == 0)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "Admin,DormManager")]
    [HttpPost("reset")]
    public async Task<IActionResult> ResetAssignments()
    {
        await _assignmentService.ResetAssignmentsAsync();
        return Ok(new { Message = "All room assignments have been cleared and room occupancy reset to 0." });
    }
}
