using Microsoft.EntityFrameworkCore;
using DormMS.Server.Services.Interfaces;
using DormMS.Server.Repositories.Interfaces;
using DormMS.Server.DTOs;
using DormMS.Server.Helpers;
using DormMS.Server.Models;

namespace DormMS.Server.Services.Implementations;

public class AssignmentService : Interfaces.IAssignmentService
{
    private readonly Repositories.Interfaces.IAssignmentRepository _assignmentRepository;
    private readonly INotificationService _notificationService;
    private readonly AppDbContext _context;

    public AssignmentService(Repositories.Interfaces.IAssignmentRepository assignmentRepository, INotificationService notificationService, AppDbContext context)
    {
        _assignmentRepository = assignmentRepository;
        _notificationService = notificationService;
        _context = context;
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

    /// Zajednicka logika - cita sacuvane RankingResult redove za konkurs (filtrirano po
    /// isFreshmanView i Status == Assigned), pa pokusava da dodeli sobu svakom studentu
    /// koji jos nema dodeljenu sobu.
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

        // Zastita od ponovne raspodele: ako bar jedan od ovih studenata vec ima sobu, smatra se
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

        // Sortiranje po bodovima opadajuce da bi redosled dodele sobe pratio rang
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
                await _notificationService.NotifyStudentAsync(
                    student.Id,
                    $"Dodeljena vam je soba {assignment.AssignedRoomId} u domu {assignment.AssignedDormName}.");
            }
            else
            {
                result.Waitlisted++;
                // assignment.Status = AssignmentStatus.Waitlisted;
                assignment.Status = AssignmentStatus.AssignedNoRoomAvailable;
                rankingResult.Status = AssignmentStatus.AssignedNoRoomAvailable;

                await _notificationService.NotifyStudentAsync(
                    student.Id,
                    "Nažalost, trenutno nema slobodnih mesta u domu za Vas. Bićete obavešteni kada se oslobodi soba.");
            }

            result.Assignments.Add(assignment);
        }
        await _context.SaveChangesAsync();
        return result;
    }

    /// Rangira starije studente (ne brucose) po fakultetima.
    /// Formula: (averageGrade * 5) + (espb / yearsOfStudy * 0.8) + espbBonus + financialBonus
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
                continue;
            }

            var calculatedPoints = StudentPointsCalculator.CalculateOlderStudentPoints(
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

        var quotas = FacultyQuotaCalculator.Calculate(
            groupedByFaculty.ToDictionary(k => k.Key, v => v.Value.Count),
            competition.MaxStudents);

        foreach (var faculty in groupedByFaculty)
        {
            var quota = quotas[faculty.Key];
            for (int i = 0; i < faculty.Value.Count; i++)
                faculty.Value[i].Status = i < quota
                    ? AssignmentStatus.Assigned
                    : AssignmentStatus.Waitlisted;
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

    /// Rangira brucose po fakultetima.
    /// Formula: bb = po * 8 (po = prosecna ocena iz srednje skole, bb = bodovi brucosa)
    /// + socijalni bonus (1 bod ako je prihod po clanu < 50% prosecne plate)
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
            .Where(a => /*(*/ a.Status == ApplicationStatus.Approved
                        && a.Student.IsBudgetStudent
                        && a.Student.IsFreshman) // samo brucosi
            .ToList();

        var freshmanAssignments = new List<FreshmanAssignmentDto>();

        foreach (var application in approvedApplications)
        {
            var student = application.Student;
            var incomePerMember = student.FinancialStatus?.IncomePerMember ?? 0;

            var highSchoolAverage = student.HighSchoolAverageGrade ?? 0;
            var points = StudentPointsCalculator.CalculateFreshmanPoints(highSchoolAverage, incomePerMember);

            var assignment = new FreshmanAssignmentDto
            {
                StudentId = student.Id,
                StudentName = student.Name,
                StudentSurname = student.Surname,
                StudentIDNumber = student.StudentIDNumber,
                HighSchoolAverageGrade = highSchoolAverage,
                FacultyName = student.FacultyName,
                HighSchoolPoints = points.HighSchoolPoints,
                SocialBonus = points.SocialBonus,
                CalculatedPoints = points.CalculatedPoints,
                TotalFamilyIncome = student.FinancialStatus?.TotalFamilyIncome ?? 0,
                FamilyMemberCount = student.FinancialStatus?.FamilyMemberCount ?? 0,
                IncomePerMember = incomePerMember,
                Status = AssignmentStatus.NotAssigned
            };

            freshmanAssignments.Add(assignment);
        }

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

        var quotas = FacultyQuotaCalculator.Calculate(
            groupedByFaculty.ToDictionary(k => k.Key, v => v.Value.Count),
            competition.MaxStudents);

        foreach (var faculty in groupedByFaculty)
        {
            var quota = quotas[faculty.Key];
            for (int i = 0; i < faculty.Value.Count; i++)
                faculty.Value[i].Status = i < quota
                    ? AssignmentStatus.Assigned
                    : AssignmentStatus.Waitlisted;
        }

        var oldResults = await _context.RankingResults
            .Where(r => r.CompetitionId == competitionId)
            .ToListAsync();
        _context.RankingResults.RemoveRange(oldResults);

        competition.Status = CompetitionStatus.Completed;

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

        await _context.SaveChangesAsync();

        result.RankingsByFaculty = groupedByFaculty;
        result.TotalStudents = freshmanAssignments.Count;

        return result;
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
                RankingResultId = r.Id,
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
                Status = r.Status,
                IsPhysicallyAssigned = r.Student.DormitoryRoomId != null
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
                RankingsByFaculty = groupedByFaculty,
                RoomsAssigned = roomsAssigned
            };
        }
        else
        {
            var studentAssignments = savedResults.Select(r => new StudentAssignmentDto
            {
                RankingResultId = r.Id,
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
                Status = r.Status,
                IsPhysicallyAssigned = r.Student.DormitoryRoomId != null
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
                RankingsByFaculty = groupedByFaculty,
                RoomsAssigned = roomsAssigned
            };
        }
    }

    public async Task<StudentAssignmentDto> AssignSingleStudentAsync(int rankingResultId)
    {
        var rankingResult = await _context.RankingResults
            .Include(r => r.Student)
            .FirstOrDefaultAsync(r => r.Id == rankingResultId)
            ?? throw new KeyNotFoundException("Rezultat rangiranja nije pronađen.");

        if (rankingResult.Student.DormitoryRoomId != null)
            throw new InvalidOperationException("Student već ima dodeljenu sobu.");

        if (rankingResult.Status != AssignmentStatus.Assigned
            && rankingResult.Status != AssignmentStatus.AssignedNoRoomAvailable
            && rankingResult.Status != AssignmentStatus.Waitlisted)
            throw new InvalidOperationException("Student nije prošao rangiranje po kvoti.");

        var dorms = await _assignmentRepository.GetAllDormsWithRoomsAsync();

        DormitoryRoom? foundRoom = null;
        Dorm? foundDorm = null;

        foreach (var dorm in dorms)
        {
            var room = await _assignmentRepository.GetAvailableRoomAsync(dorm.Id, rankingResult.Student.Gender);
            if (room != null)
            {
                foundRoom = room;
                foundDorm = dorm;
                break;
            }
        }

        if (foundRoom == null)
        {
            bool anyRoomAnywhere = false;
            foreach (var dorm in dorms)
            {
                if (await _assignmentRepository.GetAnyAvailableRoomAsync(dorm.Id) != null)
                {
                    anyRoomAnywhere = true;
                    break;
                }
            }

            throw new InvalidOperationException(
                anyRoomAnywhere
                    ? "Trenutno nema slobodne sobe odgovarajućeg pola."
                    : "Nema slobodnih mesta u domu.");
            //  throw new InvalidOperationException("Trenutno nema slobodne sobe odgovarajućeg pola.");
        }


        await _assignmentRepository.AssignStudentToRoomAsync(rankingResult.StudentId, foundRoom.Id);

        rankingResult.Status = AssignmentStatus.Assigned;
        await _context.SaveChangesAsync();

        await _notificationService.NotifyStudentAsync(
            rankingResult.StudentId,
            $"Dodeljena vam je soba {foundRoom.RoomNumber} u domu.");

        return new StudentAssignmentDto
        {
            RankingResultId = rankingResult.Id,
            StudentId = rankingResult.StudentId,
            StudentName = rankingResult.Student.Name,
            StudentSurname = rankingResult.Student.Surname,
            StudentIDNumber = rankingResult.Student.StudentIDNumber,
            AssignedDormId = foundDorm!.Id,
            AssignedDormName = foundDorm.Name,
            AssignedRoomId = foundRoom.Id,
            Status = AssignmentStatus.Assigned
        };
    }

    public async Task RemoveStudentFromRoomAsync(int studentId)
    {
        var student = await _context.Students
            .Include(s => s.DormitoryRoom)
            .FirstOrDefaultAsync(s => s.Id == studentId)
            ?? throw new KeyNotFoundException("Student nije pronađen.");

        if (student.DormitoryRoomId == null)
            throw new InvalidOperationException("Student trenutno nema dodeljenu sobu.");

        var roomNumber = student.DormitoryRoom?.RoomNumber ?? "";

        await _assignmentRepository.ReleaseRoomAsync(studentId);

        var rankingResult = await _context.RankingResults
            .FirstOrDefaultAsync(r => r.StudentId == studentId);
        if (rankingResult != null)
        {
            rankingResult.Status = AssignmentStatus.Expelled;
            await _context.SaveChangesAsync();
        }

        await _notificationService.NotifyStudentAsync(
            studentId,
            $"Uklonjeni ste iz sobe {roomNumber}. Za dodatne informacije obratite se referentu doma.");
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
            room.Status = CalculateStatus(room);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    // reset SAMO za jedan konkurs.
    public async Task<bool> ResetAssignmentAsync(int competitionId)
    {
        var assignedResults = await _context.RankingResults
            .Include(r => r.Student)
            .Where(r => r.CompetitionId == competitionId && r.Status == AssignmentStatus.Assigned)
            .ToListAsync();

        if (!assignedResults.Any())
            return false;

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
            room.Status = CalculateStatus(room);
        }

        foreach (var result in assignedResults)
        {
            result.Student.DormitoryRoomId = null;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    private static RoomStatus CalculateStatus(DormitoryRoom room)
    {
        if (room.Status == RoomStatus.Maintenance)
            return RoomStatus.Maintenance;

        if (room.Status == RoomStatus.Closed)
            return RoomStatus.Closed;

        return room.CurrentOccupancy >= room.Capacity
            ? RoomStatus.FullyOccupied
            : RoomStatus.Available;
    }

    private async Task<bool> TryAssignStudentAsync(StudentAssignmentDto assignment, Student student, List<Dorm> dorms)
    {
        foreach (var dorm in dorms)
        {
            var room = await _assignmentRepository.GetAvailableRoomAsync(dorm.Id, student.Gender);

            if (room == null)
            {
                continue;
            }

            await _assignmentRepository.AssignStudentToRoomAsync(assignment.StudentId, room.Id);

            assignment.AssignedDormId = dorm.Id;
            assignment.AssignedDormName = dorm.Name;
            assignment.AssignedRoomId = room.Id;

            return true;

        }

        return false;
    }
}
