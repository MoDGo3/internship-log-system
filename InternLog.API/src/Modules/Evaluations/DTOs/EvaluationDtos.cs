namespace InternLog.API.Modules.Evaluations.DTOs;

public class EvaluationDto {
    public int EvaluationID { get; set; }
    public int InternshipID { get; set; }
    public int EvaluatorID { get; set; }
    public string EvalType { get; set; } = string.Empty;
    public decimal? TotalScore { get; set; }
    public string? Comment { get; set; }
    public DateTime EvaluatedAt { get; set; }
}

public class CreateEvaluationRequest {
    public int InternshipID { get; set; }
    public decimal? TotalScore { get; set; }
    public string? Comment { get; set; }
}
