public class DormService : IDormService
{
    private readonly IDormRepository _repository;

    public DormService(IDormRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Dorm>> GetAllAsync() => _repository.GetAllAsync();

    public Task<Dorm?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

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
        if (dto.Capacity <= 0)
            return false;

        if (dto.CurrentOccupancy < 0)
            return false;

        if (dto.CurrentOccupancy > dto.Capacity)
            return false;

        return true;
    }

    private static Dorm MapToEntity(DormCreateUpdateDto dto, Dorm? existing = null)
    {
        var dorm = existing ?? new Dorm();

        dorm.Name = dto.Name;
        dorm.Location = dto.Location;
        dorm.Capacity = dto.Capacity;
        dorm.CurrentOccupancy = dto.CurrentOccupancy;

        return dorm;
    }
}
//potrebno je ubaciti mapiranje za DormGetDto,