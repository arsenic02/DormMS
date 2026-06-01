using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public static class SeedExtensions
{
    public static async Task SeedDefaultUsersAsync(this IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await SeedAdminAsync(context, configuration);
        await SeedDormManagerAsync(context, configuration);
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

    private static async Task SeedDormManagerAsync(AppDbContext context, IConfiguration configuration)
    {
        var email = configuration["DormManagerSeed:Email"]?.Trim().ToLowerInvariant();
        var password = configuration["DormManagerSeed:Password"];
        var name = configuration["DormManagerSeed:Name"] ?? "Referent";
        var surname = configuration["DormManagerSeed:Surname"] ?? "Dom";
        var phone = configuration["DormManagerSeed:PhoneNumber"] ?? string.Empty;
        var configuredDormId = configuration.GetValue<int?>("DormManagerSeed:DormId");

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        var dormId = await GetOrCreateSeedDormIdAsync(context, configuredDormId);

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
        {
            var manager = new DormManager
            {
                Name = name,
                Surname = surname,
                Email = email,
                PhoneNumber = phone,
                DormId = dormId
            };
            context.DormManagers.Add(manager);
            await context.SaveChangesAsync();

            user = new User
            {
                Email = email,
                Role = UserRole.DormManager,
                IsActive = true,
                DormManagerId = manager.Id
            };
            user.PasswordHash = HashPassword(user, password);
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return;
        }

        if (user.Role != UserRole.DormManager)
            return;

        if (user.DormManagerId.HasValue)
            return;

        var existingManager = await context.DormManagers.FirstOrDefaultAsync(m => m.Email == email);
        if (existingManager is null)
        {
            existingManager = new DormManager
            {
                Name = name,
                Surname = surname,
                Email = email,
                PhoneNumber = phone,
                DormId = dormId
            };
            context.DormManagers.Add(existingManager);
            await context.SaveChangesAsync();
        }

        user.DormManagerId = existingManager.Id;
        await context.SaveChangesAsync();
    }

    private static async Task<int> GetOrCreateSeedDormIdAsync(AppDbContext context, int? configuredDormId)
    {
        if (configuredDormId.HasValue &&
            await context.Dorms.AnyAsync(d => d.Id == configuredDormId.Value))
        {
            return configuredDormId.Value;
        }

        var existingDormId = await context.Dorms
            .OrderBy(d => d.Id)
            .Select(d => d.Id)
            .FirstOrDefaultAsync();

        if (existingDormId != 0)
            return existingDormId;

        var dorm = new Dorm
        {
            Name = "Studentski dom (seed)",
            Location = "Beograd",
            Capacity = 100,
            CurrentOccupancy = 0
        };
        context.Dorms.Add(dorm);
        await context.SaveChangesAsync();
        return dorm.Id;
    }

    private static string HashPassword(User user, string password)
    {
        var hasher = new PasswordHasher<User>();
        return hasher.HashPassword(user, password);
    }
}
