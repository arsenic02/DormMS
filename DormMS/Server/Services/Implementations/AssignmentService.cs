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
        var result = new AssignmentResultDto { CompetitionId = competitionId };
        
        var competition = await _assignmentRepository.GetCompetitionWithApplicationsAsync(competitionId);
        if (competition == null)
        {
            result.Errors.Add("Competition not found");
            return result;
        }

        result.CompetitionName = competition.Name;

        var approvedApplications = competition.Applications
            .Where(a => a.Status == ApplicationStatus.Approved)
            .ToList();

        result.TotalStudents = approvedApplications.Count;

        if (result.TotalStudents == 0)
        {
            result.Errors.Add("No approved applications found for this competition");
            return result;
        }

        var dorms = await _assignmentRepository.GetAllDormsWithRoomsAsync();
        var totalAvailableSpots = dorms.Sum(d => d.AvailableSpots);

        if (totalAvailableSpots == 0)
        {
            result.Errors.Add("No available spots in any dormitory");
            return result;
        }

        var studentAssignments = new List<StudentAssignmentDto>();

        foreach (var application in approvedApplications)
        {
            var student = application.Student;
            
            if (student.IsFreshman)
            {
                result.Errors.Add($"Student {student.StudentIDNumber} is a freshman - freshman competition logic not implemented");
                result.Failed++;
                continue;
            }

            var yearsOfStudy = student.CurrentYearOfStudy;
            if (yearsOfStudy <= 0)
            {
                result.Errors.Add($"Student {student.StudentIDNumber} has invalid years of study");
                result.Failed++;
                continue;
            }

            var calculatedPoints = CalculatePoints(student.AverageGrade, student.ESPB, yearsOfStudy);

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
                Status = AssignmentStatus.NotAssigned
            };

            studentAssignments.Add(assignment);
        }

        studentAssignments = studentAssignments
            .OrderByDescending(s => s.CalculatedPoints)
            .ToList();

        foreach (var assignment in studentAssignments)
        {
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

    private decimal CalculatePoints(decimal averageGrade, int espb, int yearsOfStudy)
    {
        if (yearsOfStudy <= 0)
            return 0;

        var points = (averageGrade * 5) + ((decimal)espb / yearsOfStudy * 0.8m);
        return Math.Round(points, 2);
    }

    private async Task<bool> TryAssignStudentAsync(StudentAssignmentDto assignment, List<Dorm> dorms)
    {
        foreach (var dorm in dorms.Where(d => !d.IsFull))
        {
            var room = await _assignmentRepository.GetAvailableRoomAsync(dorm.Id);
            if (room != null)
            {
                await _assignmentRepository.AssignStudentToRoomAsync(assignment.StudentId, room.Id);
                assignment.AssignedDormId = dorm.Id;
                assignment.AssignedDormName = dorm.Name;
                assignment.AssignedRoomId = room.Id;
                return true;
            }
        }

        return false;
    }
}
