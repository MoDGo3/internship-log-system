using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InternLog.API.Modules.Messages.DTOs;
using InternLog.API.Core.Common;

namespace InternLog.API.Modules.Messages;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    /// <summary>ดึงรายการบทสนทนาทั้งหมดของตัวเอง</summary>
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        var conversations = await _messageService.GetConversationsAsync();
        return Ok(ApiResponse<IEnumerable<ConversationDto>>.Ok(conversations));
    }

    /// <summary>ดึงประวัติข้อความกับ user คนใดคนหนึ่ง</summary>
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetMessages(int userId)
    {
        var messages = await _messageService.GetMessagesAsync(userId);
        return Ok(ApiResponse<IEnumerable<MessageResponseDto>>.Ok(messages));
    }

    /// <summary>ส่งข้อความ</summary>
    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequestDto request)
    {
        var result = await _messageService.SendMessageAsync(request);
        return Ok(ApiResponse<MessageResponseDto>.Ok(result, "ส่งข้อความเรียบร้อย"));
    }

    /// <summary>มาร์คทุกข้อความในบทสนทนากับ user นี้ว่าอ่านแล้ว</summary>
    [HttpPut("{userId}/read")]
    public async Task<IActionResult> MarkAsRead(int userId)
    {
        await _messageService.MarkConversationAsReadAsync(userId);
        return Ok(ApiResponse<string>.Ok("ok", "อ่านข้อความแล้ว"));
    }
}
