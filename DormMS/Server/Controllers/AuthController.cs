using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto request)
    {
        var response = await _authService.RegisterAsync(request);
        if (!response.IsAuthenticated)
            return BadRequest(response);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request)
    {
        var response = await _authService.LoginAsync(request);
        if (!response.IsAuthenticated)
            return Unauthorized(response);

        return Ok(response);
    }
}
