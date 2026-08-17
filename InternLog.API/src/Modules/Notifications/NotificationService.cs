using Microsoft.EntityFrameworkCore;
using InternLog.API.Modules.Notifications.DTOs;
using InternLog.API.Core.Interfaces;
using InternLog.API.Core.Common;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Infrastructure.Data.Models;

namespace InternLog.API.Modules.Notifications;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public NotificationService(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<NotificationResponseDto>> GetNotificationsAsync()
    {
        var userId = _currentUser.UserId;

        var notifications = await _context.Notifications
            .Where(n => n.UserID == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return notifications.Select(MapToDto);
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        var userId = _currentUser.UserId;

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationID == notificationId && n.UserID == userId);

        if (notification == null)
            throw AppException.NotFound("ไม่พบการแจ้งเตือนนี้");

        notification.IsRead = true;
        await _context.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync()
    {
        var userId = _currentUser.UserId;

        var unread = await _context.Notifications
            .Where(n => n.UserID == userId && !n.IsRead)
            .ToListAsync();

        if (unread.Count > 0)
        {
            unread.ForEach(n => n.IsRead = true);
            await _context.SaveChangesAsync();
        }
    }

    public async Task CreateAsync(int userId, string title, string message, string? link = null)
    {
        var notification = new Notification
        {
            UserID = userId,
            Title = title,
            Message = message,
            Link = link,
            IsRead = false,
            CreatedAt = DateTime.Now
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    private static NotificationResponseDto MapToDto(Notification n) => new(
        n.NotificationID,
        n.UserID,
        n.Title,
        n.Message,
        n.Link,
        n.IsRead,
        n.CreatedAt
    );
}
