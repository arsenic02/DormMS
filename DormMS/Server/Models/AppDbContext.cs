using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Dorm> Dorms { get; set; }
    public DbSet<DormitoryRoom> DormitoryRooms { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<DormManager> DormManagers { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<User> Users { get; set; }
}