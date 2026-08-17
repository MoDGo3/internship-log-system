using System.ComponentModel.DataAnnotations;

namespace InternLog.API.Modules.Reports.DTOs;

public record ReportGenerateRequestDto(
    [Required] int InternshipID,
    [Required] string PeriodLabel,
    [Required] DateOnly StartDate,
    [Required] DateOnly EndDate
);

public record ReportReviewRequestDto(
    [Required] string Status, // "approved" or "returned"
    string? ReviewNote
);

public record ReportResponseDto(
    int ReportID,
    int InternshipID,
    int StudentID,
    string StudentName,
    string PeriodLabel,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    string? ReviewNote,
    string? FilePath,
    DateTime CreatedAt
);
