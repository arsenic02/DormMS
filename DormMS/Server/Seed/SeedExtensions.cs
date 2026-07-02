using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DormMS.Server.Models;

public static class SeedExtensions
{
    public static async Task SeedDefaultUsersAsync(this IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await SeedAdminAsync(context, configuration);
    }

    private static async Task SeedAdminAsync(AppDbContext context, IConfiguration configuration)
    {
        var email = configuration["AdminSeed:Email"]?.Trim().ToLowerInvariant();
        var password = configuration["AdminSeed:Password"];
        var name = configuration["AdminSeed:Name"] ?? "System";
        var surname = configuration["AdminSeed:Surname"] ?? "Admin";
        var phone = configuration["AdminSeed:PhoneNumber"] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
        {
            var admin = new Admin
            {
                Name = name,
                Surname = surname,
                Email = email,
                PhoneNumber = phone
            };
            context.Admins.Add(admin);
            await context.SaveChangesAsync();

            user = new User
            {
                Email = email,
                Role = UserRole.Admin,
                IsActive = true,
                AdminId = admin.Id
            };
            user.PasswordHash = HashPassword(user, password);
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return;
        }

        if (user.Role != UserRole.Admin)
            return;

        if (user.AdminId.HasValue)
            return;

        var existingAdmin = await context.Admins.FirstOrDefaultAsync(a => a.Email == email);
        if (existingAdmin is null)
        {
            existingAdmin = new Admin
            {
                Name = name,
                Surname = surname,
                Email = email,
                PhoneNumber = phone
            };
            context.Admins.Add(existingAdmin);
            await context.SaveChangesAsync();
        }

        user.AdminId = existingAdmin.Id;
        await context.SaveChangesAsync();
    }

    private static string HashPassword(User user, string password)
    {
        var hasher = new PasswordHasher<User>();
        return hasher.HashPassword(user, password);
    }
}
