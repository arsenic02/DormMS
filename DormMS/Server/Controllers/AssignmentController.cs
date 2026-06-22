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

    [Authorize(Roles = "Admin,DormManager")]
    [HttpPost("assign-older")]
    public async Task<ActionResult<AssignmentResultDto>> AssignOlderStudentsToDorms(AssignmentRequestDto dto)
    {
        var result = await _assignmentService.AssignOlderStudentsToDormsAsync(dto.CompetitionId);

        if (result.Errors.Any() && result.SuccessfullyAssigned == 0)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "Admin,DormManager")]
    [HttpPost("assign-freshmen")]
    public async Task<ActionResult<AssignmentResultDto>> AssignFreshmenToDorms(AssignmentRequestDto dto)
    {
        var result = await _assignmentService.AssignFreshmenToDormsAsync(dto.CompetitionId);

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
    [HttpGet("results/{competitionId}")]
    public async Task<ActionResult> GetSavedResults(int competitionId, [FromQuery] bool isFreshmanView)
    {
        var result = await _assignmentService.GetSavedRankingResultsAsync(competitionId, isFreshmanView);
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

    // [Authorize(Roles = "Admin,DormManager")]
    [HttpPost("reset-flow")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResetFlow()
    {
        var success = await _assignmentService.ResetFlowAsync();
        if (!success)
            return BadRequest("Reset nije uspeo.");
        return Ok(new { message = "Ceo flow (rangiranje + dodela) je resetovan za sve studente i sobe." });
    }

    [HttpPost("reset-assignment/{competitionId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResetAssignment(int competitionId)
    {
        var success = await _assignmentService.ResetAssignmentAsync(competitionId);
        if (!success)
            return BadRequest("Nema dodeljenih studenata za ovaj konkurs, ili konkurs ne postoji.");

        return Ok(new { message = $"Dodela sobe je resetovana za konkurs {competitionId}." });
    }
}
