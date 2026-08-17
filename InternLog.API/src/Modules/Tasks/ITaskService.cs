using InternLog.API.Modules.Tasks.DTOs;

namespace InternLog.API.Modules.Tasks;

public interface ITaskService
{
    Task<IEnumerable<TaskResponseDto>> GetTasksAsync();
    Task<TaskResponseDto> GetTaskDetailAsync(int taskId);
    Task<TaskResponseDto> CreateTaskAsync(TaskRequestDto request);
    Task<TaskResponseDto> UpdateTaskAsync(int taskId, TaskRequestDto request);
    Task<TaskResponseDto> UpdateTaskStatusAsync(int taskId, TaskStatusUpdateDto request);
    Task DeleteTaskAsync(int taskId);
}
