using DormMS.Server.DTOs;

namespace DormMS.Server.Services.Interfaces;

public interface IAssignmentService
{
    Task<AssignmentResultDto> AssignStudentsToDormsAsync(int competitionId);
}
