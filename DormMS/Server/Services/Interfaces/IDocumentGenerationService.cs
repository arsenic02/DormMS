namespace DormMS.Server.Services.Interfaces;

using DormMS.Server.DTOs;

public interface IDocumentGenerationService
{
    Task<GenerateDocumentsResultDto> GenerateDocumentsForCompetitionAsync(int competitionId);
}