using InternLog.API.Modules.Notifications.DTOs;

namespace InternLog.API.Modules.Notifications;

public interface INotificationService
{
    /// <summary>ดึงการแจ้งเตือนทั้งหมดของผู้ใช้ปัจจุบัน</summary>
    Task<IEnumerable<NotificationResponseDto>> GetNotificationsAsync();

    /// <summary>มาร์ค Notification ว่าอ่านแล้ว</summary>
    Task MarkAsReadAsync(int notificationId);

    /// <summary>มาร์คทุกรายการว่าอ่านแล้ว</summary>
    Task MarkAllAsReadAsync();

    /// <summary>สร้าง Notification ใหม่ (ใช้ภายในระบบ เช่น เมื่ออนุมัติ WorkLog)</summary>
    Task CreateAsync(int userId, string title, string message, string? link = null);
}
