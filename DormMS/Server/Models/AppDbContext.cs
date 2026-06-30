using Microsoft.EntityFrameworkCore;

namespace DormMS.Server.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Dorm> Dorms { get; set; }
    public DbSet<DormitoryRoom> DormitoryRooms { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<DormManager> DormManagers { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Competition> Competitions { get; set; }
    public DbSet<CompetitionApplication> CompetitionApplications { get; set; }

    public DbSet<StudentFinancialStatus> StudentFinancialStatuses { get; set; }
    public DbSet<RankingResult> RankingResults { get; set; }
    public DbSet<RoomSwapRequest> RoomSwapRequests { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RoomSwapRequest>()
            .HasOne(r => r.Sender)
            .WithMany(s => s.SentSwapRequests)
            .HasForeignKey(r => r.SenderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RoomSwapRequest>()
            .HasOne(r => r.Receiver)
            .WithMany(s => s.ReceivedSwapRequests)
            .HasForeignKey(r => r.ReceiverId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RoomSwapRequest>()
            .HasOne(r => r.SenderRoom)
            .WithMany()
            .HasForeignKey(r => r.SenderRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RoomSwapRequest>()
            .HasOne(r => r.ReceiverRoom)
            .WithMany()
            .HasForeignKey(r => r.ReceiverRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Student)
            .WithMany()
            .HasForeignKey(n => n.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Dorm)
            .WithMany()
            .HasForeignKey(n => n.DormId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}