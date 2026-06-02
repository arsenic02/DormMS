public class DormCreateUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}

public class DormGetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int CurrentOccupancy { get; set; }
    public int AvailableSpots { get; set; }
    public bool IsFull { get; set; }
    public List<DormitoryRoomGetDto> Rooms { get; set; } = new();
}
