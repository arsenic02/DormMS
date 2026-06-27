using DormMS.Server.Services.Interfaces;
using DormMS.Server.Repositories.Interfaces;
using DormMS.Server.Models;
using DormMS.Server.DTOs;

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

    public async Task GenerateRoomsAsync(GenerateRoomsDto dto)
    {
        if (dto.DoubleRooms < 0 || dto.TripleRooms < 0)
            throw new ArgumentException("Broj soba ne može biti negativan");

        if (dto.MalePercentage < 0 || dto.MalePercentage > 100)
            throw new ArgumentException("MalePercentage mora biti izmedju 0 i 100");

        var dorm = await _repository.GetByIdWithRoomsAsync(dto.DormId);

        if (dorm == null)
            throw new Exception("Dom nije pronađen");

        var rooms = new List<DormitoryRoom>();
        int counter = dorm.Rooms.Count + 1;

        // Dvokrevetne sobe - podela po polu
        int doubleMaleCount = (int)Math.Round(dto.DoubleRooms * dto.MalePercentage / 100.0);
        int doubleFemaleCount = dto.DoubleRooms - doubleMaleCount;

        for (int i = 0; i < doubleMaleCount; i++)
        {
            rooms.Add(new DormitoryRoom
            {
                RoomNumber = $"{counter++:000}",
                Capacity = 2,
                CurrentOccupancy = 0,
                Gender = GenderType.Male,
                Status = RoomStatus.Available,
                DormId = dorm.Id
            });
        }

        for (int i = 0; i < doubleFemaleCount; i++)
        {
            rooms.Add(new DormitoryRoom
            {
                RoomNumber = $"{counter++:000}",
                Capacity = 2,
                CurrentOccupancy = 0,
                Gender = GenderType.Female,
                Status = RoomStatus.Available,
                DormId = dorm.Id
            });
        }

        // Trokrevetne sobe - podela po polu (isti procenat, racuna se odvojeno)
        int tripleMaleCount = (int)Math.Round(dto.TripleRooms * dto.MalePercentage / 100.0);
        int tripleFemaleCount = dto.TripleRooms - tripleMaleCount;

        for (int i = 0; i < tripleMaleCount; i++)
        {
            rooms.Add(new DormitoryRoom
            {
                RoomNumber = $"{counter++:000}",
                Capacity = 3,
                CurrentOccupancy = 0,
                Gender = GenderType.Male,
                Status = RoomStatus.Available,
                DormId = dorm.Id
            });
        }

        for (int i = 0; i < tripleFemaleCount; i++)
        {
            rooms.Add(new DormitoryRoom
            {
                RoomNumber = $"{counter++:000}",
                Capacity = 3,
                CurrentOccupancy = 0,
                Gender = GenderType.Female,
                Status = RoomStatus.Available,
                DormId = dorm.Id
            });
        }

        await _repository.AddRoomsAsync(rooms);
    }

    public async Task<int> DeleteAllRoomsForDormAsync(int dormId)
    {
        var dorm = await _repository.GetByIdWithRoomsAsync(dormId);

        if (dorm == null)
            throw new Exception("Dom nije pronađen");

        var roomIds = dorm.Rooms.Select(r => r.Id).ToList();

        if (roomIds.Count == 0)
            return 0;

        await _repository.ClearStudentRoomAssignmentsAsync(roomIds);
        var deletedCount = await _repository.DeleteRoomsByIdsAsync(roomIds);

        return deletedCount;
    }

    private static Dorm MapToEntity(DormCreateUpdateDto dto, Dorm? existing = null)
    {
        var dorm = existing ?? new Dorm();

        dorm.Name = dto.Name;
        dorm.Location = dto.Location;
        dorm.Municipality=dto.Municipality;

        return dorm;
    }

    private static DormGetDto MapToGetDto(Dorm dorm) => new()
    {
        Id = dorm.Id,
        Name = dorm.Name,
        Location = dorm.Location,
        Municipality = dorm.Municipality,
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
