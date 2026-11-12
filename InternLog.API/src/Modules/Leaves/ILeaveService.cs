using Microsoft.AspNetCore.Http;
using InternLog.API.Core.Storage;
using InternLog.API.Modules.Leaves.DTOs;

namespace InternLog.API.Modules.Leaves;

public interface ILeaveService
{
    Task<IEnumerable<LeaveQuotaDto>> GetQuotasAsync();
    Task<IEnumerable<LeaveResponseDto>> GetStudentLeavesAsync();
    Task<IEnumerable<LeaveResponseDto>> GetPendingLeavesAsync();
    Task<LeaveResponseDto> SubmitLeaveAsync(LeaveRequestDto request, IFormFile? attachment);
    Task<LeaveResponseDto> ReviewLeaveAsync(int leaveId, ReviewLeaveDto reviewDto);
    Task<StoredFile> GetAttachmentAsync(int leaveId);
}
