public class Dorm
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int CurrentOccupancy { get; set; }
    public List<DormitoryRoom> Rooms { get; set; } = new List<DormitoryRoom>();

    public int AvailableSpots => Capacity - CurrentOccupancy;
    public bool IsFull => CurrentOccupancy >= Capacity;
}