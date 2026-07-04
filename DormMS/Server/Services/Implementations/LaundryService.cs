using Microsoft.EntityFrameworkCore;
using DormMS.Server.Services.Interfaces;
using DormMS.Server.Models;
using DormMS.Server.DTOs;

namespace DormMS.Server.Services.Implementations;

public class LaundryService : ILaundryService
{
    private readonly AppDbContext _context;
    private const int ReservationDurationHours = 2;
    private const int MaxReservationsPerDay = 1;

    public LaundryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LaundryMachineGetDto>> GetAllMachinesAsync()
    {
        var machines = await _context.LaundryMachines
            .Include(m => m.Dorm)
            .ToListAsync();
        return machines.Select(MapToMachineDto);
    }

    public async Task<IEnumerable<LaundryMachineGetDto>> GetMachinesByDormAsync(int dormId)
    {
        var machines = await _context.LaundryMachines
            .Include(m => m.Dorm)
            .Where(m => m.DormId == dormId)
            .ToListAsync();
        return machines.Select(MapToMachineDto);
    }

    public async Task<LaundryMachineGetDto?> GetMachineByIdAsync(int id)
    {
        var machine = await _context.LaundryMachines
            .Include(m => m.Dorm)
            .FirstOrDefaultAsync(m => m.Id == id);
        return machine is null ? null : MapToMachineDto(machine);
    }

    public async Task<LaundryMachine?> CreateMachineAsync(LaundryMachineCreateUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return null;

        var dormExists = await _context.Dorms.AnyAsync(d => d.Id == dto.DormId);
        if (!dormExists)
            return null;

        var machine = new LaundryMachine
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            DormId = dto.DormId,
            IsActive = true
        };

        await _context.LaundryMachines.AddAsync(machine);
        await _context.SaveChangesAsync();
        return machine;
    }

    public async Task<bool?> UpdateMachineAsync(int id, LaundryMachineCreateUpdateDto dto)
    {
        var machine = await _context.LaundryMachines.FindAsync(id);
        if (machine is null)
            return null;

        if (string.IsNullOrWhiteSpace(dto.Name))
            return false;

        var dormExists = await _context.Dorms.AnyAsync(d => d.Id == dto.DormId);
        if (!dormExists)
            return false;

        machine.Name = dto.Name.Trim();
        machine.Description = dto.Description?.Trim();
        machine.DormId = dto.DormId;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteMachineAsync(int id)
    {
        var machine = await _context.LaundryMachines.FindAsync(id);
        if (machine is null)
            return false;

        _context.LaundryMachines.Remove(machine);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task ToggleMachineStatusAsync(int id)
    {
        var machine = await _context.LaundryMachines.FindAsync(id);
        if (machine is not null)
        {
            machine.IsActive = !machine.IsActive;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<LaundrySlotDto>> GetAvailableSlotsAsync(int machineId, DateTime date)
    {
        var machine = await _context.LaundryMachines.FindAsync(machineId);
        if (machine is null || !machine.IsActive)
            return new List<LaundrySlotDto>();

        // Koristimo "naivne" datume (bez vremenskog zona)
        var startOfDay = DateTime.SpecifyKind(date.Date, DateTimeKind.Unspecified);
        var endOfDay = startOfDay.AddDays(1);

        var reservations = await _context.LaundryReservations
            .Where(r => r.LaundryMachineId == machineId &&
                        r.StartTime >= startOfDay &&
                        r.StartTime < endOfDay &&
                        r.Status != LaundryReservationStatus.Cancelled)
            .Include(r => r.Student)
            .ToListAsync();

        var slots = new List<LaundrySlotDto>();
        var currentTime = startOfDay.AddHours(6);

        while (currentTime < startOfDay.AddHours(22))
        {
            var slotEnd = currentTime.AddHours(ReservationDurationHours);
            var overlappingReservation = reservations.FirstOrDefault(r =>
                r.StartTime < slotEnd && r.EndTime > currentTime);

            slots.Add(new LaundrySlotDto
            {
                StartTime = currentTime,
                EndTime = slotEnd,
                IsAvailable = overlappingReservation is null,
                Reservation = overlappingReservation is null ? null : MapToReservationDto(overlappingReservation)
            });

            currentTime = slotEnd;
        }

        return slots;
    }

    public async Task<ServiceResult<LaundryReservationGetDto>> CreateReservationAsync(LaundryReservationCreateDto dto, int studentId)
    {
        var student = await _context.Students.FindAsync(studentId);
        if (student is null)
            return ServiceResult<LaundryReservationGetDto>.Fail("Student nije pronađen");

        // Provera da li student ima dodeljenu sobu
        if (!student.DormitoryRoomId.HasValue)
            return ServiceResult<LaundryReservationGetDto>.Fail("Ne možete rezervisati veš mašinu jer nemate dodeljenu sobu u domu");

        var machine = await _context.LaundryMachines.FindAsync(dto.LaundryMachineId);
        if (machine is null)
            return ServiceResult<LaundryReservationGetDto>.Fail("Mašina nije pronađena");
        if (!machine.IsActive)
            return ServiceResult<LaundryReservationGetDto>.Fail("Mašina nije aktivna");

        // Koristimo "naivne" datume (bez vremenskog zona)
        var startTime = DateTime.SpecifyKind(dto.StartTime, DateTimeKind.Unspecified);
        var endTime = startTime.AddHours(ReservationDurationHours);
        var now = DateTime.Now;

        if (startTime < now)
            return ServiceResult<LaundryReservationGetDto>.Fail($"Termin je već prošao (traženo: {startTime}, sada: {now})");

        if (startTime.TimeOfDay < new TimeSpan(6, 0, 0) || startTime.TimeOfDay >= new TimeSpan(22, 0, 0))
            return ServiceResult<LaundryReservationGetDto>.Fail("Rezervacije su moguće samo između 06:00 i 22:00");

        var hasOverlap = await _context.LaundryReservations
            .AnyAsync(r => r.LaundryMachineId == dto.LaundryMachineId &&
                           r.Status != LaundryReservationStatus.Cancelled &&
                           r.StartTime < endTime && r.EndTime > startTime);
        if (hasOverlap)
            return ServiceResult<LaundryReservationGetDto>.Fail("Ovaj termin je već zauzet");

        var startOfDay = startTime.Date;
        var endOfDay = startOfDay.AddDays(1);
        var reservationsToday = await _context.LaundryReservations
            .CountAsync(r => r.StudentId == studentId &&
                             r.StartTime >= startOfDay &&
                             r.StartTime < endOfDay &&
                             r.Status != LaundryReservationStatus.Cancelled);
        if (reservationsToday >= MaxReservationsPerDay)
            return ServiceResult<LaundryReservationGetDto>.Fail("Već imate rezervaciju za danas");

        var reservation = new LaundryReservation
        {
            LaundryMachineId = dto.LaundryMachineId,
            StudentId = studentId,
            StartTime = startTime,
            EndTime = endTime,
            Status = LaundryReservationStatus.Upcoming,
            CreatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)
        };

        await _context.LaundryReservations.AddAsync(reservation);
        await _context.SaveChangesAsync();

        await _context.Entry(reservation).Reference(r => r.LaundryMachine).LoadAsync();
        await _context.Entry(reservation).Reference(r => r.Student).LoadAsync();

        return ServiceResult<LaundryReservationGetDto>.Success(MapToReservationDto(reservation));
    }

    public async Task<ServiceResult<bool>> CancelReservationAsync(int reservationId, int studentId)
  {
    var reservation = await _context.LaundryReservations
      .FirstOrDefaultAsync(r => r.Id == reservationId && r.StudentId == studentId);

    if (reservation is null)
      return ServiceResult<bool>.Fail("Rezervacija nije pronađena");
    
    if (reservation.Status == LaundryReservationStatus.Cancelled)
      return ServiceResult<bool>.Fail("Rezervacija je već otkazana");
    
    if (reservation.Status == LaundryReservationStatus.Completed)
      return ServiceResult<bool>.Fail("Rezervacija je već završena");

    // Uklonjeno ograničenje: može otkazati sve dok rezervacija nije počela
    if (reservation.StartTime <= DateTime.Now)
      return ServiceResult<bool>.Fail("Rezervacija je već počela, nije moguće otkazati");

    reservation.Status = LaundryReservationStatus.Cancelled;
    await _context.SaveChangesAsync();
    return ServiceResult<bool>.Success(true);
  }

    public async Task<IEnumerable<LaundryReservationGetDto>> GetStudentReservationsAsync(int studentId)
    {
        var reservations = await _context.LaundryReservations
            .Where(r => r.StudentId == studentId)
            .Include(r => r.LaundryMachine)
            .Include(r => r.Student)
            .OrderByDescending(r => r.StartTime)
            .ToListAsync();
        return reservations.Select(MapToReservationDto);
    }

    public async Task<IEnumerable<LaundryReservationGetDto>> GetMachineReservationsAsync(int machineId)
    {
        var reservations = await _context.LaundryReservations
            .Where(r => r.LaundryMachineId == machineId)
            .Include(r => r.LaundryMachine)
            .Include(r => r.Student)
            .OrderByDescending(r => r.StartTime)
            .ToListAsync();
        return reservations.Select(MapToReservationDto);
    }

    private static LaundryMachineGetDto MapToMachineDto(LaundryMachine machine) => new()
    {
        Id = machine.Id,
        Name = machine.Name,
        Description = machine.Description,
        IsActive = machine.IsActive,
        DormId = machine.DormId,
        DormName = machine.Dorm.Name
    };

    private static LaundryReservationGetDto MapToReservationDto(LaundryReservation reservation)
    {
        // Dinamički određujemo status
        var now = DateTime.Now;
        string dynamicStatus;
        
        if (reservation.Status == LaundryReservationStatus.Cancelled)
        {
            // Ako je već otkazana, zadržavamo status Cancelled
            dynamicStatus = LaundryReservationStatus.Cancelled.ToString();
        }
        else if (now < reservation.StartTime)
        {
            // Rezeravcija još nije počela
            dynamicStatus = LaundryReservationStatus.Upcoming.ToString();
        }
        else if (now >= reservation.StartTime && now <= reservation.EndTime)
        {
            // Rezervacija je aktivna upravo sada
            dynamicStatus = LaundryReservationStatus.Active.ToString();
        }
        else
        {
            // Rezervacija je završena
            dynamicStatus = LaundryReservationStatus.Completed.ToString();
        }

        return new LaundryReservationGetDto
        {
            Id = reservation.Id,
            LaundryMachineId = reservation.LaundryMachineId,
            LaundryMachineName = reservation.LaundryMachine.Name,
            StudentId = reservation.StudentId,
            StudentName = $"{reservation.Student.Name} {reservation.Student.Surname}",
            StartTime = reservation.StartTime,
            EndTime = reservation.EndTime,
            Status = dynamicStatus,
            CreatedAt = reservation.CreatedAt
        };
    }
}
