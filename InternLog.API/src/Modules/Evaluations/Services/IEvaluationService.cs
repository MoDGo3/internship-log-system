using InternLog.API.Modules.Evaluations.DTOs;

namespace InternLog.API.Modules.Evaluations.Services;

public interface IEvaluationService {
    Task<List<EvaluationDto>> GetEvaluationsAsync(int userId, string role);
    Task<EvaluationDto?> CreateEvaluationAsync(int userId, string role, CreateEvaluationRequest request);
}
