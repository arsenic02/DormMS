using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    private int? GetCurrentUserId()
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return int.TryParse(idValue, out var userId) ? userId : null;
    }
}
