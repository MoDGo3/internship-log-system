using InternLog.API.Modules.Evaluations.DTOs;

namespace InternLog.API.Modules.Evaluations;

public interface IEvaluationService
{
    Task<IEnumerable<EvaluationResponseDto>> GetEvaluationsAsync(int internshipId);
    Task<EvaluationResponseDto> GetEvaluationDetailAsync(int evaluationId);
    Task<EvaluationResponseDto> CreateEvaluationAsync(EvaluationRequestDto request);
}
