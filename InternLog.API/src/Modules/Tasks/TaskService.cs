using Microsoft.EntityFrameworkCore;
using InternLog.API.Modules.Tasks.DTOs;
using InternLog.API.Core.Interfaces;
using InternLog.API.Core.Common;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Infrastructure.Data.Models;

namespace InternLog.API.Modules.Tasks;

public class TaskService : ITaskService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public TaskService(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<TaskResponseDto>> GetTasksAsync()
    {
        var userId = _currentUser.UserId;
        var role = _currentUser.Role;

        IQueryable<TaskItem> query = _context.Tasks
            .Include(t => t.Internship).ThenInclude(i => i.Student)
            .Include(t => t.Assigner);

        if (role == "student")
            query = query.Where(t => t.Internship.StudentID == userId);
        else // advisor or supervisor
            query = query.Where(t => t.AssignedBy == userId);

        var tasks = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();

        return tasks.Select(MapToDto);
    }

    public async Task<TaskResponseDto> GetTaskDetailAsync(int taskId)
    {
        var task = await _context.Tasks
            .Include(t => t.Internship).ThenInclude(i => i.Student)
            .Include(t => t.Assigner)
            .FirstOrDefaultAsync(t => t.TaskID == taskId);

        if (task == null)
            throw AppException.NotFound("ไม่พบงานนี้");

        ValidateTaskAccess(task);

        return MapToDto(task);
    }

    public async Task<TaskResponseDto> CreateTaskAsync(TaskRequestDto request)
    {
        var assignerId = _currentUser.UserId;
        var role = _currentUser.Role;

        if (role == "student")
            throw AppException.Forbidden("นักศึกษาไม่สามารถสร้างงานมอบหมายได้");

        var internship = await _context.Internships
            .Include(i => i.Student)
            .FirstOrDefaultAsync(i => i.InternshipID == request.InternshipID);

        if (internship == null)
            throw AppException.NotFound("ไม่พบข้อมูลการฝึกงานนี้");

        if (role == "advisor" && internship.AdvisorID != assignerId)
            throw AppException.Forbidden("สามารถสั่งงานได้เฉพาะนักศึกษาในที่ปรึกษาของตนเอง");
            
        if (role == "supervisor" && internship.SupervisorID != assignerId)
            throw AppException.Forbidden("สามารถสั่งงานได้เฉพาะนักศึกษาในความดูแลของตนเอง");

        var task = new TaskItem
        {
            InternshipID = request.InternshipID,
            AssignedBy = assignerId,
            Title = request.Title,
            Detail = request.Detail,
            DueDate = request.DueDate,
            Priority = request.Priority,
            Status = "todo",
            CreatedAt = DateTime.Now
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Load navigations
        task.Internship = internship;
        task.Assigner = await _context.Users.FirstAsync(u => u.UserID == assignerId);

        return MapToDto(task);
    }

    public async Task<TaskResponseDto> UpdateTaskAsync(int taskId, TaskRequestDto request)
    {
        var assignerId = _currentUser.UserId;

        var task = await _context.Tasks
            .Include(t => t.Internship).ThenInclude(i => i.Student)
            .Include(t => t.Assigner)
            .FirstOrDefaultAsync(t => t.TaskID == taskId);

        if (task == null)
            throw AppException.NotFound("ไม่พบงานนี้");

        if (task.AssignedBy != assignerId)
            throw AppException.Forbidden("สามารถแก้ไขได้เฉพาะงานที่ตนเองเป็นคนสั่งเท่านั้น");

        task.Title = request.Title;
        task.Detail = request.Detail;
        task.DueDate = request.DueDate;
        task.Priority = request.Priority;
        // Not changing status here, there's another endpoint for that.

        await _context.SaveChangesAsync();

        return MapToDto(task);
    }

    public async Task<TaskResponseDto> UpdateTaskStatusAsync(int taskId, TaskStatusUpdateDto request)
    {
        var task = await _context.Tasks
            .Include(t => t.Internship).ThenInclude(i => i.Student)
            .Include(t => t.Assigner)
            .FirstOrDefaultAsync(t => t.TaskID == taskId);

        if (task == null)
            throw AppException.NotFound("ไม่พบงานนี้");

        ValidateTaskAccess(task);

        task.Status = request.Status;
        await _context.SaveChangesAsync();

        return MapToDto(task);
    }

    public async Task DeleteTaskAsync(int taskId)
    {
        var assignerId = _currentUser.UserId;

        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.TaskID == taskId);

        if (task == null)
            throw AppException.NotFound("ไม่พบงานนี้");

        if (task.AssignedBy != assignerId)
            throw AppException.Forbidden("สามารถลบได้เฉพาะงานที่ตนเองเป็นคนสั่งเท่านั้น");

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
    }

    private void ValidateTaskAccess(TaskItem task)
    {
        var userId = _currentUser.UserId;
        var role = _currentUser.Role;

        if (role == "student" && task.Internship.StudentID != userId)
            throw AppException.Forbidden("คุณไม่มีสิทธิ์เข้าถึงงานนี้");

        if (role != "student" && task.AssignedBy != userId)
            throw AppException.Forbidden("คุณไม่มีสิทธิ์เข้าถึงงานนี้");
    }

    private static TaskResponseDto MapToDto(TaskItem task)
    {
        return new TaskResponseDto(
            task.TaskID,
            task.InternshipID,
            task.Internship?.StudentID ?? 0,
            task.Internship?.Student != null ? $"{task.Internship.Student.FirstName} {task.Internship.Student.LastName}" : string.Empty,
            task.AssignedBy,
            task.Assigner != null ? $"{task.Assigner.FirstName} {task.Assigner.LastName}" : string.Empty,
            task.Title,
            task.Detail,
            task.DueDate,
            task.Priority,
            task.Status,
            task.CreatedAt
        );
    }
}
