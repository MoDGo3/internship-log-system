using System;
using System.Linq;
using System.Threading.Tasks;
using InternLog.API.Core.Common;
using InternLog.API.Core.Interfaces;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Infrastructure.Data.Models;
using InternLog.API.Modules.Evaluations;
using InternLog.API.Modules.Evaluations.DTOs;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace InternLog.API.Tests.Modules.Evaluations;

public class EvaluationServiceTests
{
    private readonly AppDbContext _db;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly EvaluationService _evaluationService;

    public EvaluationServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _currentUserMock = new Mock<ICurrentUserService>();
        _evaluationService = new EvaluationService(_db, _currentUserMock.Object);

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        _db.Roles.Add(new Role { RoleID = 1, RoleName = "student" });
        _db.Roles.Add(new Role { RoleID = 3, RoleName = "supervisor" });

        _db.Users.Add(new User { UserID = 1, RoleID = 1, Email = "student@test.com", FirstName = "Test", LastName = "Student", IsActive = true });
        _db.Users.Add(new User { UserID = 2, RoleID = 3, Email = "supervisor@test.com", FirstName = "Test", LastName = "Supervisor", IsActive = true });

        _db.Internships.Add(new Internship
        {
            InternshipID = 1,
            StudentID = 1,
            SupervisorID = 2,
            Status = "in_progress"
        });

        _db.SaveChanges();
    }

    [Fact]
    public async Task CreateEvaluationAsync_SupervisorEvaluatesStudent_ReturnsEvaluation()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(2); // Supervisor
        _currentUserMock.Setup(c => c.Role).Returns("supervisor");

        var req = new EvaluationRequestDto(1, "supervisor_eval", null, 95.5m, "A", "Great job");
        var result = await _evaluationService.CreateEvaluationAsync(req);

        Assert.NotNull(result);
        Assert.Equal("supervisor_eval", result.EvaluatorRole);
        Assert.Equal(95.5m, result.TotalScore);
        Assert.Equal("A", result.Grade);
    }

    [Fact]
    public async Task CreateEvaluationAsync_StudentEvaluatesCompany_ReturnsEvaluation()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(1); // Student
        _currentUserMock.Setup(c => c.Role).Returns("student");

        var req = new EvaluationRequestDto(1, "student_eval", null, 80m, "B", "Good company");
        var result = await _evaluationService.CreateEvaluationAsync(req);

        Assert.NotNull(result);
        Assert.Equal("student_eval", result.EvaluatorRole);
        Assert.Equal(80m, result.TotalScore);
    }

    [Fact]
    public async Task CreateEvaluationAsync_DuplicateEvaluation_ThrowsBadRequest()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(1); // Student
        _currentUserMock.Setup(c => c.Role).Returns("student");

        _db.Evaluations.Add(new Evaluation { InternshipID = 1, EvaluatorRole = "student_eval", TotalScore = 90m });
        await _db.SaveChangesAsync();

        var req = new EvaluationRequestDto(1, "student_eval", null, 80m, "B", "Good company");

        var ex = await Assert.ThrowsAsync<AppException>(() => _evaluationService.CreateEvaluationAsync(req));
        Assert.Equal(400, ex.StatusCode);
        Assert.Contains("มีการประเมิน", ex.Message);
    }

    [Fact]
    public async Task GetEvaluationsAsync_ValidInternship_ReturnsEvaluations()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(1); // Student
        _currentUserMock.Setup(c => c.Role).Returns("student");

        _db.Evaluations.Add(new Evaluation { InternshipID = 1, EvaluatorRole = "student_eval", TotalScore = 90m });
        _db.Evaluations.Add(new Evaluation { InternshipID = 1, EvaluatorRole = "supervisor_eval", TotalScore = 95m });
        await _db.SaveChangesAsync();

        var result = await _evaluationService.GetEvaluationsAsync(1);

        Assert.Equal(2, result.Count());
    }
}
