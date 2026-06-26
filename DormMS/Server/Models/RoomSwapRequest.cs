using DormMS.Server.Models;

public class RoomSwapRequest
{
    public int Id { get; set; }

    public int SenderId { get; set; }
    public Student Sender { get; set; } = null!;

    public int ReceiverId { get; set; }
    public Student Receiver { get; set; } = null!;

    public RoomSwapStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? RespondedAt { get; set; }
}

public enum RoomSwapStatus
{
    Pending=0,
    Accepted=1,
    Rejected=2,
}