using System;
using System.Linq;
using System.Threading.Tasks;
using InternLog.API.Core.Common;
using InternLog.API.Core.Interfaces;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Infrastructure.Data.Models;
using InternLog.API.Modules.Messages;
using InternLog.API.Modules.Messages.DTOs;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace InternLog.API.Tests.Modules.Messages;

public class MessageServiceTests
{
    private readonly AppDbContext _db;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly MessageService _messageService;

    public MessageServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _currentUserMock = new Mock<ICurrentUserService>();
        _messageService = new MessageService(_db, _currentUserMock.Object);

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        _db.Roles.Add(new Role { RoleID = 1, RoleName = "student" });
        _db.Roles.Add(new Role { RoleID = 2, RoleName = "advisor" });

        _db.Users.Add(new User { UserID = 1, RoleID = 1, Email = "student@test.com", FirstName = "Test", LastName = "Student", IsActive = true });
        _db.Users.Add(new User { UserID = 2, RoleID = 2, Email = "advisor@test.com", FirstName = "Test", LastName = "Advisor", IsActive = true });

        _db.SaveChanges();
    }

    [Fact]
    public async Task SendMessageAsync_ValidRequest_ReturnsMessage()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(1);

        var req = new SendMessageRequestDto(2, "Hello Advisor");
        var result = await _messageService.SendMessageAsync(req);

        Assert.NotNull(result);
        Assert.Equal("Hello Advisor", result.Content);
        Assert.Equal(2, result.ReceiverID);
        Assert.False(result.IsRead);
    }

    [Fact]
    public async Task SendMessageAsync_ToSelf_ThrowsBadRequest()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(1);

        var req = new SendMessageRequestDto(1, "Hello Myself");

        var ex = await Assert.ThrowsAsync<AppException>(() => _messageService.SendMessageAsync(req));
        Assert.Equal(400, ex.StatusCode);
        Assert.Contains("ไม่สามารถส่งข้อความหาตัวเองได้", ex.Message);
    }

    [Fact]
    public async Task GetMessagesAsync_ValidUser_ReturnsConversation()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(1);

        _db.Messages.Add(new Message { SenderID = 1, ReceiverID = 2, Content = "Hi", IsRead = true, CreatedAt = DateTime.Now.AddMinutes(-10) });
        _db.Messages.Add(new Message { SenderID = 2, ReceiverID = 1, Content = "Hello", IsRead = false, CreatedAt = DateTime.Now.AddMinutes(-5) });
        await _db.SaveChangesAsync();

        var result = await _messageService.GetMessagesAsync(2);

        Assert.Equal(2, result.Count());
        Assert.Equal("Hi", result.First().Content);
        // Getting messages marks unread messages from the other user as read
        var unreadInDb = await _db.Messages.AnyAsync(m => m.ReceiverID == 1 && !m.IsRead);
        Assert.False(unreadInDb);
    }

    [Fact]
    public async Task MarkConversationAsReadAsync_UnreadMessages_MarksAsRead()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(1);

        _db.Messages.Add(new Message { SenderID = 2, ReceiverID = 1, Content = "Msg 1", IsRead = false, CreatedAt = DateTime.Now.AddMinutes(-10) });
        _db.Messages.Add(new Message { SenderID = 2, ReceiverID = 1, Content = "Msg 2", IsRead = false, CreatedAt = DateTime.Now.AddMinutes(-5) });
        await _db.SaveChangesAsync();

        await _messageService.MarkConversationAsReadAsync(2);

        var unreadCount = await _db.Messages.CountAsync(m => m.ReceiverID == 1 && !m.IsRead);
        Assert.Equal(0, unreadCount);
    }
}
