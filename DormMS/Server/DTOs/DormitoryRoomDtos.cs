public class DormitoryRoomCreateUpdateDto
{
    public string RoomNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int CurrentOccupancy { get; set; }
    public bool IsOccupied { get; set; }
    public GenderType Gender { get; set; }
    public RoomStatus Status { get; set; }
    public int DormId { get; set; }
}
