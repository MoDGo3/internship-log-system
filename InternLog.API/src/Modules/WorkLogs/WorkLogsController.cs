using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using InternLog.API.Modules.WorkLogs.DTOs;
using InternLog.API.Core.Common;

namespace InternLog.API.Modules.WorkLogs;

[ApiController]
[Route("api/[controller]")]
public class WorkLogsController : ControllerBase
{
    private readonly IWorkLogService _workLogService;

    public WorkLogsController(IWorkLogService workLogService)
    {
        _workLogService = workLogService;
    }

    [HttpGet("categories")]
    [Authorize]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _workLogService.GetCategoriesAsync();
        return Ok(ApiResponse<IEnumerable<WorkCategoryDto>>.Ok(categories));
    }

    [HttpGet]
    [Authorize(Roles = "student")]
    public async Task<IActionResult> GetStudentLogs()
    {
        var logs = await _workLogService.GetStudentLogsAsync();
        return Ok(ApiResponse<IEnumerable<WorkLogResponseDto>>.Ok(logs));
    }

    [HttpGet("all")]
    [Authorize(Roles = "advisor,admin")]
    public async Task<IActionResult> GetAllLogs()
    {
        var logs = await _workLogService.GetAllLogsAsync();
        return Ok(ApiResponse<IEnumerable<WorkLogResponseDto>>.Ok(logs));
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "student,advisor,admin")]
    public async Task<IActionResult> GetLogDetail(int id)
    {
        var log = await _workLogService.GetLogDetailAsync(id);
        return Ok(ApiResponse<WorkLogResponseDto>.Ok(log));
    }

    [HttpPost]
    [Authorize(Roles = "student")]
    public async Task<IActionResult> CreateLog([FromBody] WorkLogRequestDto request)
    {
        var result = await _workLogService.CreateLogAsync(request);
        return CreatedAtAction(nameof(GetLogDetail), new { id = result.LogID }, ApiResponse<WorkLogResponseDto>.Ok(result, "บันทึกงานเรียบร้อย"));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "student")]
    public async Task<IActionResult> UpdateLog(int id, [FromBody] WorkLogRequestDto request)
    {
        var result = await _workLogService.UpdateLogAsync(id, request);
        return Ok(ApiResponse<WorkLogResponseDto>.Ok(result, "แก้ไขบันทึกการทำงานเรียบร้อย"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "student")]
    public async Task<IActionResult> DeleteLog(int id)
    {
        await _workLogService.DeleteLogAsync(id);
        return Ok(ApiResponse<object>.Ok(null!, "ลบบันทึกการทำงานเรียบร้อย"));
    }

    [HttpPost("{id}/attachments")]
    [Authorize(Roles = "student")]
    public async Task<IActionResult> UploadAttachment(int id, IFormFile file)
    {
        var result = await _workLogService.UploadAttachmentAsync(id, file);
        return Ok(ApiResponse<AttachmentResponseDto>.Ok(result, "อัปโหลดไฟล์เรียบร้อย"));
    }

    [HttpDelete("/api/attachments/{id}")]
    [Authorize(Roles = "student")]
    public async Task<IActionResult> DeleteAttachment(int id)
    {
        await _workLogService.DeleteAttachmentAsync(id);
        return Ok(ApiResponse<object>.Ok(null!, "ลบไฟล์แนบเรียบร้อย"));
    }

    [HttpPost("{id}/comments")]
    [Authorize(Roles = "advisor,admin")]
    public async Task<IActionResult> AddComment(int id, [FromBody] CommentRequestDto request)
    {
        var result = await _workLogService.AddCommentAsync(id, request);
        return Ok(ApiResponse<CommentResponseDto>.Ok(result, "เพิ่มคอมเมนต์เรียบร้อย"));
    }
}
