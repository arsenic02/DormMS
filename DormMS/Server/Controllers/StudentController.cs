using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DormMS.Server.Services.Interfaces;
using DormMS.Server.DTOs;
using DormMS.Server.Models;

[ApiController]
[Route("api/[controller]")]
public class StudentController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StudentGetDto>>> GetStudents()
    {
        return Ok(await _studentService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StudentGetDto>> GetStudent(int id)
    {
        var student = await _studentService.GetByIdAsync(id);
        if (student is null)
            return NotFound();

        return student;
    }

    [Authorize(Roles = "Student")]
    [HttpGet("me")]
    public async Task<ActionResult<StudentGetDto>> GetMyProfile()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var profile = await _studentService.GetByUserIdAsync(userId.Value);
        if (profile is null)
            return NotFound();

        return profile;
    }

    [Authorize(Roles = "Student")]
    [HttpGet("me/room")]
    public async Task<ActionResult<MyRoomGetDto>> GetMyRoom()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var result = await _studentService.GetMyRoomAsync(userId.Value);
        return Ok(result);
    }

    [HttpGet("paged")]
    [Authorize(Roles = "Admin,DormManager")]
    public async Task<ActionResult<PagedResultDto<StudentGetDto>>> GetStudentsPaged(
[FromQuery] int pageNumber = 1,
[FromQuery] int pageSize = 10,
[FromQuery] StudentProfileStatus? profileStatus = null,
[FromQuery] string? faculty = null,
[FromQuery] bool? isFreshman = null,
[FromQuery] FinancialVerificationStatus? financialStatus = null)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var result = await _studentService.GetPagedAsync(pageNumber, pageSize, profileStatus, faculty, isFreshman, financialStatus);
        return Ok(result);
    }

    [Authorize(Roles = "Student")]
    [HttpPut("me/profile")]
    public async Task<ActionResult<StudentGetDto>> CompleteMyProfile(StudentProfileUpdateDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized();

        var profile = await _studentService.CompleteProfileAsync(userId.Value, dto);
        if (profile is null)
            return BadRequest();

        return profile;
    }

    [HttpPost]
    public async Task<ActionResult<Student>> CreateStudent(StudentCreateUpdateDto dto)
    {
        var student = await _studentService.CreateAsync(dto);
        if (student is null)
            return BadRequest();

        return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, student);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStudent(int id, StudentCreateUpdateDto dto)
    {
        var result = await _studentService.UpdateAsync(id, dto);
        if (result is null)
            return NotFound();
        if (result is false)
            return BadRequest();

        return NoContent();
    }

    [HttpPost("import-csv")]
    public async Task<ActionResult<StudentImportResultDto>> ImportFromCsv(IFormFile csvFile)
    {
        if (csvFile == null || csvFile.Length == 0)
            return BadRequest("No file uploaded");

        if (!csvFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only CSV files are allowed");

        var result = await _studentService.ImportFromCsvAsync(csvFile);
        return Ok(result);
    }

    [Authorize(Roles = "Student")]
    [HttpPost("me/financial-status")]
    public async Task<ActionResult<StudentFinancialStatusGetDto>> SubmitFinancialStatus(StudentFinancialStatusCreateDto dto)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
            return Unauthorized();

        var result = await _studentService
            .SubmitFinancialStatusAsync(userId.Value, dto);

        if (result is null)
            return BadRequest();

        return Ok(result);
    }

    [HttpPatch("{studentId}/verify-profile")]
    [Authorize(Roles = "Admin,DormManager")]
    public async Task<IActionResult> VerifyProfile(int studentId, StudentProfileVerificationDto dto)
    {
        var result = await _studentService.VerifyProfileAsync(studentId, dto.Status);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPatch("{studentId}/verify-financial-status")]
    [Authorize(Roles = "Admin,DormManager")]
    public async Task<IActionResult> VerifyFinancialStatus(int studentId, [FromBody] FinancialVerificationStatus status)
    {
        var result = await _studentService.VerifyFinancialStatusAsync(studentId, status);
        if (!result) return NotFound();
        return Ok(new { Message = $"Financial status for student {studentId} has been set to {status}." });
    }

    [HttpDelete("all")]
    [Authorize(Roles = "Admin,DormManager")]
    public async Task<ActionResult<int>> ClearAllStudents()
    {
        var count = await _studentService.ClearAllStudentsAsync();
        return Ok(new { Message = $"Successfully deleted {count} students and their user accounts.", Count = count });
    }

    [HttpPost("bulk-verify-profiles")]
    [Authorize(Roles = "Admin,DormManager")]
    public async Task<ActionResult<int>> BulkVerifyProfiles([FromBody] BulkVerifyProfilesDto dto)
    {
        var count = await _studentService.BulkVerifyProfilesAsync(dto);
        return Ok(new { Message = $"Successfully updated {count} student profiles to {dto.Status}.", Count = count });
    }

    [HttpGet("by-freshman-status")]
    public async Task<ActionResult<List<StudentSummaryDto>>> GetByFreshmanStatus([FromQuery] bool? isFreshman)
    {
        var students = await _studentService.GetByFreshmanStatusAsync(isFreshman);
        return Ok(students);
    }

    private int? GetCurrentUserId()
    {
        var idValue = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue("sub")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(idValue, out var userId) ? userId : null;
    }
}
