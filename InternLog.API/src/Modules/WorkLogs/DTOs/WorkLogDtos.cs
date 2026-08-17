using System.ComponentModel.DataAnnotations;

namespace InternLog.API.Modules.WorkLogs.DTOs;

public record WorkLogRequestDto(
    [Required] int CategoryID,
    [Required] DateOnly LogDate,
    [Required][MaxLength(200)] string Title,
    string? Detail,
    string? Problem,
    string? Solution
);

public record WorkLogResponseDto(
    int LogID,
    int StudentID,
    string StudentName,
    int CategoryID,
    string CategoryName,
    DateOnly LogDate,
    decimal? Hours,
    string Title,
    string? Detail,
    string? Problem,
    string? Solution,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<AttachmentResponseDto> Attachments,
    List<CommentResponseDto> Comments
);

public record AttachmentResponseDto(
    int AttachmentID,
    string FileName,
    string FileUrl,
    long FileSize,
    DateTime UploadedAt
);

public record CommentRequestDto(
    [Required] string Message
);

public record CommentResponseDto(
    int CommentID,
    int UserID,
    string UserName,
    string Role,
    string Message,
    DateTime CreatedAt
);

public record ReviewLogDto(
    [Required] bool IsApproved,
    string? Comment
);
