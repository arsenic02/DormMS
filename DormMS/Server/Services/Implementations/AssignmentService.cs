using Microsoft.EntityFrameworkCore;
using DormMS.Server.Services.Interfaces;
using DormMS.Server.Repositories.Interfaces;
using DormMS.Server.DTOs;
using DormMS.Server.Models;

namespace DormMS.Server.Services.Implementations;

public class AssignmentService : IAssignmentService
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly AppDbContext _context;

    public AssignmentService(IAssignmentRepository assignmentRepository, AppDbContext context)
    {
        _assignmentRepository = assignmentRepository;
        _context = context;
    }

    public async Task<AssignmentResultDto> AssignStudentsToDormsAsync(int competitionId)
    {
        var competition = await _assignmentRepository.GetCompetitionWithApplicationsAsync(competitionId);
        var rankingResult = await RankOlderStudentsAsync(competitionId);
        var rankedStudents = rankingResult.RankingsByFaculty.Values.SelectMany(x => x).ToList();
        var result = await AssignRankedStudentsAsync(rankedStudents);

        if (competition != null)
        {
            result.CompetitionId = competition.Id;
            result.CompetitionName = competition.Name;
        }

        return result;
    }

    /// <summary>
    /// Rangira starije studente (ne brucose) po fakultetima.
    /// Formula: (averageGrade * 5) + (espb / yearsOfStudy * 0.8) + espbBonus + financialBonus
    /// </summary>
    public async Task<OlderStudentsRankingResultDto> RankOlderStudentsAsync(int competitionId)
    {
        var result = new OlderStudentsRankingResultDto();
        var competition = await _assignmentRepository.GetCompetitionWithApplicationsAsync(competitionId);

        if (competition == null)
        {
            result.Errors.Add("Competition not found");
            return result;
        }

        result.CompetitionId = competition.Id;
        result.CompetitionName = competition.Name;

        // Samo stariji studenti (ne brucosi), budzetski
        var approvedApplications = competition.Applications
            .Where(a => /*(*/ a.Status == ApplicationStatus.Approved //|| a.Status == ApplicationStatus.Pending)
                        && a.Student.IsBudgetStudent
                        && !a.Student.IsFreshman) // iskljuci brucose
            .ToList();

        var studentAssignments = new List<StudentAssignmentDto>();

        foreach (var application in approvedApplications)
        {
            var student = application.Student;
            var yearsOfStudy = student.CurrentYearOfStudy;

            if (yearsOfStudy <= 0)
            {
                continue; // preskacemo greske za sada, mozemo dodati error handling
            }

            var calculatedPoints = CalculateOlderStudentPoints(
                student.AverageGrade ?? 0m,
                student.ESPB,
                yearsOfStudy,
                student.FinancialStatus?.IncomePerMember ?? 0);

            var assignment = new StudentAssignmentDto
            {
                StudentId = student.Id,
                StudentName = student.Name,
                StudentSurname = student.Surname,
                StudentIDNumber = student.StudentIDNumber,
                AverageGrade = student.AverageGrade ?? 0m,
                ESPB = student.ESPB,
                YearsOfStudy = yearsOfStudy,
                CalculatedPoints = calculatedPoints,
                TotalFamilyIncome = student.FinancialStatus?.TotalFamilyIncome ?? 0,
                FamilyMemberCount = student.FinancialStatus?.FamilyMemberCount ?? 0,
                IncomePerMember = student.FinancialStatus?.IncomePerMember ?? 0,
                FacultyName = student.FacultyName ?? "Unknown",
                Status = AssignmentStatus.NotAssigned
            };

            studentAssignments.Add(assignment);
        }

        // Grupisemo po fakultetima pa sortiramo svaki fakultet posebno
        var groupedByFaculty = studentAssignments
            .GroupBy(s => s.FacultyName ?? "Unknown")
            .ToDictionary(
                g => g.Key,
                g => g
                    .OrderByDescending(s => s.CalculatedPoints)
                    .ThenByDescending(s => s.ESPB)
                    .ThenBy(s => s.YearsOfStudy)
                    .ThenByDescending(s => s.AverageGrade)
                    .ThenBy(s => s.IncomePerMember)
                    .ToList()
            );

        if (competition.MaxStudents.HasValue)
        {
            var quotas = CalculateFacultyQuotas(
                groupedByFaculty.ToDictionary(k => k.Key, v => v.Value.Count),
                competition.MaxStudents.Value);

            foreach (var faculty in groupedByFaculty)
            {
                var quota = quotas[faculty.Key];
                for (int i = 0; i < faculty.Value.Count; i++)
                    faculty.Value[i].Status = i < quota
                        ? AssignmentStatus.Assigned
                        : AssignmentStatus.Waitlisted;
            }
        }

        result.RankingsByFaculty = groupedByFaculty;
        result.TotalStudents = studentAssignments.Count;

        return result;
    }

    /// <summary>
    /// Rangira brucose po fakultetima.
    /// Formula: bb = po * 8 (po = prosecna ocena iz srednje skole, bb = bodovi brucosa)
    /// + socijalni bonus (1 bod ako je prihod po clanu < 50% prosecne plate)
    /// </summary>
    public async Task<FreshmanRankingResultDto> RankFreshmenAsync(int competitionId)
    {
        var result = new FreshmanRankingResultDto();
        var competition = await _assignmentRepository.GetCompetitionWithApplicationsAsync(competitionId);

        if (competition == null)
        {
            result.Errors.Add("Competition not found");
            return result;
        }

        result.CompetitionId = competition.Id;
        result.CompetitionName = competition.Name;

        // Samo brucosi, budzetski studenti
        var approvedApplications = competition.Applications
            .Where(a => /*(*/ a.Status == ApplicationStatus.Approved //|| a.Status == ApplicationStatus.Pending)
                        && a.Student.IsBudgetStudent
                        && a.Student.IsFreshman) // samo brucosi
            .ToList();

        var freshmanAssignments = new List<FreshmanAssignmentDto>();

        foreach (var application in approvedApplications)
        {
            var student = application.Student;
            var incomePerMember = student.FinancialStatus?.IncomePerMember ?? 0;

            // bb = po * 8
            var highSchoolAverage = student.HighSchoolAverageGrade ?? 0;
            var highSchoolPoints = highSchoolAverage * 8;

            // Socijalni bonus: 1 bod ako je prihod po clanu < 50% prosecne plate
            const decimal averageSalary = 121650m;
            var socialBonus = incomePerMember > 0 && incomePerMember < (averageSalary * 0.5m) ? 1m : 0m;

            var totalPoints = highSchoolPoints + socialBonus;

            var assignment = new FreshmanAssignmentDto
            {
                StudentId = student.Id,
                StudentName = student.Name,
                StudentSurname = student.Surname,
                StudentIDNumber = student.StudentIDNumber,
                HighSchoolAverageGrade = highSchoolAverage,
                FacultyName = student.FacultyName,
                HighSchoolPoints = Math.Round(highSchoolPoints, 2),
                SocialBonus = socialBonus,
                CalculatedPoints = Math.Round(totalPoints, 2),
                TotalFamilyIncome = student.FinancialStatus?.TotalFamilyIncome ?? 0,
                FamilyMemberCount = student.FinancialStatus?.FamilyMemberCount ?? 0,
                IncomePerMember = incomePerMember,
                Status = AssignmentStatus.NotAssigned
            };

            freshmanAssignments.Add(assignment);
        }

        // Grupisemo po fakultetima pa sortiramo svaki fakultet posebno
        var groupedByFaculty = freshmanAssignments
            .GroupBy(s => s.FacultyName ?? "Unknown")
            .ToDictionary(
                g => g.Key,
                g => g
                    .OrderByDescending(s => s.CalculatedPoints)
                    .ThenByDescending(s => s.HighSchoolPoints)
                    .ThenBy(s => s.IncomePerMember)
                    .ToList()
            );

        if (competition.MaxStudents.HasValue)
        {
            var quotas = CalculateFacultyQuotas(
                groupedByFaculty.ToDictionary(k => k.Key, v => v.Value.Count),
                competition.MaxStudents.Value);

            foreach (var faculty in groupedByFaculty)
            {
                var quota = quotas[faculty.Key];
                for (int i = 0; i < faculty.Value.Count; i++)
                    faculty.Value[i].Status = i < quota
                        ? AssignmentStatus.Assigned
                        : AssignmentStatus.Waitlisted;
            }
        }

        result.RankingsByFaculty = groupedByFaculty;
        result.TotalStudents = freshmanAssignments.Count;

        return result;
    }

    private static Dictionary<string, int> CalculateFacultyQuotas(
    Dictionary<string, int> facultyApplicationCounts,
    int maxStudents)
    {
        var total = facultyApplicationCounts.Values.Sum();
        var quotas = new Dictionary<string, int>();
        var assigned = 0;

        foreach (var faculty in facultyApplicationCounts)
        {
            var proportion = (double)faculty.Value / total;
            var quota = (int)Math.Floor(proportion * maxStudents);
            quotas[faculty.Key] = quota;
            assigned += quota;
        }

        // Ostatak zbog Floor zaokruzivanja - dodeli fakultetima sa najvise prijava
        var remaining = maxStudents - assigned;
        var sortedBySize = facultyApplicationCounts
            .OrderByDescending(f => f.Value)
            .Select(f => f.Key)
            .ToList();

        for (int i = 0; i < remaining; i++)
            quotas[sortedBySize[i % sortedBySize.Count]]++;

        return quotas;
    }

    // public async Task<List<StudentAssignmentDto>> RankStudentsAsync(int competitionId)
    // {
    //     // Poziva novu metodu za starije studente radi kompatibilnosti
    //     var result = await RankOlderStudentsAsync(competitionId);
    //     return result.RankingsByFaculty.Values.SelectMany(x => x).ToList();
    // }

    public async Task<AssignmentResultDto> AssignRankedStudentsAsync(List<StudentAssignmentDto> rankedStudents)
    {
        var result = new AssignmentResultDto();

        var dorms = await _assignmentRepository.GetAllDormsWithRoomsAsync();
        var totalAvailableSpots = dorms.Sum(d => d.AvailableSpots);

        if (totalAvailableSpots == 0)
        {
            result.Errors.Add("No available spots in any dormitory");
            return result;
        }

        result.TotalStudents = rankedStudents.Count;

        foreach (var assignment in rankedStudents)
        {
            if (assignment.Status == AssignmentStatus.Error)
            {
                result.Failed++;
                result.Assignments.Add(assignment);
                continue;
            }

            var assigned = await TryAssignStudentAsync(assignment, dorms);

            if (assigned)
            {
                result.SuccessfullyAssigned++;
                assignment.Status = AssignmentStatus.Assigned;
            }
            else
            {
                result.Waitlisted++;
                assignment.Status = AssignmentStatus.Waitlisted;
            }

            result.Assignments.Add(assignment);
        }

        return result;
    }

    public async Task<AssignmentResultDto> BulkAssignRankedStudentsAsync(List<StudentAssignmentDto> rankedStudents)
    {
        var result = new AssignmentResultDto();
        var dorms = await _assignmentRepository.GetAllDormsWithRoomsAsync();

        var studentIds = rankedStudents.Where(s => s.Status != AssignmentStatus.Error).Select(s => s.StudentId).ToList();
        var students = await _assignmentRepository.GetStudentsByIdsAsync(studentIds);
        var studentsDict = students.ToDictionary(s => s.Id);

        result.TotalStudents = rankedStudents.Count;

        foreach (var assignment in rankedStudents)
        {
            if (assignment.Status == AssignmentStatus.Error)
            {
                result.Failed++;
                result.Assignments.Add(assignment);
                continue;
            }

            if (!studentsDict.TryGetValue(assignment.StudentId, out var student))
            {
                assignment.Status = AssignmentStatus.Error;
                result.Errors.Add($"Student with ID {assignment.StudentId} not found in database.");
                result.Failed++;
                result.Assignments.Add(assignment);
                continue;
            }

            var assigned = TryAssignStudentInMemory(assignment, student, dorms);

            if (assigned)
            {
                result.SuccessfullyAssigned++;
                assignment.Status = AssignmentStatus.Assigned;
            }
            else
            {
                result.Waitlisted++;
                assignment.Status = AssignmentStatus.Waitlisted;
            }

            result.Assignments.Add(assignment);
        }

        await _assignmentRepository.SaveChangesAsync();
        return result;
    }

    private bool TryAssignStudentInMemory(StudentAssignmentDto assignment, Student student, List<Dorm> dorms)
    {
        foreach (var dorm in dorms.Where(d => !d.IsFull))
        {
            var room = dorm.Rooms.FirstOrDefault(r =>
                r.CurrentOccupancy < r.Capacity &&
                (int)r.Gender == (int)student.Gender);

            if (room != null)
            {
                student.DormitoryRoomId = room.Id;
                room.CurrentOccupancy++;

                assignment.AssignedDormId = dorm.Id;
                assignment.AssignedDormName = dorm.Name;
                assignment.AssignedRoomId = room.Id;
                return true;
            }
        }
        return false;
    }

    public async Task<bool> ResetAssignmentsAsync()
    {
        var studentsWithRooms = await _context.Students
            .Where(s => s.DormitoryRoomId != null)
            .ToListAsync();

        foreach (var student in studentsWithRooms)
        {
            student.DormitoryRoomId = null;
        }

        var rooms = await _context.DormitoryRooms.ToListAsync();
        foreach (var room in rooms)
        {
            room.CurrentOccupancy = 0;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    private decimal CalculateOlderStudentPoints(decimal averageGrade, int espb, int yearsOfStudy, decimal incomePerMember)
    {
        if (yearsOfStudy <= 0)
            return 0;

        var academicPoints = (averageGrade * 5) + ((decimal)espb / yearsOfStudy * 0.8m);

        var espbBonus = 0m;
        if (espb >= 120 && espb <= 240)
            espbBonus = 1m;
        else if (espb > 240)
            espbBonus = 2m;

        const decimal averageSalary = 121650m;
        var financialBonus = 0m;
        if (incomePerMember > 0 && incomePerMember < (averageSalary * 0.5m))
        {
            financialBonus = 1m;
        }

        var totalPoints = academicPoints + espbBonus + financialBonus;
        return Math.Round(totalPoints, 2);
    }

    private async Task<bool> TryAssignStudentAsync(StudentAssignmentDto assignment, List<Dorm> dorms)
    {
        foreach (var dorm in dorms.Where(d => !d.IsFull))
        {
            var room = await _assignmentRepository.GetAvailableRoomAsync(dorm.Id);
            if (room != null)
            {
                await _assignmentRepository.AssignStudentToRoomAsync(assignment.StudentId, room.Id);

                // Increment room occupancy
                room.CurrentOccupancy++;
                await _context.SaveChangesAsync();

                assignment.AssignedDormId = dorm.Id;
                assignment.AssignedDormName = dorm.Name;
                assignment.AssignedRoomId = room.Id;
                return true;
            }
        }

        return false;
    }
}
