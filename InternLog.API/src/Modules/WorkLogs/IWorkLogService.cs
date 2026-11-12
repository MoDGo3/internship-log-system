using Microsoft.AspNetCore.Http;
using InternLog.API.Core.Storage;
using InternLog.API.Modules.WorkLogs.DTOs;

namespace InternLog.API.Modules.WorkLogs;

public interface IWorkLogService
{
    Task<IEnumerable<WorkCategoryDto>> GetCategoriesAsync();
    Task<IEnumerable<WorkLogResponseDto>> GetStudentLogsAsync();
    Task<IEnumerable<WorkLogResponseDto>> GetAllLogsAsync();
    Task<WorkLogResponseDto> GetLogDetailAsync(int logId);
    Task<WorkLogResponseDto> CreateLogAsync(WorkLogRequestDto request);
    Task<WorkLogResponseDto> UpdateLogAsync(int logId, WorkLogRequestDto request);
    Task DeleteLogAsync(int logId);
    Task<AttachmentResponseDto> UploadAttachmentAsync(int logId, IFormFile file);
    Task DeleteAttachmentAsync(int attachmentId);
    Task<StoredFile> GetAttachmentAsync(int attachmentId);
    Task<CommentResponseDto> AddCommentAsync(int logId, CommentRequestDto commentDto);
}
