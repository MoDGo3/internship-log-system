using InternLog.API.Modules.AdvisorVisits.DTOs;

namespace InternLog.API.Modules.AdvisorVisits;

public interface IAdvisorVisitService
{
    Task<IEnumerable<AdvisorVisitResponseDto>> GetVisitsAsync(int internshipId);
    Task<AdvisorVisitResponseDto> GetVisitDetailAsync(int visitId);
    Task<AdvisorVisitResponseDto> CreateVisitAsync(AdvisorVisitRequestDto request);
    Task<AdvisorVisitResponseDto> UpdateVisitAsync(int visitId, AdvisorVisitRequestDto request);
}
