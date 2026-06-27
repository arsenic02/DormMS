namespace DormMS.Server.DTOs;

using DormMS.Server.Models;

public class UserGetDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public string? ProfileName { get; set; }
}