using System.ComponentModel.DataAnnotations;

namespace InternLog.API.Modules.Evaluations.DTOs;

public record EvaluationRequestDto(
    [Required] int InternshipID,
    [Required] string EvaluatorRole,  // "supervisor_eval" or "student_eval"
    string? CriteriaJson,             // JSON array ของ criteria + scores
    decimal? TotalScore,
    string? Grade,                    // A|B|C|D|F
    string? Comment
);

public record EvaluationResponseDto(
    int EvaluationID,
    int InternshipID,
    int StudentID,
    string StudentName,
    string EvaluatorRole,
    string? CriteriaJson,
    decimal? TotalScore,
    string? Grade,
    string? Comment,
    DateTime CreatedAt
);
