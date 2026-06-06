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
    public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
    {
        return Ok(await _studentService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Student>> GetStudent(int id)
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStudent(int id)
    {
        var deleted = await _studentService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

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

    [HttpPost("import-financial-status")]
    [Authorize(Roles = "Admin,DormManager")]
    public async Task<ActionResult<BulkFinancialStatusImportResultDto>> ImportFinancialStatus(IFormFile csvFile)
    {
        if (csvFile == null || csvFile.Length == 0)
            return BadRequest("No file uploaded");

        if (!csvFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only CSV files are allowed");

        var result = await _studentService.BulkImportFinancialStatusAsync(csvFile);
        return Ok(result);
    }
    private int? GetCurrentUserId()
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return int.TryParse(idValue, out var userId) ? userId : null;
    }
}
