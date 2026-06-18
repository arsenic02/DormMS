using DormMS.Server.DTOs;

namespace DormMS.Server.Services.Interfaces;

public interface IAssignmentService
{
    Task<AssignmentResultDto> AssignStudentsToDormsAsync(int competitionId);

    // Rangiranje starijih studenata (vraca rezultat grupisan po fakultetima)
    Task<OlderStudentsRankingResultDto> RankOlderStudentsAsync(int competitionId);

    // Rangiranje brucosa (vraca rezultat grupisan po fakultetima)
    Task<FreshmanRankingResultDto> RankFreshmenAsync(int competitionId);

    // Kompatibilnost sa starim endpoint-om
   // Task<List<StudentAssignmentDto>> RankStudentsAsync(int competitionId);

    Task<AssignmentResultDto> AssignRankedStudentsAsync(List<StudentAssignmentDto> rankedStudents);
    Task<AssignmentResultDto> BulkAssignRankedStudentsAsync(List<StudentAssignmentDto> rankedStudents);
    Task<bool> ResetAssignmentsAsync();
}
