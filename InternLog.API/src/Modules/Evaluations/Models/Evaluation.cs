using InternLog.API.Modules.Auth.Models;
using InternLog.API.Modules.Admin.Models;
namespace InternLog.API.Modules.Evaluations.Models;
public class Evaluation {
    public int EvaluationID { get; set; }
    public int InternshipID { get; set; }
    public int EvaluatorID { get; set; }
    public string EvalType { get; set; } = string.Empty;
    public decimal? TotalScore { get; set; }
    public string? Comment { get; set; }
    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
    
    public Internship Internship { get; set; } = null!;
    public User Evaluator { get; set; } = null!;
}
