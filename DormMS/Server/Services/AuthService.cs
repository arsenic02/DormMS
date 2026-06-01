using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly JwtSettings _jwtSettings;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthService(AppDbContext context, IOptions<JwtSettings> jwtOptions)
    {
        _context = context;
        _jwtSettings = jwtOptions.Value;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new AuthResponseDto
            {
                IsAuthenticated = false,
                Message = "Email and password are required."
            };
        }

        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Surname) ||
            string.IsNullOrWhiteSpace(request.StudentIDNumber))
        {
            return new AuthResponseDto
            {
                IsAuthenticated = false,
                Message = "Name, surname and student ID number are required."
            };
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var emailExists = await _context.Users.AnyAsync(u => u.Email == normalizedEmail);
        if (emailExists)
        {
            return new AuthResponseDto
            {
                IsAuthenticated = false,
                Message = "Email is already registered."
            };
        }

        var indexNumber = request.StudentIDNumber.Trim();
        var indexExists = await _context.Students.AnyAsync(s => s.StudentIDNumber == indexNumber);
        if (indexExists)
        {
            return new AuthResponseDto
            {
                IsAuthenticated = false,
                Message = "Student ID number is already registered."
            };
        }

        var student = new Student
        {
            Name = request.Name.Trim(),
            Surname = request.Surname.Trim(),
            StudentIDNumber = indexNumber,
            Email = normalizedEmail,
            Status = StudentStatus.Active,
            IsProfileComplete = false
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        var user = new User
        {
            Email = normalizedEmail,
            Role = UserRole.Student,
            IsActive = true,
            StudentId = student.Id
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return BuildAuthResponse(user, "Registration successful. Complete your profile when ready.");
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new AuthResponseDto
            {
                IsAuthenticated = false,
                Message = "Email and password are required."
            };
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);

        if (user is null)
        {
            return new AuthResponseDto
            {
                IsAuthenticated = false,
                Message = "Invalid credentials."
            };
        }

        if (!user.IsActive)
        {
            return new AuthResponseDto
            {
                IsAuthenticated = false,
                Message = "User is inactive."
            };
        }

        var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verifyResult == PasswordVerificationResult.Failed)
        {
            return new AuthResponseDto
            {
                IsAuthenticated = false,
                Message = "Invalid credentials."
            };
        }

        return BuildAuthResponse(user, "Login successful.");
    }

    private AuthResponseDto BuildAuthResponse(User user, string message)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var keyBytes = Encoding.UTF8.GetBytes(_jwtSettings.Key);
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(keyBytes),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: signingCredentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthResponseDto
        {
            IsAuthenticated = true,
            Message = message,
            Email = user.Email,
            Role = user.Role.ToString(),
            Token = tokenString,
            ExpiresAtUtc = expiresAt
        };
    }
}
