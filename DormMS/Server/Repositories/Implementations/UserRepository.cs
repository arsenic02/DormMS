namespace DormMS.Server.Repositories.Implementations;

using DormMS.Server.DTOs;
using DormMS.Server.Models;
using DormMS.Server.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly PasswordHasher<User> _passwordHasher;

    public UserRepository(AppDbContext context, IAssignmentRepository assignmentRepository)
    {
        _context = context;
        _assignmentRepository = assignmentRepository;
        _passwordHasher = new PasswordHasher<User>();
    }
    public async Task<User?> GetByIdTrackedAsync(int id)
    => await _context.Users.FindAsync(id);

    public async Task DeleteUserAndProfileAsync(User user)
    {
        if (user.Role == UserRole.Student && user.StudentId.HasValue)
        {
            await _assignmentRepository.ReleaseRoomAsync(user.StudentId.Value);

            var student = await _context.Students.FindAsync(user.StudentId.Value);
            if (student is not null)
                _context.Students.Remove(student);
        }
        else if (user.Role == UserRole.DormManager && user.DormManagerId.HasValue)
        {
            var manager = await _context.DormManagers.FindAsync(user.DormManagerId.Value);
            if (manager is not null)
                _context.DormManagers.Remove(manager);
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
    // public async Task DeleteUserAndProfileAsync(User user)
    // {
    //     if (user.Role == UserRole.Student && user.StudentId.HasValue)
    //     {
    //         var student = await _context.Students.FindAsync(user.StudentId.Value);
    //         if (student is not null)
    //             _context.Students.Remove(student);
    //     }
    //     else if (user.Role == UserRole.DormManager && user.DormManagerId.HasValue)
    //     {
    //         var manager = await _context.DormManagers.FindAsync(user.DormManagerId.Value);
    //         if (manager is not null)
    //             _context.DormManagers.Remove(manager);
    //     }

    //     _context.Users.Remove(user);
    //     await _context.SaveChangesAsync();
    // }
    public async Task<PagedResultDto<UserGetDto>> GetPagedAsync(int pageNumber, int pageSize, UserRole? roleFilter)
    {
        var query = _context.Users.AsNoTracking();

        if (roleFilter.HasValue)
            query = query.Where(u => u.Role == roleFilter.Value);

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Pribavi profile po tipu, posebno za svaku rolu zastupljenu u trenutnoj stranici
        var studentIds = users.Where(u => u.StudentId.HasValue).Select(u => u.StudentId!.Value).ToList();
        var dormManagerIds = users.Where(u => u.DormManagerId.HasValue).Select(u => u.DormManagerId!.Value).ToList();
        var adminIds = users.Where(u => u.AdminId.HasValue).Select(u => u.AdminId!.Value).ToList();

        var students = await _context.Students
            .AsNoTracking()
            .Where(s => studentIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => $"{s.Name} {s.Surname}");

        var dormManagers = await _context.DormManagers
            .AsNoTracking()
            .Where(dm => dormManagerIds.Contains(dm.Id))
            .ToDictionaryAsync(dm => dm.Id, dm => $"{dm.Name} {dm.Surname}");

        var admins = await _context.Admins
            .AsNoTracking()
            .Where(a => adminIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => $"{a.Name} {a.Surname}");

        var items = users.Select(u => new UserGetDto
        {
            Id = u.Id,
            Email = u.Email,
            Role = u.Role,
            IsActive = u.IsActive,
            ProfileName = u.StudentId.HasValue && students.TryGetValue(u.StudentId.Value, out var studentName)
                ? studentName
                : u.DormManagerId.HasValue && dormManagers.TryGetValue(u.DormManagerId.Value, out var managerName)
                    ? managerName
                    : u.AdminId.HasValue && admins.TryGetValue(u.AdminId.Value, out var adminName)
                        ? adminName
                        : null
        }).ToList();

        return new PagedResultDto<UserGetDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<User> CreateDormManagerAsync(DormManagerCreateDto dto)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        
        // Get first available dorm
        var dorm = await _context.Dorms.FirstOrDefaultAsync();
        if (dorm is null)
            throw new InvalidOperationException("Nijedan dom ne postoji u sistemu.");
        
        var dormManager = new DormManager
        {
            Name = dto.Name.Trim(),
            Surname = dto.Surname.Trim(),
            Email = normalizedEmail,
            PhoneNumber = dto.PhoneNumber.Trim(),
            DormId = dorm.Id
        };

        _context.DormManagers.Add(dormManager);
        await _context.SaveChangesAsync();

        var user = new User
        {
            Email = normalizedEmail,
            Role = UserRole.DormManager,
            IsActive = true,
            DormManagerId = dormManager.Id
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }
}