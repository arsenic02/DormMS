namespace DormMS.Server.Repositories.Interfaces;

using DormMS.Server.DTOs;
using DormMS.Server.Models;

public interface IUserRepository
{
    Task<PagedResultDto<UserGetDto>> GetPagedAsync(int pageNumber, int pageSize, UserRole? roleFilter);
    Task<User?> GetByIdTrackedAsync(int id);
    Task DeleteUserAndProfileAsync(User user);
    Task<User> CreateDormManagerAsync(DormManagerCreateDto dto);
    Task<AdminProfileDto?> GetAdminProfileAsync(int userId);
    Task<DormManagerProfileDto?> GetDormManagerProfileAsync(int userId);
}