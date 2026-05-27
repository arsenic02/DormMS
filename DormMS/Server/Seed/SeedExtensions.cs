using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public static class SeedExtensions
{
    public static async Task SeedDefaultAdminAsync(this IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var adminEmail = configuration["AdminSeed:Email"]?.Trim().ToLowerInvariant();
        var adminPassword = configuration["AdminSeed:Password"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            return;

        var exists = await context.Users.AnyAsync(u => u.Email == adminEmail);
        if (exists)
            return;

        var user = new User
        {
            Email = adminEmail,
            Role = UserRole.Admin,
            IsActive = true
        };

        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, adminPassword);

        context.Users.Add(user);
        await context.SaveChangesAsync();
    }
}
