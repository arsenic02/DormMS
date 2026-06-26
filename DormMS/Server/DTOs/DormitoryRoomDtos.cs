namespace DormMS.Server.DTOs;

using DormMS.Server.Models;

public class DormitoryRoomCreateUpdateDto
{
    public string RoomNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
   // public int CurrentOccupancy { get; set; } //ne treba, inicijalno je 0
    public GenderType Gender { get; set; }
    public RoomStatus Status { get; set; }
    public int DormId { get; set; }
}

public class DormitoryRoomGetDto
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int CurrentOccupancy { get; set; }
    public int AvailableSpots { get; set; }
    //public bool IsOccupied { get; set; } mozda da ostane za frontend
    public GenderType Gender { get; set; }
    public RoomStatus Status { get; set; }
    public int DormId { get; set; }
    public string? DormName { get; set; }
    public List<StudentSummaryDto> Students { get; set; } = new();
    // NEMA navigacionog Dorm objekta!
}

public class MyRoomGetDto
{
    public bool HasRoom { get; set; }
    public int? RoomId { get; set; }
    public string? RoomNumber { get; set; }
    public string? DormName { get; set; }
    public int? Capacity { get; set; }
    public int? CurrentOccupancy { get; set; }
    public List<RoommateSummaryDto> Roommates { get; set; } = new();
}

public class RoommateSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string StudentIDNumber { get; set; } = string.Empty;
    public string? FacultyName { get; set; }
    public bool IsFreshman { get; set; }
}