public interface ICompetitionService
{
    Task<IEnumerable<CompetitionGetDto>> GetAllAsync();
    Task<CompetitionGetDto?> GetByIdAsync(int id);
    Task<IEnumerable<CompetitionGetDto>> GetOpenCompetitionsAsync();

    Task<CompetitionGetDto?> OpenCompetitionAsync(int id);

    Task<CompetitionGetDto?> ToggleCompetitionStatusAsync(int id); 
    Task<CompetitionGetDto?> CreateAsync(CompetitionCreateDto dto, int userId, string role);
    //Task<CompetitionGetDto?> CreateAsync(CompetitionCreateDto dto, int? adminId = null, int? dormManagerId = null);
    Task<CompetitionGetDto?> UpdateAsync(int id, CompetitionUpdateDto dto);
    Task<bool> DeleteAsync(int id);
    Task<CompetitionApplicationGetDto?> ApplyAsync(int studentId, CompetitionApplicationCreateDto dto);
    Task<IEnumerable<CompetitionApplicationGetDto>> GetApplicationsByCompetitionIdAsync(int competitionId);
    Task<CompetitionApplicationGetDto?> UpdateApplicationAsync(int id, CompetitionApplicationUpdateDto dto);
    Task<bool> DeleteApplicationAsync(int id);
    Task<CompetitionImportResultDto> BulkApplyAsync(BulkApplicationDto dto);
}

public class CompetitionImportResultDto
{
    public int TotalProcessed { get; set; }
    public int SuccessfullyApplied { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();
}
