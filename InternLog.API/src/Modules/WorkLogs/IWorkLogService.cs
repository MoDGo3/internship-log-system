using Microsoft.AspNetCore.Http;
using InternLog.API.Modules.WorkLogs.DTOs;

namespace InternLog.API.Modules.WorkLogs;

public interface IWorkLogService
{
    Task<IEnumerable<WorkLogResponseDto>> GetStudentLogsAsync();
    Task<IEnumerable<WorkLogResponseDto>> GetPendingLogsAsync();
    Task<WorkLogResponseDto> GetLogDetailAsync(int logId);
    Task<WorkLogResponseDto> CreateLogAsync(WorkLogRequestDto request, bool isSubmit);
    Task<WorkLogResponseDto> UpdateLogAsync(int logId, WorkLogRequestDto request);
    Task<WorkLogResponseDto> SubmitLogAsync(int logId);
    Task<WorkLogResponseDto> ReviewLogAsync(int logId, ReviewLogDto reviewDto);
    Task<AttachmentResponseDto> UploadAttachmentAsync(int logId, IFormFile file);
    Task DeleteAttachmentAsync(int attachmentId);
    Task<CommentResponseDto> AddCommentAsync(int logId, CommentRequestDto commentDto);
}
