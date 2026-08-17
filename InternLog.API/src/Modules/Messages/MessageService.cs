using Microsoft.EntityFrameworkCore;
using InternLog.API.Modules.Messages.DTOs;
using InternLog.API.Core.Interfaces;
using InternLog.API.Core.Common;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Infrastructure.Data.Models;

namespace InternLog.API.Modules.Messages;

public class MessageService : IMessageService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public MessageService(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<ConversationDto>> GetConversationsAsync()
    {
        var userId = _currentUser.UserId;

        // ดึง MessageID ล่าสุดของแต่ละบทสนทนา
        var lastMessageIds = await _context.Messages
            .Where(m => m.SenderID == userId || m.ReceiverID == userId)
            .GroupBy(m => m.SenderID == userId ? m.ReceiverID : m.SenderID)
            .Select(g => g.OrderByDescending(m => m.CreatedAt).First().MessageID)
            .ToListAsync();

        var lastMessages = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => lastMessageIds.Contains(m.MessageID))
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        var result = new List<ConversationDto>();
        foreach (var msg in lastMessages)
        {
            var otherUserId = msg.SenderID == userId ? msg.ReceiverID : msg.SenderID;
            var otherUser = msg.SenderID == userId ? msg.Receiver : msg.Sender;

            var unreadCount = await _context.Messages
                .CountAsync(m => m.SenderID == otherUserId && m.ReceiverID == userId && !m.IsRead);

            result.Add(new ConversationDto(
                otherUserId,
                $"{otherUser.FirstName} {otherUser.LastName}",
                msg.Content.Length > 60 ? msg.Content[..60] + "..." : msg.Content,
                msg.CreatedAt,
                unreadCount
            ));
        }

        return result;
    }

    public async Task<IEnumerable<MessageResponseDto>> GetMessagesAsync(int otherUserId)
    {
        var userId = _currentUser.UserId;

        var messages = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => (m.SenderID == userId && m.ReceiverID == otherUserId) ||
                        (m.SenderID == otherUserId && m.ReceiverID == userId))
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        // มาร์คข้อความที่รับว่าอ่านแล้วโดยอัตโนมัติ
        var unread = messages.Where(m => m.ReceiverID == userId && !m.IsRead).ToList();
        if (unread.Count > 0)
        {
            unread.ForEach(m => m.IsRead = true);
            await _context.SaveChangesAsync();
        }

        return messages.Select(MapToDto);
    }

    public async Task<MessageResponseDto> SendMessageAsync(SendMessageRequestDto request)
    {
        var senderId = _currentUser.UserId;

        var receiver = await _context.Users.FirstOrDefaultAsync(u => u.UserID == request.ReceiverID);
        if (receiver == null)
            throw AppException.NotFound("ไม่พบผู้รับข้อความ");

        if (senderId == request.ReceiverID)
            throw AppException.BadRequest("ไม่สามารถส่งข้อความหาตัวเองได้");

        var message = new Message
        {
            SenderID = senderId,
            ReceiverID = request.ReceiverID,
            Content = request.Content,
            IsRead = false,
            CreatedAt = DateTime.Now
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        message.Sender = (await _context.Users.FirstOrDefaultAsync(u => u.UserID == senderId))!;
        message.Receiver = receiver;

        return MapToDto(message);
    }

    public async Task MarkConversationAsReadAsync(int otherUserId)
    {
        var userId = _currentUser.UserId;

        var unread = await _context.Messages
            .Where(m => m.SenderID == otherUserId && m.ReceiverID == userId && !m.IsRead)
            .ToListAsync();

        if (unread.Count > 0)
        {
            unread.ForEach(m => m.IsRead = true);
            await _context.SaveChangesAsync();
        }
    }

    private static MessageResponseDto MapToDto(Message m) => new(
        m.MessageID,
        m.SenderID,
        m.Sender != null ? $"{m.Sender.FirstName} {m.Sender.LastName}" : string.Empty,
        m.ReceiverID,
        m.Receiver != null ? $"{m.Receiver.FirstName} {m.Receiver.LastName}" : string.Empty,
        m.Content,
        m.IsRead,
        m.CreatedAt
    );
}
