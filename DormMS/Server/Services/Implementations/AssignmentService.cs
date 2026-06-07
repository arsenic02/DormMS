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
        var rankedStudents = await RankStudentsAsync(competitionId);
        return await AssignRankedStudentsAsync(rankedStudents);
    }

    public async Task<List<StudentAssignmentDto>> RankStudentsAsync(int competitionId)
    {
        var competition = await _assignmentRepository.GetCompetitionWithApplicationsAsync(competitionId);
        if (competition == null)
        {
            return new List<StudentAssignmentDto>();
        }

        var approvedApplications = competition.Applications
            .Where(a => a.Status == ApplicationStatus.Approved)
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

    private decimal CalculatePoints(decimal averageGrade, int espb, int yearsOfStudy, decimal incomePerMember)
    {
        if (yearsOfStudy <= 0)
            return 0;

        var academicPoints = (averageGrade * 5) + ((decimal)espb / yearsOfStudy * 0.8m);
        
        // Financial points: lower income per member = higher points
        // Assuming average income per member is around 10000, we give bonus points for lower income
        var financialPoints = 0m;
        if (incomePerMember > 0)
        {
            // Max 2 points for financial status (income per member <= 5000 gets max points)
            financialPoints = Math.Max(0, 2m - (incomePerMember / 5000m));
        }
        
        var totalPoints = academicPoints + financialPoints;
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
