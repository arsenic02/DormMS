namespace DormMS.Server.Repositories.Interfaces;

using DormMS.Server.DTOs;
using DormMS.Server.Models;

public interface IUserRepository
{
    Task<PagedResultDto<UserGetDto>> GetPagedAsync(int pageNumber, int pageSize, UserRole? roleFilter);
}