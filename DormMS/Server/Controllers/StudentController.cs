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

    [HttpPost]
    public async Task<ActionResult<Student>> CreateStudent(StudentCreateUpdateDto dto)
    {
        var (student, error) = await _studentService.CreateAsync(dto);
        if (error is not null)
            return BadRequest(error);

        return CreatedAtAction(nameof(GetStudent), new { id = student!.Id }, student);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStudent(int id, StudentCreateUpdateDto dto)
    {
        var (found, error) = await _studentService.UpdateAsync(id, dto);
        if (!found)
            return NotFound();
        if (error is not null)
            return BadRequest(error);

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
}
