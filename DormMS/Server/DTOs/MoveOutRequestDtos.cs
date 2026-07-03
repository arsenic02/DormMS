using DormMS.Server.Models;

public class CreateMoveOutRequestDto
{
}

public class MoveOutRequestDto
{
    public int Id { get; set; }

    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentIdNumber { get; set; } = string.Empty;

    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string DormName { get; set; } = string.Empty;

    public string RoomNumberSnapshot { get; set; } = string.Empty;
    public string DormNameSnapshot { get; set; } = string.Empty;

    public MoveOutStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
}
