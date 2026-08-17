using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace InternLog.API.Modules.Leaves.DTOs;

public record LeaveQuotaDto(
    int LeaveTypeID,
    string TypeName,
    decimal MaxDays,
    decimal UsedDays,
    decimal PendingDays,
    decimal RemainingDays
);

public record LeaveResponseDto(
    int LeaveID,
    int StudentID,
    string StudentName,
    int LeaveTypeID,
    string LeaveTypeName,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal DayCount,
    string Reason,
    string? AttachmentUrl,
    string Status,
    string? ReviewNote,
    DateTime CreatedAt
);

public record LeaveRequestDto(
    [Required] int LeaveTypeID,
    [Required] DateOnly StartDate,
    [Required] DateOnly EndDate,
    [Required][MaxLength(1000)] string Reason
);

public record ReviewLeaveDto(
    [Required] bool IsApproved,
    [MaxLength(500)] string? ReviewNote
);
