using DormMS.Server.Services.Interfaces;
using DormMS.Server.Repositories.Interfaces;

namespace DormMS.Server.Services.Implementations;

public class DormService : IDormService
{
    private readonly IDormRepository _repository;

    public DormService(IDormRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<DormGetDto>> GetAllAsync()
    {
        var dorms = await _repository.GetAllAsync();
        return dorms.Select(MapToGetDto);
    }

    public async Task<DormGetDto?> GetByIdAsync(int id)
    {
        var dorm = await _repository.GetByIdAsync(id);

        if (dorm is null)
            return null;

        return MapToGetDto(dorm);
        //return dorm is null ? null : MapToGetDto(dorm);
    }

    public async Task<Dorm?> CreateAsync(DormCreateUpdateDto dto)
    {
        if (!IsValid(dto))
            return null;

        var dorm = MapToEntity(dto);
        await _repository.AddAsync(dorm);
        return dorm;
    }

    public async Task<bool?> UpdateAsync(int id, DormCreateUpdateDto dto)
    {
        var dorm = await _repository.GetByIdTrackedAsync(id);
        if (dorm is null)
            return null;

        if (!IsValid(dto))
            return false;

        MapToEntity(dto, dorm);
        await _repository.UpdateAsync(dorm);
        return true;
    }

    public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);

    private static bool IsValid(DormCreateUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return false;

        return true;
    }

    private static Dorm MapToEntity(DormCreateUpdateDto dto, Dorm? existing = null)
    {
        var dorm = existing ?? new Dorm();

        dorm.Name = dto.Name;
        dorm.Location = dto.Location;

        return dorm;
    }

    private static DormGetDto MapToGetDto(Dorm dorm) => new()
    {
        Id = dorm.Id,
        Name = dorm.Name,
        Location = dorm.Location,
        Capacity = dorm.Capacity,
        CurrentOccupancy = dorm.CurrentOccupancy,
        AvailableSpots = dorm.AvailableSpots,
        IsFull = dorm.IsFull,
        Rooms = dorm.Rooms.Select(MapRoomToGetDto).ToList()
    };

    private static DormitoryRoomGetDto MapRoomToGetDto(DormitoryRoom room) => new()
    {
        Id = room.Id,
        RoomNumber = room.RoomNumber,
        Capacity = room.Capacity,
        CurrentOccupancy = room.CurrentOccupancy,
        AvailableSpots = room.AvailableSpots,
        //IsOccupied = room.IsOccupied,
        Gender = room.Gender,
        Status = room.Status,
        DormId = room.DormId
    };
}
