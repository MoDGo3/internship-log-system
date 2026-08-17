using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InternLog.API.Core.Common;
using InternLog.API.Core.Interfaces;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Infrastructure.Data.Models;
using InternLog.API.Modules.WorkLogs;
using InternLog.API.Modules.WorkLogs.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace InternLog.API.Tests.Modules.WorkLogs;

public class WorkLogServiceTests
{
    private readonly AppDbContext _db;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly WorkLogService _workLogService;
    private readonly string _tempPath;

    public WorkLogServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);

        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(c => c.UserId).Returns(1); // Default to student ID 1

        _envMock = new Mock<IWebHostEnvironment>();
        _tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempPath);
        _envMock.Setup(e => e.WebRootPath).Returns(_tempPath);

        _workLogService = new WorkLogService(_db, _currentUserMock.Object, _envMock.Object);

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        _db.Roles.Add(new Role { RoleID = 1, RoleName = "student" });
        _db.Roles.Add(new Role { RoleID = 2, RoleName = "advisor" });

        _db.Users.Add(new User { UserID = 1, RoleID = 1, Email = "student@test.com", PasswordHash = "hash", FirstName = "Test", LastName = "Student", IsActive = true });
        _db.Users.Add(new User { UserID = 2, RoleID = 2, Email = "advisor@test.com", PasswordHash = "hash", FirstName = "Test", LastName = "Advisor", IsActive = true });
        
        _db.WorkCategories.Add(new WorkCategory { CategoryID = 1, CategoryName = "Programming" });

        _db.SaveChanges();
    }

    [Fact]
    public async Task CreateLogAsync_Draft_ReturnsDraftLog()
    {
        var req = new WorkLogRequestDto(1, DateOnly.FromDateTime(DateTime.Now), "Fixed bugs", "Fixed 5 bugs", null, null);
        var result = await _workLogService.CreateLogAsync(req, isSubmit: false);

        Assert.NotNull(result);
        Assert.Equal("draft", result.Status);
        Assert.Equal("Fixed bugs", result.Title);
    }

    [Fact]
    public async Task SubmitLogAsync_DraftLog_ChangesStatusToSubmitted()
    {
        var log = new WorkLog { StudentID = 1, CategoryID = 1, LogDate = DateOnly.FromDateTime(DateTime.Now), Title = "To Submit", Status = "draft" };
        _db.WorkLogs.Add(log);
        await _db.SaveChangesAsync();

        var result = await _workLogService.SubmitLogAsync(log.LogID);

        Assert.Equal("submitted", result.Status);
    }

    [Fact]
    public async Task ReviewLogAsync_Approve_ChangesStatusToApproved()
    {
        var log = new WorkLog { StudentID = 1, CategoryID = 1, LogDate = DateOnly.FromDateTime(DateTime.Now), Title = "To Approve", Status = "submitted" };
        _db.WorkLogs.Add(log);
        await _db.SaveChangesAsync();

        _currentUserMock.Setup(c => c.UserId).Returns(2); // Advisor

        var result = await _workLogService.ReviewLogAsync(log.LogID, new ReviewLogDto(true, null));

        Assert.Equal("approved", result.Status);
    }

    [Fact]
    public async Task ReviewLogAsync_Return_ChangesStatusToReturnedAndAddsComment()
    {
        var log = new WorkLog { StudentID = 1, CategoryID = 1, LogDate = DateOnly.FromDateTime(DateTime.Now), Title = "To Return", Status = "submitted" };
        _db.WorkLogs.Add(log);
        await _db.SaveChangesAsync();

        _currentUserMock.Setup(c => c.UserId).Returns(2); // Advisor

        var result = await _workLogService.ReviewLogAsync(log.LogID, new ReviewLogDto(false, "Needs more details"));

        Assert.Equal("returned", result.Status);
        Assert.Single(result.Comments);
        Assert.Equal("Needs more details", result.Comments.First().Message);
    }

    [Fact]
    public async Task UpdateLogAsync_DraftLog_UpdatesData()
    {
        var log = new WorkLog { StudentID = 1, CategoryID = 1, LogDate = DateOnly.FromDateTime(DateTime.Now), Title = "Old Title", Status = "draft" };
        _db.WorkLogs.Add(log);
        await _db.SaveChangesAsync();

        var req = new WorkLogRequestDto(1, DateOnly.FromDateTime(DateTime.Now), "New Title", "New details", null, null);
        var result = await _workLogService.UpdateLogAsync(log.LogID, req);

        Assert.Equal("New Title", result.Title);
        Assert.Equal("New details", result.Detail);
    }

    [Fact]
    public async Task UpdateLogAsync_SubmittedLog_ThrowsBadRequest()
    {
        var log = new WorkLog { StudentID = 1, CategoryID = 1, LogDate = DateOnly.FromDateTime(DateTime.Now), Title = "Submitted Log", Status = "submitted" };
        _db.WorkLogs.Add(log);
        await _db.SaveChangesAsync();

        var req = new WorkLogRequestDto(1, DateOnly.FromDateTime(DateTime.Now), "New Title", null, null, null);

        var ex = await Assert.ThrowsAsync<AppException>(() => _workLogService.UpdateLogAsync(log.LogID, req));
        Assert.Equal(400, ex.StatusCode);
        Assert.Contains("ไม่สามารถแก้ไขได้", ex.Message);
    }

    [Fact]
    public async Task UploadAttachmentAsync_DraftLog_ReturnsAttachmentResponse()
    {
        var log = new WorkLog { StudentID = 1, CategoryID = 1, LogDate = DateOnly.FromDateTime(DateTime.Now), Title = "With File", Status = "draft" };
        _db.WorkLogs.Add(log);
        await _db.SaveChangesAsync();

        var fileMock = new Mock<IFormFile>();
        var content = "Fake image content";
        var fileName = "screenshot.png";
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write(content);
        writer.Flush();
        ms.Position = 0;
        
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(ms.Length);
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default)).Callback<Stream, System.Threading.CancellationToken>((stream, token) =>
        {
            ms.CopyTo(stream);
        }).Returns(Task.CompletedTask);

        var result = await _workLogService.UploadAttachmentAsync(log.LogID, fileMock.Object);

        Assert.NotNull(result);
        Assert.Contains("screenshot.png", result.FileName);
    }

    [Fact]
    public async Task AddCommentAsync_ValidLog_ReturnsCommentResponse()
    {
        var log = new WorkLog { StudentID = 1, CategoryID = 1, LogDate = DateOnly.FromDateTime(DateTime.Now), Title = "Log", Status = "submitted" };
        _db.WorkLogs.Add(log);
        await _db.SaveChangesAsync();

        _currentUserMock.Setup(c => c.UserId).Returns(2); // Advisor

        var result = await _workLogService.AddCommentAsync(log.LogID, new CommentRequestDto("Good job"));

        Assert.NotNull(result);
        Assert.Equal("Good job", result.Message);
    }

    [Fact]
    public async Task GetStudentLogsAsync_ReturnsLogs()
    {
        _db.WorkLogs.Add(new WorkLog { StudentID = 1, CategoryID = 1, LogDate = DateOnly.FromDateTime(DateTime.Now), Title = "Log 1", Status = "draft" });
        _db.WorkLogs.Add(new WorkLog { StudentID = 1, CategoryID = 1, LogDate = DateOnly.FromDateTime(DateTime.Now).AddDays(-1), Title = "Log 2", Status = "approved" });
        await _db.SaveChangesAsync();

        var logs = await _workLogService.GetStudentLogsAsync();

        Assert.Equal(2, logs.Count());
    }

    [Fact]
    public async Task CreateLogAsync_WithAttendance_ComputesHours()
    {
        var logDate = DateOnly.FromDateTime(DateTime.Now);
        _db.Attendances.Add(new InternLog.API.Infrastructure.Data.Models.Attendance { StudentID = 1, AttDate = logDate, Status = "ontime", TotalHours = 8.5m });
        await _db.SaveChangesAsync();

        var req = new WorkLogRequestDto(1, logDate, "Auto Hours", null, null, null);
        var result = await _workLogService.CreateLogAsync(req, isSubmit: false);

        Assert.Equal(8.5m, result.Hours);
    }
}
