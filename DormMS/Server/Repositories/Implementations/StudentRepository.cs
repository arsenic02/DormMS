using Microsoft.EntityFrameworkCore;
using DormMS.Server.Repositories.Interfaces;
using DormMS.Server.Models;
using DormMS.Server.DTOs;

namespace DormMS.Server.Repositories.Implementations;

public class StudentRepository : IStudentRepository
{
    private readonly AppDbContext _context;

    public StudentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Student>> GetAllAsync()
    {
        return await _context.Students
        .Include(s => s.FinancialStatus)
        .AsNoTracking()
        .ToListAsync();
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        return await _context.Students
            .Include(s => s.FinancialStatus)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Student?> GetByUserIdAsync(int userId)
    {
        return await _context.Students
            .Include(s => s.FinancialStatus)
            .FirstOrDefaultAsync(s => _context.Users
                .Any(u => u.Id == userId && u.StudentId == s.Id));
    }

    public async Task<Student?> GetByIdTrackedAsync(int id)
    {
        return await _context.Students.FindAsync(id);
    }

    public async Task<Student?> GetByIdTrackedByUserIdAsync(int userId)
    {
        var studentId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.StudentId)
            .FirstOrDefaultAsync();

        if (!studentId.HasValue)
            return null;

        return await GetByIdTrackedAsync(studentId.Value);
    }

    public async Task<Student?> GetByIdWithFinancialStatusAsync(int id)
    {
        return await _context.Students
            .Include(s => s.FinancialStatus)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<StudentFinancialStatus?> GetFinancialStatusByStudentIdAsync(int studentId)
    {
        return await _context.StudentFinancialStatuses
            .FirstOrDefaultAsync(x => x.StudentId == studentId);
    }

    public async Task AddFinancialStatusAsync(StudentFinancialStatus financialStatus)
    {
        _context.StudentFinancialStatuses.Add(financialStatus);
        await _context.SaveChangesAsync();
    }

    public async Task AddAsync(Student student)
    {
        _context.Students.Add(student);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Student student)
    {
        _context.Students.Update(student);//mozda bude problem u nekim metodama, ali 99% ne bi trebalo
        await _context.SaveChangesAsync();
    }
    public async Task UpdateProfileAsync(Student student)
    {
        _context.Students.Update(student);
        await _context.SaveChangesAsync();
    }
    // public async Task<bool> DeleteAsync(int id)
    // {
    //     var student = await _context.Students.FindAsync(id);
    //     if (student is null)
    //         return false;

    //     _context.Students.Remove(student);
    //     await _context.SaveChangesAsync();
    //     return true;
    // }

    public async Task<bool> DormitoryRoomExistsAsync(int roomId)
    {
        return await _context.DormitoryRooms.AnyAsync(r => r.Id == roomId);
    }

    public async Task<Student?> GetByStudentIdNumberAsync(string studentIdNumber)
    {
        return await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.StudentIDNumber == studentIdNumber);
    }

    public async Task<PagedResultDto<StudentGetDto>> GetPagedAsync(
    int pageNumber,
    int pageSize,
    StudentProfileStatus? profileStatusFilter,
    string? facultyFilter,
    bool? isFreshmanFilter,
    FinancialVerificationStatus? financialStatusFilter = null)
    {
        var query = _context.Students
            .AsNoTracking()
            .Include(s => s.FinancialStatus)
            .AsQueryable();

        if (profileStatusFilter.HasValue)
            query = query.Where(s => s.ProfileStatus == profileStatusFilter.Value);

        if (!string.IsNullOrWhiteSpace(facultyFilter))
            query = query.Where(s => s.FacultyName != null && s.FacultyName.Contains(facultyFilter));

        if (financialStatusFilter.HasValue)
            query = query.Where(s => s.FinancialStatus != null && s.FinancialStatus.VerificationStatus == financialStatusFilter.Value);

        var totalCount = await query.CountAsync();

        // IsFreshman je computed property (DateTime.Now.Year - YearOfEnrollment + 1 == 1),
        // ne postoji kao kolona u bazi, pa filter mora da se primeni nad materijalizovanom listom
        var students = await query
            .OrderBy(s => s.Surname)
            .ThenBy(s => s.Name)
            .ToListAsync();

        if (isFreshmanFilter.HasValue)
            students = students.Where(s => s.IsFreshman == isFreshmanFilter.Value).ToList();

        var totalCountAfterFreshmanFilter = isFreshmanFilter.HasValue ? students.Count : totalCount;

        var pagedStudents = students
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var items = pagedStudents.Select(s => new StudentGetDto
        {
            Id = s.Id,
            Name = s.Name,
            Surname = s.Surname,
            Email = s.Email,
            JMBG = s.JMBG,
            StudentIDNumber = s.StudentIDNumber,
            HomeAddress = s.HomeAddress,
            MunicipalityOfResidence = s.MunicipalityOfResidence,
            Age = s.Age,
            Gender = s.Gender,
            YearOfEnrollment = s.YearOfEnrollment,
            StudyLevel = s.StudyLevel,
            Status = s.Status,
            FacultyId = s.FacultyId,
            FacultyName = s.FacultyName,
            ESPB = s.ESPB,
            AverageGrade = s.AverageGrade,
            HighSchoolAverageGrade = s.HighSchoolAverageGrade,
            IsBudgetStudent = s.IsBudgetStudent,
            DormitoryRoomId = s.DormitoryRoomId,
            CurrentYearOfStudy = s.CurrentYearOfStudy,
            IsFreshman = s.IsFreshman,
            ProfileStatus = s.ProfileStatus,
            IsEligibleToApply = s.IsEligibleToApply,
            FinancialStatus = s.FinancialStatus != null ? new StudentFinancialStatusGetDto
            {
                TotalFamilyIncome = s.FinancialStatus.TotalFamilyIncome,
                FamilyMemberCount = s.FinancialStatus.FamilyMemberCount,
                IncomePerMember = s.FinancialStatus.IncomePerMember,
                VerificationStatus = s.FinancialStatus.VerificationStatus,
                SubmittedAt = s.FinancialStatus.SubmittedAt,
                VerifiedAt = s.FinancialStatus.VerifiedAt
            } : null
        }).ToList();

        return new PagedResultDto<StudentGetDto>
        {
            Items = items,
            TotalCount = totalCountAfterFreshmanFilter,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<List<Student>> SearchForRoomSwapAsync(string? idNumber,
    string? name, string? surname, string? faculty, string? room, int excludeStudentId)
    {
        var query = _context.Students
            .Include(s => s.DormitoryRoom)
            .Where(s => s.Id != excludeStudentId && s.DormitoryRoomId != null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(idNumber))
            query = query.Where(s => EF.Functions.ILike(s.StudentIDNumber, $"%{idNumber}%"));

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(s => EF.Functions.ILike(s.Name, $"%{name}%"));

        if (!string.IsNullOrWhiteSpace(surname))
            query = query.Where(s => EF.Functions.ILike(s.Surname, $"%{surname}%"));

        // if (!string.IsNullOrWhiteSpace(faculty))
        //     query = query.Where(s => EF.Functions.ILike(s.FacultyName, $"%{faculty}%"));
        if (!string.IsNullOrWhiteSpace(faculty))
            query = query.Where(s => s.FacultyName != null && EF.Functions.ILike(s.FacultyName, $"%{faculty}%"));

        if (!string.IsNullOrWhiteSpace(room))
            query = query.Where(s => s.DormitoryRoom != null && EF.Functions.ILike(s.DormitoryRoom.RoomNumber, $"%{room}%"));

        return await query.Take(20).ToListAsync();
    }
}
