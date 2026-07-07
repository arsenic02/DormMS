using DormMS.Server.Models;
using DormMS.Server.DTOs;

namespace DormMS.Server.Services.Interfaces;


public record ServiceResult<T>(T? Value, string? Error = null)
{
    public bool IsSuccess => Error == null;
    public static ServiceResult<T> Success(T value) => new ServiceResult<T>(value);
    public static ServiceResult<T> Fail(string error) => new ServiceResult<T>(default, error);
}

public interface ILaundryService
{
    Task<IEnumerable<LaundryMachineGetDto>> GetAllMachinesAsync();
    Task<IEnumerable<LaundryMachineGetDto>> GetMachinesByDormAsync(int dormId);
    Task<LaundryMachineGetDto?> GetMachineByIdAsync(int id);
    Task<LaundryMachine?> CreateMachineAsync(LaundryMachineCreateUpdateDto dto);
    Task<bool?> UpdateMachineAsync(int id, LaundryMachineCreateUpdateDto dto);
    Task<bool> DeleteMachineAsync(int id);
    Task ToggleMachineStatusAsync(int id);

    Task<IEnumerable<LaundrySlotDto>> GetAvailableSlotsAsync(int machineId, DateTime date);
    Task<ServiceResult<LaundryReservationGetDto>> CreateReservationAsync(LaundryReservationCreateDto dto, int studentId);
    Task<ServiceResult<bool>> CancelReservationAsync(int reservationId, int studentId);
    Task<IEnumerable<LaundryReservationGetDto>> GetStudentReservationsAsync(int studentId);
    Task<IEnumerable<LaundryReservationGetDto>> GetMachineReservationsAsync(int machineId);
}
