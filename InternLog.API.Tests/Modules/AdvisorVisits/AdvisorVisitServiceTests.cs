using System;
using System.Linq;
using System.Threading.Tasks;
using InternLog.API.Core.Common;
using InternLog.API.Core.Interfaces;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Infrastructure.Data.Models;
using InternLog.API.Modules.AdvisorVisits;
using InternLog.API.Modules.AdvisorVisits.DTOs;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace InternLog.API.Tests.Modules.AdvisorVisits;

public class AdvisorVisitServiceTests
{
    private readonly AppDbContext _db;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly AdvisorVisitService _advisorVisitService;

    public AdvisorVisitServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _currentUserMock = new Mock<ICurrentUserService>();
        _advisorVisitService = new AdvisorVisitService(_db, _currentUserMock.Object);

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        _db.Roles.Add(new Role { RoleID = 1, RoleName = "student" });
        _db.Roles.Add(new Role { RoleID = 2, RoleName = "advisor" });

        _db.Users.Add(new User { UserID = 1, RoleID = 1, Email = "student@test.com", FirstName = "Test", LastName = "Student", IsActive = true });
        _db.Users.Add(new User { UserID = 2, RoleID = 2, Email = "advisor@test.com", FirstName = "Test", LastName = "Advisor", IsActive = true });

        _db.Internships.Add(new Internship
        {
            InternshipID = 1,
            StudentID = 1,
            AdvisorID = 2,
            Status = "in_progress"
        });

        _db.SaveChanges();
    }

    [Fact]
    public async Task CreateVisitAsync_AdvisorRecordsVisit_ReturnsVisit()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(2); // Advisor

        var req = new AdvisorVisitRequestDto(1, 1, 1, DateOnly.FromDateTime(DateTime.Now), "Online", "Good progress", 85);
        var result = await _advisorVisitService.CreateVisitAsync(req);

        Assert.NotNull(result);
        Assert.Equal(1, result.VisitNo);
        Assert.Equal("Online", result.VisitType);
        Assert.Equal(85, result.Score);
    }

    [Fact]
    public async Task CreateVisitAsync_DuplicateVisitNo_ThrowsBadRequest()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(2); // Advisor

        var visit = new AdvisorVisit { InternshipID = 1, PersonNo = 1, VisitNo = 1, VisitDate = DateOnly.FromDateTime(DateTime.Now), AdvisorID = 2, VisitType = "Online" };
        _db.AdvisorVisits.Add(visit);
        await _db.SaveChangesAsync();

        var req = new AdvisorVisitRequestDto(1, 1, 1, DateOnly.FromDateTime(DateTime.Now), "Onsite", null, null);

        var ex = await Assert.ThrowsAsync<AppException>(() => _advisorVisitService.CreateVisitAsync(req));
        Assert.Equal(400, ex.StatusCode);
        Assert.Contains("มีการบันทึกการนิเทศครั้งที่ 1", ex.Message);
    }

    [Fact]
    public async Task UpdateVisitAsync_AdvisorUpdatesVisit_UpdatesData()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(2); // Advisor

        var visit = new AdvisorVisit { InternshipID = 1, PersonNo = 1, VisitNo = 1, VisitDate = DateOnly.FromDateTime(DateTime.Now), AdvisorID = 2, VisitType = "Online" };
        _db.AdvisorVisits.Add(visit);
        await _db.SaveChangesAsync();

        var req = new AdvisorVisitRequestDto(1, 1, 1, DateOnly.FromDateTime(DateTime.Now), "Onsite", "Updated summary", 90);
        var result = await _advisorVisitService.UpdateVisitAsync(visit.VisitID, req);

        Assert.Equal("Onsite", result.VisitType);
        Assert.Equal("Updated summary", result.Summary);
        Assert.Equal(90, result.Score);
    }

    [Fact]
    public async Task GetVisitsAsync_ValidInternship_ReturnsVisits()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(2); // Advisor
        _currentUserMock.Setup(c => c.Role).Returns("advisor");

        _db.AdvisorVisits.Add(new AdvisorVisit { InternshipID = 1, PersonNo = 1, VisitNo = 1, VisitDate = DateOnly.FromDateTime(DateTime.Now), AdvisorID = 2, VisitType = "Online" });
        _db.AdvisorVisits.Add(new AdvisorVisit { InternshipID = 1, PersonNo = 1, VisitNo = 2, VisitDate = DateOnly.FromDateTime(DateTime.Now.AddDays(7)), AdvisorID = 2, VisitType = "Onsite" });
        await _db.SaveChangesAsync();

        var result = await _advisorVisitService.GetVisitsAsync(1);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task CreateVisitAsync_StudentRecordsVisit_ThrowsForbidden()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(1); // Student (ID 1 is not the Advisor of Internship 1)

        var req = new AdvisorVisitRequestDto(1, 1, 1, DateOnly.FromDateTime(DateTime.Now), "Online", null, null);

        var ex = await Assert.ThrowsAsync<AppException>(() => _advisorVisitService.CreateVisitAsync(req));
        Assert.Equal(403, ex.StatusCode);
        Assert.Contains("คุณไม่ใช่ที่ปรึกษาของการฝึกงานนี้", ex.Message);
    }
}
