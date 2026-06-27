namespace DormMS.Server.Services.Interfaces;

using DormMS.Server.DTOs;
using DormMS.Server.Models;

public interface IUserService
{
    Task<PagedResultDto<UserGetDto>> GetPagedAsync(int pageNumber, int pageSize, UserRole? roleFilter);
}