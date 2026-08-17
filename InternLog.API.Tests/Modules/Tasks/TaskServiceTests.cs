using System;
using System.Linq;
using System.Threading.Tasks;
using InternLog.API.Core.Common;
using InternLog.API.Core.Interfaces;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Infrastructure.Data.Models;
using InternLog.API.Modules.Tasks;
using InternLog.API.Modules.Tasks.DTOs;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace InternLog.API.Tests.Modules.Tasks;

public class TaskServiceTests
{
    private readonly AppDbContext _db;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly TaskService _taskService;

    public TaskServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _currentUserMock = new Mock<ICurrentUserService>();
        _taskService = new TaskService(_db, _currentUserMock.Object);

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        _db.Roles.Add(new Role { RoleID = 1, RoleName = "student" });
        _db.Roles.Add(new Role { RoleID = 2, RoleName = "advisor" });
        _db.Roles.Add(new Role { RoleID = 3, RoleName = "supervisor" });

        _db.Users.Add(new User { UserID = 1, RoleID = 1, Email = "student@test.com", FirstName = "Stu", LastName = "Dent", IsActive = true });
        _db.Users.Add(new User { UserID = 2, RoleID = 2, Email = "advisor@test.com", FirstName = "Ad", LastName = "Visor", IsActive = true });
        _db.Users.Add(new User { UserID = 3, RoleID = 3, Email = "supervisor@test.com", FirstName = "Super", LastName = "Visor", IsActive = true });
        _db.Users.Add(new User { UserID = 4, RoleID = 2, Email = "advisor2@test.com", FirstName = "Ad2", LastName = "Visor2", IsActive = true });

        _db.Internships.Add(new Internship
        {
            InternshipID = 1,
            StudentID = 1,
            AdvisorID = 2,
            SupervisorID = 3,
            Status = "in_progress"
        });

        _db.SaveChanges();
    }

    [Fact]
    public async Task CreateTaskAsync_SupervisorAssignsTask_ReturnsTask()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(3); // Supervisor
        _currentUserMock.Setup(c => c.Role).Returns("supervisor");

        var req = new TaskRequestDto(1, "Fix Login", "Login page is broken", DateOnly.FromDateTime(DateTime.Now.AddDays(2)), "high");
        var result = await _taskService.CreateTaskAsync(req);

        Assert.NotNull(result);
        Assert.Equal("todo", result.Status);
        Assert.Equal("Fix Login", result.Title);
        Assert.Equal(3, result.AssignedBy);
    }

    [Fact]
    public async Task UpdateTaskStatusAsync_StudentUpdatesStatus_ChangesStatus()
    {
        var task = new TaskItem { InternshipID = 1, AssignedBy = 3, Title = "To Do", Status = "todo" };
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        _currentUserMock.Setup(c => c.UserId).Returns(1); // Student
        _currentUserMock.Setup(c => c.Role).Returns("student");

        var result = await _taskService.UpdateTaskStatusAsync(task.TaskID, new TaskStatusUpdateDto("doing"));

        Assert.Equal("doing", result.Status);
    }

    [Fact]
    public async Task UpdateTaskAsync_AssignerUpdatesTask_UpdatesData()
    {
        var task = new TaskItem { InternshipID = 1, AssignedBy = 3, Title = "Old Title", Detail = "Old detail", Priority = "low", Status = "todo" };
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        _currentUserMock.Setup(c => c.UserId).Returns(3); // Supervisor
        _currentUserMock.Setup(c => c.Role).Returns("supervisor");

        var req = new TaskRequestDto(1, "New Title", "New detail", null, "high");
        var result = await _taskService.UpdateTaskAsync(task.TaskID, req);

        Assert.Equal("New Title", result.Title);
        Assert.Equal("New detail", result.Detail);
        Assert.Equal("high", result.Priority);
    }

    [Fact]
    public async Task DeleteTaskAsync_AssignerDeletesTask_DeletesSuccessfully()
    {
        var task = new TaskItem { InternshipID = 1, AssignedBy = 3, Title = "To Delete", Status = "todo" };
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        _currentUserMock.Setup(c => c.UserId).Returns(3); // Supervisor

        await _taskService.DeleteTaskAsync(task.TaskID);

        var deletedTask = await _db.Tasks.FindAsync(task.TaskID);
        Assert.Null(deletedTask);
    }

    [Fact]
    public async Task GetTasksAsync_ReturnsTasksFilteredByRole()
    {
        _db.Tasks.Add(new TaskItem { InternshipID = 1, AssignedBy = 3, Title = "Task 1", Status = "todo" });
        _db.Tasks.Add(new TaskItem { InternshipID = 1, AssignedBy = 2, Title = "Task 2", Status = "done" });
        await _db.SaveChangesAsync();

        // As student 1: Should see both tasks because they belong to Internship 1
        _currentUserMock.Setup(c => c.UserId).Returns(1);
        _currentUserMock.Setup(c => c.Role).Returns("student");

        var studentTasks = await _taskService.GetTasksAsync();
        Assert.Equal(2, studentTasks.Count());

        // As supervisor 3: Should only see Task 1
        _currentUserMock.Setup(c => c.UserId).Returns(3);
        _currentUserMock.Setup(c => c.Role).Returns("supervisor");

        var supervisorTasks = await _taskService.GetTasksAsync();
        Assert.Single(supervisorTasks);
        Assert.Equal("Task 1", supervisorTasks.First().Title);
    }

    [Fact]
    public async Task CreateTaskAsync_AdvisorAssignsToOtherStudent_ThrowsForbidden()
    {
        // User 4 is Advisor 2, but Internship 1 is supervised by Advisor 2
        _currentUserMock.Setup(c => c.UserId).Returns(4); // Advisor2
        _currentUserMock.Setup(c => c.Role).Returns("advisor");

        var req = new TaskRequestDto(1, "Task Title", null, null, "low");

        var ex = await Assert.ThrowsAsync<AppException>(() => _taskService.CreateTaskAsync(req));
        Assert.Equal(403, ex.StatusCode);
        Assert.Contains("สามารถสั่งงานได้เฉพาะนักศึกษาในที่ปรึกษาของตนเอง", ex.Message);
    }

    [Fact]
    public async Task CreateTaskAsync_StudentAssignsTask_ThrowsForbidden()
    {
        _currentUserMock.Setup(c => c.UserId).Returns(1); // Student
        _currentUserMock.Setup(c => c.Role).Returns("student");

        var req = new TaskRequestDto(1, "Task Title", null, null, "low");

        var ex = await Assert.ThrowsAsync<AppException>(() => _taskService.CreateTaskAsync(req));
        Assert.Equal(403, ex.StatusCode);
        Assert.Contains("นักศึกษาไม่สามารถสร้างงานมอบหมายได้", ex.Message);
    }
}
