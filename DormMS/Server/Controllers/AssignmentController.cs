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
    [HttpPost("assign-single/{rankingResultId}")]
    public async Task<ActionResult<StudentAssignmentDto>> AssignSingle(int rankingResultId)
    {
        try
        {
            var result = await _assignmentService.AssignSingleStudentAsync(rankingResultId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize(Roles = "Admin,DormManager")]
    [HttpPost("remove-from-room/{studentId}")]
    public async Task<IActionResult> RemoveStudentFromRoom(int studentId)
    {
        try
        {
            await _assignmentService.RemoveStudentFromRoomAsync(studentId);
            return Ok(new { message = "Student je uklonjen iz sobe." });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
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
