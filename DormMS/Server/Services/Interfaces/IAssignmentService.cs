using DormMS.Server.DTOs;

namespace DormMS.Server.Services.Interfaces;

public interface IAssignmentService
{
    Task<AssignmentResultDto> AssignStudentsToDormsAsync(int competitionId);
    Task<List<StudentAssignmentDto>> RankStudentsAsync(int competitionId);
    Task<AssignmentResultDto> AssignRankedStudentsAsync(List<StudentAssignmentDto> rankedStudents);
    Task<AssignmentResultDto> BulkAssignRankedStudentsAsync(List<StudentAssignmentDto> rankedStudents);
    Task<bool> ResetAssignmentsAsync();
}
