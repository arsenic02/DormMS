namespace DormMS.Server.Models;

public enum UserRole
{
    Admin,
    DormManager,
    Student
}

public class User
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;   // za login
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;

    // veze na profile (opciono)
    public int? StudentId { get; set; }
    public Student? Student { get; set; }

    public int? AdminId { get; set; }
    public Admin? Admin { get; set; }

    public int? DormManagerId { get; set; }
    public DormManager? DormManager { get; set; }
}