using DormMS.Server.Models;

namespace DormMS.Server.Repositories.Interfaces;

public interface IAssignmentRepository
{
    Task<Competition?> GetCompetitionWithApplicationsAsync(int competitionId);
    Task<Dorm?> GetDormWithRoomsAsync(int dormId);
    Task<List<Dorm>> GetAllDormsWithRoomsAsync();
    Task AssignStudentToRoomAsync(int studentId, int roomId);
    Task UpdateApplicationStatusAsync(int applicationId, ApplicationStatus status);
    Task<int?> GetStudentCurrentRoomIdAsync(int studentId);
    Task<List<Student>> GetStudentsByIdsAsync(List<int> studentIds);
    Task<DormitoryRoom?> GetAvailableRoomAsync(int dormId, Gender studentGender);
    Task<DormitoryRoom?> GetAnyAvailableRoomAsync(int dormId);
    Task SaveChangesAsync();
}
