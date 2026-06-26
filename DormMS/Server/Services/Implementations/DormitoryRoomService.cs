using DormMS.Server.Services.Interfaces;
using DormMS.Server.Repositories.Interfaces;
using DormMS.Server.Models;
using DormMS.Server.DTOs;

namespace DormMS.Server.Services.Implementations;

public class DormitoryRoomService : IDormitoryRoomService
{
    private readonly IDormitoryRoomRepository _repository;

    public DormitoryRoomService(IDormitoryRoomRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<DormitoryRoom>> GetAllAsync() => _repository.GetAllAsync();

    public Task<DormitoryRoom?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
    public async Task<DormitoryRoom?> CreateAsync(DormitoryRoomCreateUpdateDto dto)
    {
        if (!await IsValidAsync(dto))
            return null;

        var room = MapToEntity(dto);
        await _repository.AddAsync(room);
        return room;
    }

    public async Task<bool?> UpdateAsync(int id, DormitoryRoomCreateUpdateDto dto)
    {
        var room = await _repository.GetByIdTrackedAsync(id);
        if (room is null)
            return null;

        if (!await IsValidAsync(dto, id))
            return false;

        MapToEntity(dto, room);
        await _repository.UpdateAsync(room);
        return true;
    }

    public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);

    private async Task<bool> IsValidAsync(DormitoryRoomCreateUpdateDto dto, int? excludeId = null)
    {
        if (dto.Capacity <= 0)
            return false;

        if (string.IsNullOrWhiteSpace(dto.RoomNumber))
            return false;

        if (!await _repository.DormExistsAsync(dto.DormId))
            return false;

        // Provera da li soba sa istim brojem vec postoji u tom domu
        if (await _repository.RoomNumberExistsAsync(dto.DormId, dto.RoomNumber, excludeId))
            return false;

        return true;
    }

    public async Task<PagedResultDto<DormitoryRoomGetDto>> GetPagedWithStudentsAsync(int pageNumber, int pageSize)
    {
        var (rooms, totalCount) = await _repository.GetPagedAsync(pageNumber, pageSize);

        var roomIds = rooms.Select(r => r.Id).ToList();
        var dormIds = rooms.Select(r => r.DormId).Distinct().ToList();

        var students = await _repository.GetStudentsByRoomIdsAsync(roomIds);
        var dormNames = await _repository.GetDormNamesByIdsAsync(dormIds);

        var studentsByRoom = students
            .GroupBy(s => s.DormitoryRoomId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var items = rooms.Select(room => new DormitoryRoomGetDto
        {
            Id = room.Id,
            RoomNumber = room.RoomNumber,
            Capacity = room.Capacity,
            CurrentOccupancy = room.CurrentOccupancy,
            AvailableSpots = room.AvailableSpots,
            Gender = room.Gender,
            Status = room.Status,
            DormId = room.DormId,
            DormName = dormNames.TryGetValue(room.DormId, out var name) ? name : null,
            Students = studentsByRoom.TryGetValue(room.Id, out var roomStudents)
                ? roomStudents.Select(s => new StudentSummaryDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Surname = s.Surname,
                    FacultyName = s.FacultyName,
                    IsFreshman = s.IsFreshman
                }).ToList()
                : new List<StudentSummaryDto>()
        }).ToList();

        return new PagedResultDto<DormitoryRoomGetDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }


    private static DormitoryRoom MapToEntity(DormitoryRoomCreateUpdateDto dto, DormitoryRoom? existing = null)
    {
        var room = existing ?? new DormitoryRoom();

        room.RoomNumber = dto.RoomNumber;
        room.Capacity = dto.Capacity;
        room.Gender = dto.Gender;
        room.Status = dto.Status;
        room.DormId = dto.DormId;

        return room;
    }
}
