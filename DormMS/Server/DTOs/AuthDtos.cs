namespace DormMS.Server.DTOs;

public class RegisterRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string StudentIDNumber { get; set; } = string.Empty;
}

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public bool IsAuthenticated { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Role { get; set; }
    public string? Token { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
}
