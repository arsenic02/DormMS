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
    [HttpPost("rank")]
    public async Task<ActionResult<List<StudentAssignmentDto>>> RankStudents(AssignmentRequestDto dto)
    {
        var rankedStudents = await _assignmentService.RankStudentsAsync(dto.CompetitionId);
        return Ok(rankedStudents);
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
}
