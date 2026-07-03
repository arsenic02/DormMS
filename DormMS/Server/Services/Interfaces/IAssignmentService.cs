using DormMS.Server.DTOs;
using DormMS.Server.Models;

namespace DormMS.Server.Services.Interfaces;

public interface IAssignmentService
{
    //Task<AssignmentResultDto> AssignStudentsToDormsAsync(int competitionId);

    Task<AssignmentResultDto> AssignOlderStudentsToDormsAsync(int competitionId);
    Task<AssignmentResultDto> AssignFreshmenToDormsAsync(int competitionId);


    // Rangiranje starijih studenata (vraca rezultat grupisan po fakultetima)
    Task<OlderStudentsRankingResultDto> RankOlderStudentsAsync(int competitionId);

    // Rangiranje brucosa (vraca rezultat grupisan po fakultetima)
    Task<FreshmanRankingResultDto> RankFreshmenAsync(int competitionId);

    Task<object> GetSavedRankingResultsAsync(int competitionId, bool isFreshmanView);

    // Kompatibilnost sa starim endpoint-om
    // Task<List<StudentAssignmentDto>> RankStudentsAsync(int competitionId);

    //Task<AssignmentResultDto> AssignRankedStudentsAsync(List<StudentAssignmentDto> rankedStudents);
    // Task<AssignmentResultDto> BulkAssignRankedStudentsAsync(List<StudentAssignmentDto> rankedStudents);
    Task<bool> ResetFlowAsync();
    //Task<bool> TryAssignStudentAsync(StudentAssignmentDto assignment, Student student,List<Dorm> dorms); //ne treba da se poziva kao endpoint
    Task<bool> ResetAssignmentAsync(int competitionId);
    Task<StudentAssignmentDto> AssignSingleStudentAsync(int rankingResultId);
    Task RemoveStudentFromRoomAsync(int studentId);
}
