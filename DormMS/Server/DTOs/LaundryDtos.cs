using DormMS.Server.Models;

namespace DormMS.Server.DTOs;

public class LaundryMachineCreateUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DormId { get; set; }
}

public class LaundryMachineGetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int DormId { get; set; }
    public string DormName { get; set; } = string.Empty;
}

public class LaundryReservationCreateDto
{
    public int LaundryMachineId { get; set; }
    public DateTime StartTime { get; set; }
}

public class LaundryReservationGetDto
{
    public int Id { get; set; }
    public int LaundryMachineId { get; set; }
    public string LaundryMachineName { get; set; } = string.Empty;
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class LaundrySlotDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAvailable { get; set; }
    public LaundryReservationGetDto? Reservation { get; set; }
}
