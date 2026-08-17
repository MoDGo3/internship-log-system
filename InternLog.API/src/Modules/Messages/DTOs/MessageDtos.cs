using System.ComponentModel.DataAnnotations;

namespace InternLog.API.Modules.Messages.DTOs;

public record SendMessageRequestDto(
    [Required] int ReceiverID,
    [Required] string Content
);

public record MessageResponseDto(
    int MessageID,
    int SenderID,
    string SenderName,
    int ReceiverID,
    string ReceiverName,
    string Content,
    bool IsRead,
    DateTime CreatedAt
);

public record ConversationDto(
    int OtherUserID,
    string OtherUserName,
    string LastMessage,
    DateTime LastMessageAt,
    int UnreadCount
);
