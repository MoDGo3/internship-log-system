using System.ComponentModel.DataAnnotations;

namespace InternLog.API.Modules.Notifications.DTOs;

public record NotificationResponseDto(
    int NotificationID,
    int UserID,
    string Title,
    string Message,
    string? Link,
    bool IsRead,
    DateTime CreatedAt
);
