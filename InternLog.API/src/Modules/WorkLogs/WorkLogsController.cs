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

    [HttpGet]
    [Authorize(Roles = "student")]
    public async Task<IActionResult> GetStudentLogs()
    {
        var logs = await _workLogService.GetStudentLogsAsync();
        return Ok(ApiResponse<IEnumerable<WorkLogResponseDto>>.Ok(logs));
    }

    [HttpGet("pending")]
    [Authorize(Roles = "advisor,supervisor")]
    public async Task<IActionResult> GetPendingLogs()
    {
        var logs = await _workLogService.GetPendingLogsAsync();
        return Ok(ApiResponse<IEnumerable<WorkLogResponseDto>>.Ok(logs));
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "student,advisor,supervisor")]
    public async Task<IActionResult> GetLogDetail(int id)
    {
        var log = await _workLogService.GetLogDetailAsync(id);
        return Ok(ApiResponse<WorkLogResponseDto>.Ok(log));
    }

    [HttpPost]
    [Authorize(Roles = "student")]
    public async Task<IActionResult> CreateLog([FromBody] WorkLogRequestDto request, [FromQuery] bool submit = false)
    {
        var result = await _workLogService.CreateLogAsync(request, submit);
        var msg = submit ? "สร้างและส่งตรวจบันทึกการทำงานเรียบร้อย" : "บันทึกร่างเรียบร้อย";
        return CreatedAtAction(nameof(GetLogDetail), new { id = result.LogID }, ApiResponse<WorkLogResponseDto>.Ok(result, msg));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "student")]
    public async Task<IActionResult> UpdateLog(int id, [FromBody] WorkLogRequestDto request)
    {
        var result = await _workLogService.UpdateLogAsync(id, request);
        return Ok(ApiResponse<WorkLogResponseDto>.Ok(result, "แก้ไขบันทึกการทำงานเรียบร้อย"));
    }

    [HttpPut("{id}/submit")]
    [Authorize(Roles = "student")]
    public async Task<IActionResult> SubmitLog(int id)
    {
        var result = await _workLogService.SubmitLogAsync(id);
        return Ok(ApiResponse<WorkLogResponseDto>.Ok(result, "ส่งตรวจบันทึกการทำงานเรียบร้อย"));
    }

    [HttpPut("{id}/review")]
    [Authorize(Roles = "advisor,supervisor")]
    public async Task<IActionResult> ReviewLog(int id, [FromBody] ReviewLogDto request)
    {
        var result = await _workLogService.ReviewLogAsync(id, request);
        var msg = request.IsApproved ? "อนุมัติบันทึกการทำงาน" : "ตีกลับให้แก้ไข";
        return Ok(ApiResponse<WorkLogResponseDto>.Ok(result, msg));
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
    [Authorize(Roles = "student,advisor,supervisor")]
    public async Task<IActionResult> AddComment(int id, [FromBody] CommentRequestDto request)
    {
        var result = await _workLogService.AddCommentAsync(id, request);
        return Ok(ApiResponse<CommentResponseDto>.Ok(result, "เพิ่มคอมเมนต์เรียบร้อย"));
    }
}
