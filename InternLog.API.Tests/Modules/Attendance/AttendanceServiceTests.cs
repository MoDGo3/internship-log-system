using System;
using System.Linq;
using System.Threading.Tasks;
using InternLog.API.Core.Common;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Infrastructure.Data.Models;
using InternLog.API.Modules.Attendance;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InternLog.API.Tests.Modules.Attendance;

public class AttendanceServiceTests
{
    private readonly AppDbContext _db;
    private readonly AttendanceService _attendanceService;

    public AttendanceServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _attendanceService = new AttendanceService(_db);

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        var role = new Role { RoleID = 1, RoleName = "student" };
        var roleAdvisor = new Role { RoleID = 2, RoleName = "advisor" };
        _db.Roles.AddRange(role, roleAdvisor);

        var student = new User { UserID = 1, RoleID = 1, Email = "student@test.com", PasswordHash = "hash", FirstName = "Test", LastName = "Student", IsActive = true };
        var advisor = new User { UserID = 2, RoleID = 2, Email = "advisor@test.com", PasswordHash = "hash", FirstName = "Test", LastName = "Advisor", IsActive = true };
        _db.Users.AddRange(student, advisor);

        var company = new Company { CompanyID = 1, CompanyName = "Test Company", WorkStartTime = new TimeOnly(9, 0), WorkEndTime = new TimeOnly(18, 0) };
        _db.Companies.Add(company);

        var internship = new Internship { InternshipID = 1, StudentID = 1, CompanyID = 1, AdvisorID = 2, Status = "active" };
        _db.Internships.Add(internship);

        _db.SaveChanges();
    }

    [Fact]
    public async Task CheckInAsync_ValidData_ReturnsAttendanceDto()
    {
        var result = await _attendanceService.CheckInAsync(1, "Test Note");

        Assert.NotNull(result);
        Assert.Equal(1, result.StudentID);
        Assert.Equal("Test Note", result.Note);
        Assert.NotNull(result.CheckInTime);

        // Status should be evaluated based on the current time and company start time
        // We cannot reliably test 'ontime' vs 'late' here without abstracting DateTime, but we can assert it returns something
        Assert.Contains(result.Status, new[] { "ontime", "late" });
    }

    [Fact]
    public async Task CheckInAsync_DuplicateCheckIn_ThrowsBadRequest()
    {
        await _attendanceService.CheckInAsync(1, "First CheckIn");

        var ex = await Assert.ThrowsAsync<AppException>(() => _attendanceService.CheckInAsync(1, "Second CheckIn"));
        Assert.Equal(400, ex.StatusCode);
        Assert.Equal("คุณได้ลงเวลาเข้างานของวันนี้ไปแล้ว", ex.Message);
    }

    [Fact]
    public async Task CheckInAsync_NoActiveInternship_ThrowsBadRequest()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => _attendanceService.CheckInAsync(999, "Note"));
        Assert.Equal(400, ex.StatusCode);
        Assert.Equal("ไม่พบข้อมูลการฝึกงานที่กำลังดำเนินการ", ex.Message);
    }

    [Fact]
    public async Task CheckOutAsync_Success_CalculatesTotalHours()
    {
        await _attendanceService.CheckInAsync(1, "Morning CheckIn");

        var result = await _attendanceService.CheckOutAsync(1, "Evening CheckOut");

        Assert.NotNull(result);
        Assert.NotNull(result.CheckOutTime);
        Assert.True(result.TotalHours >= 0);
    }

    [Fact]
    public async Task CheckOutAsync_WithoutCheckIn_ThrowsBadRequest()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => _attendanceService.CheckOutAsync(1, "Evening CheckOut"));
        Assert.Equal(400, ex.StatusCode);
        Assert.Equal("กรุณาลงเวลาเข้างานก่อน", ex.Message);
    }

    [Fact]
    public async Task CheckOutAsync_DuplicateCheckOut_ThrowsBadRequest()
    {
        await _attendanceService.CheckInAsync(1, "Morning CheckIn");
        await _attendanceService.CheckOutAsync(1, "Evening CheckOut");

        var ex = await Assert.ThrowsAsync<AppException>(() => _attendanceService.CheckOutAsync(1, "Second CheckOut"));
        Assert.Equal(400, ex.StatusCode);
        Assert.Equal("คุณได้ลงเวลาออกงานของวันนี้ไปแล้ว", ex.Message);
    }

    [Fact]
    public async Task GetHistoryAsync_ValidUserId_ReturnsList()
    {
        await _attendanceService.CheckInAsync(1, "Morning CheckIn");
        await _attendanceService.CheckOutAsync(1, "Evening CheckOut");

        var result = await _attendanceService.GetHistoryAsync(1, null, null);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(1, result.First().StudentID);
    }

    [Fact]
    public async Task GetSummaryAsync_ReturnsSummaryData()
    {
        await _attendanceService.CheckInAsync(1, "Morning CheckIn");
        await _attendanceService.CheckOutAsync(1, "Evening CheckOut");

        var result = await _attendanceService.GetSummaryAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.TotalDays);
        Assert.True(result.TotalHours >= 0);
    }

    [Fact]
    public async Task RequestEditAsync_ValidData_UpdatesStatusToPendingEdit()
    {
        var checkInResult = await _attendanceService.CheckInAsync(1, "Morning CheckIn");

        var dto = new RequestEditDto
        {
            RequestedCheckInTime = new TimeOnly(8, 30),
            RequestedCheckOutTime = new TimeOnly(17, 30),
            Note = "Forgot to checkout"
        };

        await _attendanceService.RequestEditAsync(checkInResult.AttendanceID, 1, dto);

        var updated = await _db.Attendances.FindAsync(checkInResult.AttendanceID);
        Assert.NotNull(updated);
        Assert.Equal("pending_edit", updated.Status);
        Assert.Contains("[Request] Forgot to checkout", updated.Note);
    }

    [Fact]
    public async Task ReviewEditAsync_Approve_UpdatesTimesAndStatus()
    {
        var checkInResult = await _attendanceService.CheckInAsync(1, "Morning CheckIn");

        var reqDto = new RequestEditDto
        {
            RequestedCheckInTime = new TimeOnly(8, 30),
            RequestedCheckOutTime = new TimeOnly(17, 30),
            Note = "Forgot"
        };
        await _attendanceService.RequestEditAsync(checkInResult.AttendanceID, 1, reqDto);

        var reviewDto = new ReviewEditDto { IsApproved = true, Remark = "OK" };
        await _attendanceService.ReviewEditAsync(checkInResult.AttendanceID, 2, reviewDto);

        var updated = await _db.Attendances.FindAsync(checkInResult.AttendanceID);
        Assert.NotNull(updated);
        Assert.Equal("ontime", updated.Status);
        Assert.Equal(new TimeOnly(8, 30), updated.CheckInTime);
        Assert.Equal(new TimeOnly(17, 30), updated.CheckOutTime);
        Assert.Contains("[Reviewer] OK", updated.Note);
    }

    [Fact]
    public async Task ReviewEditAsync_Reject_RevertsStatus()
    {
        var checkInResult = await _attendanceService.CheckInAsync(1, "Morning CheckIn");
        var reqDto = new RequestEditDto
        {
            RequestedCheckInTime = new TimeOnly(8, 30),
            RequestedCheckOutTime = new TimeOnly(17, 30),
            Note = "Forgot"
        };
        await _attendanceService.RequestEditAsync(checkInResult.AttendanceID, 1, reqDto);

        var reviewDto = new ReviewEditDto { IsApproved = false, Remark = "Rejected" };
        await _attendanceService.ReviewEditAsync(checkInResult.AttendanceID, 2, reviewDto);

        var updated = await _db.Attendances.FindAsync(checkInResult.AttendanceID);
        Assert.NotNull(updated);
        Assert.NotEqual("pending_edit", updated.Status); // Should be reverted to 'absent' since no checkout was initially present
        Assert.Contains("[Reviewer] Rejected", updated.Note);
    }
}
