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
        var rankedStudents = await RankStudentsAsync(competitionId);
        var result = await AssignRankedStudentsAsync(rankedStudents);

        if (competition != null)
        {
            result.CompetitionId = competition.Id;
            result.CompetitionName = competition.Name;
        }

        return result;
    }

    public async Task<List<StudentAssignmentDto>> RankStudentsAsync(int competitionId)
    {
        var competition = await _assignmentRepository.GetCompetitionWithApplicationsAsync(competitionId);
        if (competition == null)
        {
            return new List<StudentAssignmentDto>();
        }

        var approvedApplications = competition.Applications
            .Where(a => (a.Status == ApplicationStatus.Approved || a.Status == ApplicationStatus.Pending) && a.Student.IsBudgetStudent)
            .ToList();

        var studentAssignments = new List<StudentAssignmentDto>();

        foreach (var application in approvedApplications)
        {
            var student = application.Student;
            
            if (student.IsFreshman)
            {
                var failedAssignment = new StudentAssignmentDto
                {
                    StudentId = student.Id,
                    StudentName = student.Name,
                    StudentSurname = student.Surname,
                    StudentIDNumber = student.StudentIDNumber,
                    AverageGrade = student.AverageGrade,
                    ESPB = student.ESPB,
                    YearsOfStudy = student.CurrentYearOfStudy,
                    CalculatedPoints = 0,
                    Status = AssignmentStatus.Error
                };
                studentAssignments.Add(failedAssignment);
                continue;
            }

            var yearsOfStudy = student.CurrentYearOfStudy;
            if (yearsOfStudy <= 0)
            {
                var failedAssignment = new StudentAssignmentDto
                {
                    StudentId = student.Id,
                    StudentName = student.Name,
                    StudentSurname = student.Surname,
                    StudentIDNumber = student.StudentIDNumber,
                    AverageGrade = student.AverageGrade,
                    ESPB = student.ESPB,
                    YearsOfStudy = yearsOfStudy,
                    CalculatedPoints = 0,
                    Status = AssignmentStatus.Error
                };
                studentAssignments.Add(failedAssignment);
                continue;
            }

            var calculatedPoints = CalculatePoints(student.AverageGrade, student.ESPB, yearsOfStudy, 
                student.FinancialStatus?.IncomePerMember ?? 0);

            var assignment = new StudentAssignmentDto
            {
                StudentId = student.Id,
                StudentName = student.Name,
                StudentSurname = student.Surname,
                StudentIDNumber = student.StudentIDNumber,
                AverageGrade = student.AverageGrade,
                ESPB = student.ESPB,
                YearsOfStudy = yearsOfStudy,
                CalculatedPoints = calculatedPoints,
                TotalFamilyIncome = student.FinancialStatus?.TotalFamilyIncome ?? 0,
                FamilyMemberCount = student.FinancialStatus?.FamilyMemberCount ?? 0,
                IncomePerMember = student.FinancialStatus?.IncomePerMember ?? 0,
                Status = AssignmentStatus.NotAssigned
            };

            studentAssignments.Add(assignment);
        }

        return studentAssignments
            .OrderByDescending(s => s.CalculatedPoints)
            .ThenByDescending(s => s.ESPB)                
            .ThenBy(s => s.YearsOfStudy)                 
            .ThenByDescending(s => s.AverageGrade)       
            .ThenBy(s => s.IncomePerMember)             
            .ToList();
    }

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

    private decimal CalculatePoints(decimal averageGrade, int espb, int yearsOfStudy, decimal incomePerMember)
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
