namespace DormMS.Server.Models;

using System.ComponentModel.DataAnnotations.Schema;

public class RoomSwapRequest
{
    public int Id { get; set; }

    public int SenderId { get; set; }
    public Student Sender { get; set; } = null!;

    public int ReceiverId { get; set; }
    public Student Receiver { get; set; } = null!;

    public int SenderRoomId { get; set; }
    public DormitoryRoom SenderRoom { get; set; } = null!;

    public int ReceiverRoomId { get; set; }
    public DormitoryRoom ReceiverRoom { get; set; } = null!;

    public RoomSwapStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
}

public enum RoomSwapStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2,
}

// using DormMS.Server.Models;

// public class RoomSwapRequest
// {
//     public int Id { get; set; }

//     public int SenderId { get; set; }
//     public Student Sender { get; set; } = null!;

//     public int ReceiverId { get; set; }
//     public Student Receiver { get; set; } = null!;

//     public int SenderRoomId { get; set; }
//     public DormitoryRoom SenderRoom { get; set; } = null!;

//     public int ReceiverRoomId { get; set; }
//     public DormitoryRoom ReceiverRoom { get; set; } = null!;

//     public RoomSwapStatus Status { get; set; }

//     public DateTime CreatedAt { get; set; }

//     public DateTime? RespondedAt { get; set; }
// }

// public enum RoomSwapStatus
// {
//     Pending = 0,
//     Accepted = 1,
//     Rejected = 2,
// }