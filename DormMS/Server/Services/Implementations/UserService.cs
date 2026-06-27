namespace DormMS.Server.Services;

using DormMS.Server.DTOs;
using DormMS.Server.Models;
using DormMS.Server.Repositories.Interfaces;
using DormMS.Server.Services.Interfaces;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public Task<PagedResultDto<UserGetDto>> GetPagedAsync(int pageNumber, int pageSize, UserRole? roleFilter)
        => _userRepository.GetPagedAsync(pageNumber, pageSize, roleFilter);
}