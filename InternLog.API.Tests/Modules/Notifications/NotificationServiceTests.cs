using System;
using System.Linq;
using System.Threading.Tasks;
using InternLog.API.Core.Common;
using InternLog.API.Core.Interfaces;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Infrastructure.Data.Models;
using InternLog.API.Modules.Notifications;
using InternLog.API.Modules.Notifications.DTOs;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace InternLog.API.Tests.Modules.Notifications;

public class NotificationServiceTests
{
    private readonly AppDbContext _db;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly NotificationService _notificationService;

    public NotificationServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _currentUserMock = new Mock<ICurrentUserService>();
        _notificationService = new NotificationService(_db, _currentUserMock.Object);

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        _db.Users.Add(new User { UserID = 1, RoleID = 1, Email = "student@test.com", FirstName = "Test", LastName = "Student", IsActive = true });
        _db.SaveChanges();
    }

    [Fact]
    public async Task GetNotificationsAsync_ValidUser_ReturnsNotifications()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(1);

        _db.Notifications.Add(new Notification { UserID = 1, Title = "Alert", Message = "Msg 1", IsRead = false, CreatedAt = DateTime.Now });
        _db.Notifications.Add(new Notification { UserID = 1, Title = "Alert", Message = "Msg 2", IsRead = true, CreatedAt = DateTime.Now.AddMinutes(-5) });
        await _db.SaveChangesAsync();

        var result = await _notificationService.GetNotificationsAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task MarkAsReadAsync_ValidNotification_MarksAsRead()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(1);

        var notif = new Notification { UserID = 1, Title = "Alert", Message = "Msg", IsRead = false, CreatedAt = DateTime.Now };
        _db.Notifications.Add(notif);
        await _db.SaveChangesAsync();

        await _notificationService.MarkAsReadAsync(notif.NotificationID);

        var updatedNotif = await _db.Notifications.FindAsync(notif.NotificationID);
        Assert.True(updatedNotif.IsRead);
    }

    [Fact]
    public async Task MarkAsReadAsync_InvalidNotification_ThrowsNotFound()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(1);

        var ex = await Assert.ThrowsAsync<AppException>(() => _notificationService.MarkAsReadAsync(999));
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task MarkAllAsReadAsync_UnreadNotifications_MarksAllAsRead()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(1);

        _db.Notifications.Add(new Notification { UserID = 1, Title = "Alert", Message = "Msg 1", IsRead = false, CreatedAt = DateTime.Now });
        _db.Notifications.Add(new Notification { UserID = 1, Title = "Alert", Message = "Msg 2", IsRead = false, CreatedAt = DateTime.Now });
        await _db.SaveChangesAsync();

        await _notificationService.MarkAllAsReadAsync();

        var unreadCount = await _db.Notifications.CountAsync(n => n.UserID == 1 && !n.IsRead);
        Assert.Equal(0, unreadCount);
    }
}
