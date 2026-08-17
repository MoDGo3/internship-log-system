using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InternLog.API.Modules.Tasks.DTOs;
using InternLog.API.Core.Common;

namespace InternLog.API.Modules.Tasks;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    [Authorize(Roles = "student,advisor,supervisor")]
    public async Task<IActionResult> GetTasks()
    {
        var tasks = await _taskService.GetTasksAsync();
        return Ok(ApiResponse<IEnumerable<TaskResponseDto>>.Ok(tasks));
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "student,advisor,supervisor")]
    public async Task<IActionResult> GetTaskDetail(int id)
    {
        var task = await _taskService.GetTaskDetailAsync(id);
        return Ok(ApiResponse<TaskResponseDto>.Ok(task));
    }

    [HttpPost]
    [Authorize(Roles = "advisor,supervisor")]
    public async Task<IActionResult> CreateTask([FromBody] TaskRequestDto request)
    {
        var result = await _taskService.CreateTaskAsync(request);
        return CreatedAtAction(nameof(GetTaskDetail), new { id = result.TaskID }, ApiResponse<TaskResponseDto>.Ok(result, "มอบหมายงานเรียบร้อย"));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "advisor,supervisor")]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskRequestDto request)
    {
        var result = await _taskService.UpdateTaskAsync(id, request);
        return Ok(ApiResponse<TaskResponseDto>.Ok(result, "แก้ไขข้อมูลงานเรียบร้อย"));
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "student,advisor,supervisor")]
    public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] TaskStatusUpdateDto request)
    {
        var result = await _taskService.UpdateTaskStatusAsync(id, request);
        return Ok(ApiResponse<TaskResponseDto>.Ok(result, "อัปเดตสถานะงานเรียบร้อย"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "advisor,supervisor")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        await _taskService.DeleteTaskAsync(id);
        return Ok(ApiResponse<object>.Ok(null!, "ลบงานเรียบร้อย"));
    }
}
