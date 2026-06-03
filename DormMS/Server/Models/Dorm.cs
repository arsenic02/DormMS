public class Dorm
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Municipality { get; set; } = string.Empty;
    public List<DormitoryRoom> Rooms { get; set; } = new List<DormitoryRoom>();
    public List<DormManager> Managers { get; set; } = new();

    public int Capacity => Rooms.Sum(r => r.Capacity);
    public int CurrentOccupancy => Rooms.Sum(r => r.CurrentOccupancy);
    public int AvailableSpots => Capacity - CurrentOccupancy;
    public bool IsFull => CurrentOccupancy >= Capacity;
}