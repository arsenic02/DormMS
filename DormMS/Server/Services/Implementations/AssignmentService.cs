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


    /// Raspodeljuje sobe STARIJIM studentima na osnovu vec sacuvanog rang-rezultata
    /// (RankingResult tabela). Uzima u obzir SAMO studente sa statusom Assigned
    /// (oni unutar kvote) - studenti na cekanju (Waitlisted) ne ulaze u raspodelu.
    /// Ne dozvoljava ponovnu raspodelu ako je vec izvrsena za ovaj konkurs.
    public async Task<AssignmentResultDto> AssignOlderStudentsToDormsAsync(int competitionId)
    {
        return await AssignFromRankingResultsAsync(competitionId, isFreshmanView: false);
    }

    /// Raspodeljuje sobe BRUCOSIMA na osnovu vec sacuvanog rang-rezultata
    /// (RankingResult tabela). Uzima u obzir SAMO studente sa statusom Assigned.
    /// Ne dozvoljava ponovnu raspodelu ako je vec izvrsena za ovaj konkurs.
    public async Task<AssignmentResultDto> AssignFreshmenToDormsAsync(int competitionId)
    {
        return await AssignFromRankingResultsAsync(competitionId, isFreshmanView: true);
    }

    /// <summary>
    /// Zajednicka logika - cita sacuvane RankingResult redove za konkurs (filtrirano po
    /// isFreshmanView i Status == Assigned), pa pokusava da dodeli sobu svakom studentu
    /// koji jos nema dodeljenu sobu.
    /// </summary>
    private async Task<AssignmentResultDto> AssignFromRankingResultsAsync(int competitionId, bool isFreshmanView)
    {
        var result = new AssignmentResultDto();

        var competition = await _context.Competitions
            .FirstOrDefaultAsync(c => c.Id == competitionId);

        if (competition == null)
        {
            result.Errors.Add("Competition not found");
            return result;
        }

        result.CompetitionId = competition.Id;
        result.CompetitionName = competition.Name;

        // Uzimamo samo studente koji su Assigned (unutar kvote) iz sacuvanog rang-rezultata
        var assignedResults = await _context.RankingResults
            .Include(r => r.Student)
            .Where(r => r.CompetitionId == competitionId
                        && r.IsFreshman == isFreshmanView
                        && r.Status == AssignmentStatus.Assigned)
            .ToListAsync();

        if (!assignedResults.Any())
        {
            result.Errors.Add("Nema rangiranih studenata sa statusom 'Assigned' za ovaj konkurs. Da li je rangiranje pokrenuto?");
            return result;
        }

        // Zastita od ponovne raspodele: ako bar jedan od ovih studenata vec ima sobu, smatramo
        // da je raspodela vec izvrsena za ovaj konkurs.
        var alreadyAssignedCount = assignedResults.Count(r => r.Student.DormitoryRoomId != null);
        if (alreadyAssignedCount > 0)
        {
            result.Errors.Add("Sobe su već raspodeljene za ovaj konkurs.");
            return result;
        }

        var dorms = await _assignmentRepository.GetAllDormsWithRoomsAsync();
        var totalAvailableSpots = dorms.Sum(d => d.AvailableSpots);

        if (totalAvailableSpots == 0)
        {
            result.Errors.Add("Nema slobodnih mesta u domu.");
            return result;
        }

        // Sortiramo po bodovima opadajuce da bi redosled dodele sobe pratio rang
        // (vec su Assigned, ali medju njima i dalje vazi prioritet po bodovima)
        var orderedResults = assignedResults
            .OrderByDescending(r => r.CalculatedPoints)
            .ToList();

        result.TotalStudents = orderedResults.Count;

        foreach (var rankingResult in orderedResults)
        {
            var student = rankingResult.Student;

            var assignment = new StudentAssignmentDto
            {
                StudentId = student.Id,
                StudentName = student.Name,
                StudentSurname = student.Surname,
                StudentIDNumber = student.StudentIDNumber,
                CalculatedPoints = rankingResult.CalculatedPoints,
                FacultyName = rankingResult.FacultyName,
            };

            var assigned = await TryAssignStudentAsync(assignment, student, dorms);

            if (assigned)
            {
                result.SuccessfullyAssigned++;
                assignment.Status = AssignmentStatus.Assigned;
            }
            else
            {
                // Iako je student bio "Assigned" u rang listi, fizicki nema vise mesta
                // (npr. nema slobodne sobe odgovarajuceg pola) - ide na cekanje za sobu.
                result.Waitlisted++;
                assignment.Status = AssignmentStatus.Waitlisted;
            }

            result.Assignments.Add(assignment);
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

        if (competition.Status == CompetitionStatus.Completed)
        {
            result.Errors.Add("Konkurs je već rangiran i prebačen u status 'Completed'.");
            return result;
        }

        // Samo stariji studenti (ne brucosi), budzetski
        var approvedApplications = competition.Applications
            .Where(a => a.Status == ApplicationStatus.Approved
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

        var oldResults = await _context.RankingResults
            .Where(r => r.CompetitionId == competitionId)
            .ToListAsync();
        _context.RankingResults.RemoveRange(oldResults);

        competition.Status = CompetitionStatus.Completed;

        foreach (var assignment in studentAssignments)
        {
            _context.RankingResults.Add(new RankingResult
            {
                CompetitionId = competition.Id,
                StudentId = assignment.StudentId,
                FacultyName = assignment.FacultyName ?? "Unknown",
                IsFreshman = false,
                CalculatedPoints = assignment.CalculatedPoints,
                Status = assignment.Status,
                AverageGrade = assignment.AverageGrade,
                ESPB = assignment.ESPB,
                YearsOfStudy = assignment.YearsOfStudy,
                TotalFamilyIncome = assignment.TotalFamilyIncome,
                FamilyMemberCount = assignment.FamilyMemberCount,
                IncomePerMember = assignment.IncomePerMember
            });
        }

        await _context.SaveChangesAsync();

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

        if (competition.Status == CompetitionStatus.Completed)
        {
            result.Errors.Add("Konkurs je već rangiran i prebačen u status 'Completed'.");
            return result;
        }

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

        var oldResults = await _context.RankingResults
            .Where(r => r.CompetitionId == competitionId)
            .ToListAsync();
        _context.RankingResults.RemoveRange(oldResults);

        foreach (var assignment in freshmanAssignments)
        {
            _context.RankingResults.Add(new RankingResult
            {
                CompetitionId = competition.Id,
                StudentId = assignment.StudentId,
                FacultyName = assignment.FacultyName ?? "Unknown",
                IsFreshman = true,
                CalculatedPoints = assignment.CalculatedPoints,
                Status = assignment.Status,
                HighSchoolAverageGrade = assignment.HighSchoolAverageGrade,
                HighSchoolPoints = assignment.HighSchoolPoints,
                SocialBonus = assignment.SocialBonus,
                TotalFamilyIncome = assignment.TotalFamilyIncome,
                FamilyMemberCount = assignment.FamilyMemberCount,
                IncomePerMember = assignment.IncomePerMember
            });
        }

        competition.Status = CompetitionStatus.Completed;
        await _context.SaveChangesAsync();

        result.RankingsByFaculty = groupedByFaculty;
        result.TotalStudents = freshmanAssignments.Count;

        return result;
    }

    private static Dictionary<string, int> CalculateFacultyQuotas(
    Dictionary<string, int> facultyApplicationCounts,
    int maxStudents)
    {
        var total = facultyApplicationCounts.Values.Sum();

        if (total == 0)
            return new Dictionary<string, int>();

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

    public async Task<object> GetSavedRankingResultsAsync(int competitionId, bool isFreshmanView)
    {
        var competition = await _context.Competitions
            .FirstOrDefaultAsync(c => c.Id == competitionId);

        var savedResults = await _context.RankingResults
            .Include(r => r.Student)
            .Where(r => r.CompetitionId == competitionId && r.IsFreshman == isFreshmanView)
            .ToListAsync();

        var roomsAssigned = savedResults
       .Where(r => r.Status == AssignmentStatus.Assigned)
       .Any(r => r.Student.DormitoryRoomId != null);

        if (isFreshmanView)
        {
            var freshmanAssignments = savedResults.Select(r => new FreshmanAssignmentDto
            {
                StudentId = r.StudentId,
                StudentName = r.Student.Name,
                StudentSurname = r.Student.Surname,
                StudentIDNumber = r.Student.StudentIDNumber,
                HighSchoolAverageGrade = r.HighSchoolAverageGrade ?? 0,
                FacultyName = r.FacultyName,
                HighSchoolPoints = r.HighSchoolPoints ?? 0,
                SocialBonus = r.SocialBonus ?? 0,
                CalculatedPoints = r.CalculatedPoints,
                TotalFamilyIncome = r.TotalFamilyIncome,
                FamilyMemberCount = r.FamilyMemberCount,
                IncomePerMember = r.IncomePerMember,
                Status = r.Status
            }).ToList();

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

            return new FreshmanRankingResultDto
            {
                CompetitionId = competitionId,
                CompetitionName = competition?.Name ?? string.Empty,
                TotalStudents = freshmanAssignments.Count,
                RankingsByFaculty = groupedByFaculty
            };
        }
        else
        {
            var studentAssignments = savedResults.Select(r => new StudentAssignmentDto
            {
                StudentId = r.StudentId,
                StudentName = r.Student.Name,
                StudentSurname = r.Student.Surname,
                StudentIDNumber = r.Student.StudentIDNumber,
                AverageGrade = r.AverageGrade ?? 0,
                ESPB = r.ESPB ?? 0,
                YearsOfStudy = r.YearsOfStudy ?? 0,
                CalculatedPoints = r.CalculatedPoints,
                TotalFamilyIncome = r.TotalFamilyIncome,
                FamilyMemberCount = r.FamilyMemberCount,
                IncomePerMember = r.IncomePerMember,
                FacultyName = r.FacultyName,
                Status = r.Status
            }).ToList();

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

            return new OlderStudentsRankingResultDto
            {
                CompetitionId = competitionId,
                CompetitionName = competition?.Name ?? string.Empty,
                TotalStudents = studentAssignments.Count,
                RankingsByFaculty = groupedByFaculty
            };
        }
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

        var studentIds = rankedStudents.Where(s => s.Status != AssignmentStatus.Error).Select(s => s.StudentId).ToList();
        var students = await _assignmentRepository.GetStudentsByIdsAsync(studentIds);
        var studentsDict = students.ToDictionary(s => s.Id);

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

            var assigned = await TryAssignStudentAsync(assignment, student, dorms);

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
        // foreach (var assignment in rankedStudents)
        // {
        //     if (assignment.Status == AssignmentStatus.Error)
        //     {
        //         result.Failed++;
        //         result.Assignments.Add(assignment);
        //         continue;
        //     }

        //     var assigned = await TryAssignStudentAsync(assignment, student, dorms);

        //     if (assigned)
        //     {
        //         result.SuccessfullyAssigned++;
        //         assignment.Status = AssignmentStatus.Assigned;
        //     }
        //     else
        //     {
        //         result.Waitlisted++;
        //         assignment.Status = AssignmentStatus.Waitlisted;
        //     }

        //     result.Assignments.Add(assignment);
        // }

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

    public async Task<bool> ResetFlowAsync()
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

    // 2. Novi metod -  reset SAMO za jedan konkurs.
    public async Task<bool> ResetAssignmentAsync(int competitionId)
    {
        var assignedResults = await _context.RankingResults
            .Include(r => r.Student)
            .Where(r => r.CompetitionId == competitionId && r.Status == AssignmentStatus.Assigned)
            .ToListAsync();

        if (!assignedResults.Any())
            return false;

        // Brojimo koliko studenata se uklanja iz svake sobe, da bismo tacno
        // dekrementirali CurrentOccupancy (ne na 0, vec za tacan broj).
        var roomIdsToDecrement = assignedResults
            .Where(r => r.Student.DormitoryRoomId != null)
            .Select(r => r.Student.DormitoryRoomId!.Value)
            .GroupBy(roomId => roomId)
            .ToDictionary(g => g.Key, g => g.Count());

        if (roomIdsToDecrement.Count == 0)
            return false; // nijedan student iz ovog konkursa trenutno nema dodeljenu sobu

        var affectedRooms = await _context.DormitoryRooms
            .Where(r => roomIdsToDecrement.Keys.Contains(r.Id))
            .ToListAsync();

        foreach (var room in affectedRooms)
        {
            var countToRemove = roomIdsToDecrement[room.Id];
            room.CurrentOccupancy = Math.Max(0, room.CurrentOccupancy - countToRemove);
        }

        foreach (var result in assignedResults)
        {
            result.Student.DormitoryRoomId = null;
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

    private async Task<bool> TryAssignStudentAsync(StudentAssignmentDto assignment, Student student, List<Dorm> dorms)
    {
        foreach (var dorm in dorms.Where(d => !d.IsFull))
        {
            // 1. korak: probaj sobu ISTOG pola u ovom domu
            var room = await _assignmentRepository.GetAvailableRoomAsync(dorm.Id, student.Gender);
            var isMixed = false;

            // 2. korak (fallback): ako nema sobe odgovarajuceg pola u ovom domu,
            // dozvoli bilo koju slobodnu sobu u istom domu (mesovita dodela)
            if (room == null)
            {
                room = await _assignmentRepository.GetAnyAvailableRoomAsync(dorm.Id);
                isMixed = room != null;
            }

            if (room != null)
            {
                await _assignmentRepository.AssignStudentToRoomAsync(assignment.StudentId, room.Id);

                room.CurrentOccupancy++;
                await _context.SaveChangesAsync();

                assignment.AssignedDormId = dorm.Id;
                assignment.AssignedDormName = dorm.Name;
                assignment.AssignedRoomId = room.Id;
                assignment.IsMixedGenderAssignment = isMixed;
                return true;
            }
        }

        return false;
    }
    // private async Task<bool> TryAssignStudentAsync(StudentAssignmentDto assignment, Student student, List<Dorm> dorms)
    // {
    //     foreach (var dorm in dorms.Where(d => !d.IsFull))
    //     {
    //         //prvo da se nadje soba istog pola
    //         var room = await _assignmentRepository.GetAvailableRoomAsync(dorm.Id, student.Gender);
    //         var isMixed = false;

    //         // dozvoli bilo koju slobodnu sobu u istom domu (mesovita dodela)
    //         if (room == null)
    //         {
    //             room = await _assignmentRepository.GetAnyAvailableRoomAsync(dorm.Id);
    //             isMixed = room != null;
    //         }

    //         if (room != null)
    //         {
    //             await _assignmentRepository.AssignStudentToRoomAsync(assignment.StudentId, room.Id);

    //             room.CurrentOccupancy++;
    //             await _context.SaveChangesAsync();

    //             assignment.AssignedDormId = dorm.Id;
    //             assignment.AssignedDormName = dorm.Name;
    //             assignment.AssignedRoomId = room.Id;
    //             assignment.IsMixedGenderAssignment = isMixed;
    //             return true;
    //         }
    //     }

    //     return false;
    // }
}
