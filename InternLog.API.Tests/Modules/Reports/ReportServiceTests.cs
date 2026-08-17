using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InternLog.API.Core.Common;
using InternLog.API.Core.Interfaces;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Infrastructure.Data.Models;
using InternLog.API.Modules.Reports;
using InternLog.API.Modules.Reports.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace InternLog.API.Tests.Modules.Reports;

public class ReportServiceTests
{
    private readonly AppDbContext _db;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly ReportService _reportService;
    private readonly string _tempPath;

    public ReportServiceTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);

        _currentUserMock = new Mock<ICurrentUserService>();
        _envMock = new Mock<IWebHostEnvironment>();
        
        _tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempPath);
        _envMock.Setup(e => e.WebRootPath).Returns(_tempPath);

        _reportService = new ReportService(_db, _currentUserMock.Object, _envMock.Object);

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
    public async Task GenerateReportAsync_StudentGeneratesReport_ReturnsDraft()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(1); // Student
        _currentUserMock.Setup(c => c.Role).Returns("student");

        var req = new ReportGenerateRequestDto(1, "Week 1", DateOnly.FromDateTime(DateTime.Now.AddDays(-7)), DateOnly.FromDateTime(DateTime.Now));
        var result = await _reportService.GenerateReportAsync(req);

        Assert.NotNull(result);
        Assert.Equal("draft", result.Status);
        Assert.Equal("Week 1", result.PeriodLabel);
    }

    [Fact]
    public async Task SubmitReportAsync_DraftReport_ChangesStatusToSubmitted()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(1); // Student

        var report = new Report { StudentID = 1, InternshipID = 1, PeriodLabel = "Week 1", Status = "draft" };
        _db.Reports.Add(report);
        await _db.SaveChangesAsync();

        var result = await _reportService.SubmitReportAsync(report.ReportID);

        Assert.Equal("submitted", result.Status);
    }

    [Fact]
    public async Task ReviewReportAsync_AdvisorApproves_ChangesStatusToApproved()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(2); // Advisor

        var report = new Report { StudentID = 1, InternshipID = 1, PeriodLabel = "Week 1", Status = "submitted" };
        _db.Reports.Add(report);
        await _db.SaveChangesAsync();

        var req = new ReportReviewRequestDto("approved", "Excellent report");
        var result = await _reportService.ReviewReportAsync(report.ReportID, req);

        Assert.Equal("approved", result.Status);
        Assert.Equal("Excellent report", result.ReviewNote);
    }

    [Fact]
    public async Task ExportPdfAsync_ValidReport_ReturnsPdfFileUrl()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(1); // Student
        _currentUserMock.Setup(c => c.Role).Returns("student");

        var report = new Report { StudentID = 1, InternshipID = 1, PeriodLabel = "Week 1", Status = "approved" };
        _db.Reports.Add(report);
        await _db.SaveChangesAsync();

        var resultUrl = await _reportService.ExportPdfAsync(report.ReportID);

        Assert.NotNull(resultUrl);
        Assert.Contains(".pdf", resultUrl);
    }

    [Fact]
    public async Task GetReportsAsync_ValidInternship_ReturnsReports()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(1); // Student
        _currentUserMock.Setup(c => c.Role).Returns("student");

        _db.Reports.Add(new Report { StudentID = 1, InternshipID = 1, PeriodLabel = "Week 1", Status = "draft" });
        _db.Reports.Add(new Report { StudentID = 1, InternshipID = 1, PeriodLabel = "Week 2", Status = "approved" });
        await _db.SaveChangesAsync();

        var result = await _reportService.GetReportsAsync(1);

        Assert.Equal(2, result.Count());
    }
}
