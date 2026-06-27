using DormMS.Server.Models;

public class CreateRoomSwapRequestDto
{
    public string ReceiverStudentIdNumber { get; set; } = string.Empty;
}

public class RespondRoomSwapRequestDto
{
    public bool Accept { get; set; }
}

public class RoomSwapRequestDto
{
    public int Id { get; set; }

    public int SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderRoomNumber { get; set; } = string.Empty;

    public int ReceiverId { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverRoomNumber { get; set; } = string.Empty;

    public RoomSwapStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
}