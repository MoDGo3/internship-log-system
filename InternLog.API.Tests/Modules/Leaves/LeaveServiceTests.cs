using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InternLog.API.Core.Common;
using InternLog.API.Core.Interfaces;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Infrastructure.Data.Models;
using InternLog.API.Modules.Leaves;
using InternLog.API.Modules.Leaves.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace InternLog.API.Tests.Modules.Leaves;

public class LeaveServiceTests
{
    private readonly AppDbContext _db;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly LeaveService _leaveService;
    private readonly string _tempPath;

    public LeaveServiceTests()
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

        _leaveService = new LeaveService(_db, _currentUserMock.Object, _envMock.Object);

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        _db.Users.Add(new User { UserID = 1, RoleID = 1, Email = "student@test.com", PasswordHash = "hash", FirstName = "Test", LastName = "Student", IsActive = true });
        _db.Users.Add(new User { UserID = 2, RoleID = 2, Email = "advisor@test.com", PasswordHash = "hash", FirstName = "Test", LastName = "Advisor", IsActive = true });
        
        _db.Companies.Add(new Company { CompanyID = 1, CompanyName = "Test Co" });
        _db.Internships.Add(new Internship { InternshipID = 1, StudentID = 1, CompanyID = 1, AdvisorID = 2, Status = "active" });

        _db.LeaveTypes.Add(new LeaveType { LeaveTypeID = 1, TypeName = "Sick Leave", MaxDays = 30 });
        _db.LeaveTypes.Add(new LeaveType { LeaveTypeID = 2, TypeName = "Personal Leave", MaxDays = 3 });

        _db.SaveChanges();
    }

    [Fact]
    public async Task SubmitLeaveAsync_WithinQuota_ReturnsPendingLeave()
    {
        // Monday to Tuesday = 2 days
        var startDate = DateOnly.FromDateTime(DateTime.Now).AddDays(8);
        while (startDate.DayOfWeek == DayOfWeek.Saturday || startDate.DayOfWeek == DayOfWeek.Sunday)
            startDate = startDate.AddDays(1);
            
        var endDate = startDate.AddDays(1);
        if (endDate.DayOfWeek == DayOfWeek.Saturday) endDate = endDate.AddDays(2);

        var req = new LeaveRequestDto(1, startDate, endDate, "Sick");

        var result = await _leaveService.SubmitLeaveAsync(req, null);

        Assert.NotNull(result);
        Assert.Equal("pending", result.Status);
        Assert.Equal(2, result.DayCount);
    }

    [Fact]
    public async Task SubmitLeaveAsync_ExceedsQuota_ThrowsBadRequest()
    {
        // Require 4 days for Personal Leave (Quota is 3)
        var startDate = DateOnly.FromDateTime(DateTime.Now).AddDays(8);
        while (startDate.DayOfWeek == DayOfWeek.Saturday || startDate.DayOfWeek == DayOfWeek.Sunday)
            startDate = startDate.AddDays(1);
            
        var endDate = startDate.AddDays(5); // This spans weekend possibly, but logic calculates at least 4 weekdays

        var req = new LeaveRequestDto(2, startDate, endDate, "Too long");

        var ex = await Assert.ThrowsAsync<AppException>(() => _leaveService.SubmitLeaveAsync(req, null));
        Assert.Equal(400, ex.StatusCode);
        Assert.Contains("สิทธิ์การPersonal Leaveของคุณเหลือไม่พอ", ex.Message);
    }

    [Fact]
    public async Task SubmitLeaveAsync_PastDate_ThrowsBadRequest()
    {
        var req = new LeaveRequestDto(1, DateOnly.FromDateTime(DateTime.Now).AddDays(-1), DateOnly.FromDateTime(DateTime.Now), "Past");

        var ex = await Assert.ThrowsAsync<AppException>(() => _leaveService.SubmitLeaveAsync(req, null));
        Assert.Equal(400, ex.StatusCode);
        Assert.Equal("ไม่สามารถขอลาย้อนหลังได้", ex.Message);
    }

    [Fact]
    public async Task GetQuotasAsync_ReturnsCorrectQuotas()
    {
        // Add one approved leave of 1 day for type 2
        _db.Leaves.Add(new Leave { StudentID = 1, LeaveTypeID = 2, Status = "approved", DayCount = 1, StartDate = DateOnly.MinValue, EndDate = DateOnly.MinValue, CreatedAt = DateTime.Now });
        await _db.SaveChangesAsync();

        var quotas = await _leaveService.GetQuotasAsync();

        Assert.NotNull(quotas);
        Assert.Equal(2, quotas.Count());
        
        var personalLeave = quotas.First(q => q.LeaveTypeID == 2);
        Assert.Equal(3, personalLeave.MaxDays);
        Assert.Equal(1, personalLeave.UsedDays);
        Assert.Equal(2, personalLeave.RemainingDays);
    }

    [Fact]
    public async Task ReviewLeaveAsync_Approve_UpdatesStatusAndQuota()
    {
        // Setup pending leave
        var leave = new Leave { StudentID = 1, LeaveTypeID = 1, Status = "pending", DayCount = 2, StartDate = DateOnly.MinValue, EndDate = DateOnly.MinValue, CreatedAt = DateTime.Now };
        _db.Leaves.Add(leave);
        await _db.SaveChangesAsync();

        _currentUserMock.Setup(c => c.UserId).Returns(2); // Advisor

        var result = await _leaveService.ReviewLeaveAsync(leave.LeaveID, new ReviewLeaveDto(true, "OK"));

        Assert.Equal("approved", result.Status);
        Assert.Equal("OK", result.ReviewNote);
    }

    [Fact]
    public async Task ReviewLeaveAsync_Reject_UpdatesStatusAndQuotaDoesNotChange()
    {
        var leave = new Leave { StudentID = 1, LeaveTypeID = 1, Status = "pending", DayCount = 2, StartDate = DateOnly.MinValue, EndDate = DateOnly.MinValue, CreatedAt = DateTime.Now };
        _db.Leaves.Add(leave);
        await _db.SaveChangesAsync();

        _currentUserMock.Setup(c => c.UserId).Returns(2); // Advisor

        var result = await _leaveService.ReviewLeaveAsync(leave.LeaveID, new ReviewLeaveDto(false, "No"));

        Assert.Equal("rejected", result.Status);
    }

    [Fact]
    public async Task GetPendingLeavesAsync_ReturnsPendingLeaves()
    {
        _db.Leaves.Add(new Leave { StudentID = 1, LeaveTypeID = 1, Status = "pending", DayCount = 1, StartDate = DateOnly.MinValue, EndDate = DateOnly.MinValue, CreatedAt = DateTime.Now });
        _db.Leaves.Add(new Leave { StudentID = 1, LeaveTypeID = 1, Status = "approved", DayCount = 1, StartDate = DateOnly.MinValue, EndDate = DateOnly.MinValue, CreatedAt = DateTime.Now });
        await _db.SaveChangesAsync();

        _currentUserMock.Setup(c => c.UserId).Returns(2); // Advisor
        var pending = await _leaveService.GetPendingLeavesAsync();

        Assert.Single(pending);
        Assert.Equal("pending", pending.First().Status);
    }

    [Fact]
    public async Task SubmitLeaveAsync_WithAttachment_SavesFile()
    {
        var startDate = DateOnly.FromDateTime(DateTime.Now).AddDays(8);
        while (startDate.DayOfWeek == DayOfWeek.Saturday || startDate.DayOfWeek == DayOfWeek.Sunday)
            startDate = startDate.AddDays(1);
            
        var req = new LeaveRequestDto(1, startDate, startDate, "Sick with note");

        var fileMock = new Mock<IFormFile>();
        var content = "Fake image content";
        var fileName = "note.png";
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

        var result = await _leaveService.SubmitLeaveAsync(req, fileMock.Object);

        Assert.NotNull(result.AttachmentUrl);
        Assert.Contains("note.png", result.AttachmentUrl);
    }
}
