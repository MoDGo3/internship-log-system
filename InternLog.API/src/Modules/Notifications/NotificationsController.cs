using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InternLog.API.Modules.Notifications.DTOs;
using InternLog.API.Core.Common;

namespace InternLog.API.Modules.Notifications;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>ดึงการแจ้งเตือนทั้งหมดของตัวเอง</summary>
    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        var notifications = await _notificationService.GetNotificationsAsync();
        return Ok(ApiResponse<IEnumerable<NotificationResponseDto>>.Ok(notifications));
    }

    /// <summary>มาร์ค Notification รายการหนึ่งว่าอ่านแล้ว</summary>
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return Ok(ApiResponse<string>.Ok("ok", "อ่านแจ้งเตือนแล้ว"));
    }

    /// <summary>มาร์คทุกรายการว่าอ่านแล้ว</summary>
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _notificationService.MarkAllAsReadAsync();
        return Ok(ApiResponse<string>.Ok("ok", "อ่านแจ้งเตือนทั้งหมดแล้ว"));
    }
}
