using System.ComponentModel.DataAnnotations;

namespace InternLog.API.Modules.Tasks.DTOs;

public record TaskRequestDto(
    [Required] int InternshipID,
    [Required][MaxLength(200)] string Title,
    string? Detail,
    DateOnly? DueDate,
    [Required] string Priority // low, medium, high
);

public record TaskStatusUpdateDto(
    [Required] string Status // todo, doing, done
);

public record TaskResponseDto(
    int TaskID,
    int InternshipID,
    int StudentID,
    string StudentName,
    int AssignedBy,
    string AssignerName,
    string Title,
    string? Detail,
    DateOnly? DueDate,
    string Priority,
    string Status,
    DateTime CreatedAt
);
