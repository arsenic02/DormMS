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
    // NEMA navigacionog Dorm objekta!
}
