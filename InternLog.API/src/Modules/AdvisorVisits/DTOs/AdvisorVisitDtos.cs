using System.ComponentModel.DataAnnotations;

namespace InternLog.API.Modules.AdvisorVisits.DTOs;

public record AdvisorVisitRequestDto(
    [Required] int InternshipID,
    [Required] int PersonNo,
    [Required] int VisitNo,
    [Required] DateOnly VisitDate,
    [Required] string VisitType, // Onsite or Online
    string? Summary,
    int? Score
);

public record AdvisorVisitResponseDto(
    int VisitID,
    int InternshipID,
    int StudentID,
    string StudentName,
    int PersonNo,
    int VisitNo,
    DateOnly VisitDate,
    int AdvisorID,
    string AdvisorName,
    string VisitType,
    string? Summary,
    int? Score,
    DateTime CreatedAt
);
