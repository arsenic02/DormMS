namespace DormMS.Server.Models;

public class DormitoryRoom
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int CurrentOccupancy { get; set; }
    public GenderType Gender { get; set; }
    public RoomStatus Status { get; set; }

    public int DormId { get; set; } // FK
    public Dorm? Dorm { get; set; }

    public int AvailableSpots => Capacity - CurrentOccupancy;
    public bool IsFull => CurrentOccupancy >= Capacity;
    public bool IsOccupied => CurrentOccupancy > 0;

}

public enum GenderType
{
    Male,
    Female
}

public enum RoomStatus
{
    Available,
    FullyOccupied,
    Maintenance,
    Closed
}