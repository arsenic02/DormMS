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
    public async Task<UserDeleteResult> DeleteAsync(int id)
    {
        var user = await _userRepository.GetByIdTrackedAsync(id);
        if (user is null)
            return UserDeleteResult.NotFound;

        if (user.Role == UserRole.Admin)
            return UserDeleteResult.CannotDeleteAdmin;

        await _userRepository.DeleteUserAndProfileAsync(user);
        return UserDeleteResult.Success;
    }

    public Task<PagedResultDto<UserGetDto>> GetPagedAsync(int pageNumber, int pageSize, UserRole? roleFilter)
        => _userRepository.GetPagedAsync(pageNumber, pageSize, roleFilter);
}