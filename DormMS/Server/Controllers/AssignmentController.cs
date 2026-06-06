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
}
