namespace DormMS.Server.Models;

public class LaundryMachine
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int DormId { get; set; }
    public Dorm Dorm { get; set; } = null!;
    public ICollection<LaundryReservation> Reservations { get; set; } = new List<LaundryReservation>();
}

public class LaundryReservation
{
    public int Id { get; set; }
    public int LaundryMachineId { get; set; }
    public LaundryMachine LaundryMachine { get; set; } = null!;
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public LaundryReservationStatus Status { get; set; } = LaundryReservationStatus.Upcoming;
    public DateTime CreatedAt { get; set; } = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
}

public enum LaundryReservationStatus
{
    Upcoming = 0,
    Active = 1,
    Completed = 2,
    Cancelled = 3
}
