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

        if (!await IsValidAsync(dto))
            return false;

        MapToEntity(dto, room);
        await _repository.UpdateAsync(room);
        return true;
    }

    public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);

    private async Task<bool> IsValidAsync(DormitoryRoomCreateUpdateDto dto)
    {
        if (dto.Capacity <= 0)
            return false;

        if (dto.CurrentOccupancy < 0)
            return false;

        if (dto.CurrentOccupancy > dto.Capacity)
            return false;

        if (string.IsNullOrWhiteSpace(dto.RoomNumber))
            return false;

        return await _repository.DormExistsAsync(dto.DormId);
    }

    private static DormitoryRoom MapToEntity(DormitoryRoomCreateUpdateDto dto, DormitoryRoom? existing = null)
    {
        var room = existing ?? new DormitoryRoom();

        room.RoomNumber = dto.RoomNumber;
        room.Capacity = dto.Capacity;
        room.CurrentOccupancy = dto.CurrentOccupancy;
        room.Gender = dto.Gender;
        room.Status = dto.Status;
        room.DormId = dto.DormId;

        return room;
    }
}
