namespace DormMS.Server.Repositories.Interfaces;

using DormMS.Server.Models;

public interface IDormManagerRepository
{
    Task<DormManager?> GetByUserIdAsync(int userId);
}