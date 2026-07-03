namespace DormMS.Server.Models;

using System.ComponentModel.DataAnnotations.Schema;

public class MoveOutRequest
{
    public int Id { get; set; }

    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public int RoomId { get; set; }
    public DormitoryRoom Room { get; set; } = null!;

    public string RoomNumberSnapshot { get; set; } = string.Empty;
    public string DormNameSnapshot { get; set; } = string.Empty;

    public MoveOutStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
}

public enum MoveOutStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
