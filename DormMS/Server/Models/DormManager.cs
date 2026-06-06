namespace DormMS.Server.Models;

public class DormManager
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int DormId { get; set; } //FK

    public Dorm? Dorm { get; set; }
}