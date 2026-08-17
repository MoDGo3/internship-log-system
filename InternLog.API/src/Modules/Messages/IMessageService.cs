using InternLog.API.Modules.Messages.DTOs;

namespace InternLog.API.Modules.Messages;

public interface IMessageService
{
    /// <summary>ดึงรายการบทสนทนา (ทุก user ที่เคยคุยด้วย)</summary>
    Task<IEnumerable<ConversationDto>> GetConversationsAsync();

    /// <summary>ดึงประวัติข้อความกับ user คนใดคนหนึ่ง</summary>
    Task<IEnumerable<MessageResponseDto>> GetMessagesAsync(int otherUserId);

    /// <summary>ส่งข้อความ</summary>
    Task<MessageResponseDto> SendMessageAsync(SendMessageRequestDto request);

    /// <summary>มาร์คข้อความที่รับว่าอ่านแล้ว</summary>
    Task MarkConversationAsReadAsync(int otherUserId);
}
