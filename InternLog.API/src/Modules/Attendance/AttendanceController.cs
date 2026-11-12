using InternLog.API.Core.Common;
using InternLog.API.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace InternLog.API.Modules.Attendance;

public class NoteDto
{
    public string? Note { get; set; }
}

[Route("api/[controller]")]
[ApiController]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;
    private readonly ICurrentUserService _currentUser;

    public AttendanceController(IAttendanceService attendanceService, ICurrentUserService currentUser)
    {
        _attendanceService = attendanceService;
        _currentUser = currentUser;
    }

    [HttpPost("check-in")]
    [Authorize(Roles = "student")]
    public async Task<IActionResult> CheckIn([FromBody] NoteDto? dto)
    {
        var result = await _attendanceService.CheckInAsync(_currentUser.UserId, dto?.Note);
        return Ok(ApiResponse<AttendanceDto>.Ok(result, "ลงเวลาเข้างานสำเร็จ"));
    }

    [HttpPost("check-out")]
    [Authorize(Roles = "student")]
    public async Task<IActionResult> CheckOut([FromBody] NoteDto? dto)
    {
        var result = await _attendanceService.CheckOutAsync(_currentUser.UserId, dto?.Note);
        return Ok(ApiResponse<AttendanceDto>.Ok(result, "ลงเวลาออกงานสำเร็จ"));
    }

    [HttpGet]
    [Authorize(Roles = "student,advisor,admin")]
    public async Task<IActionResult> GetHistory([FromQuery] int? month, [FromQuery] int? year, [FromQuery] int? studentId)
    {
        var result = await _attendanceService.GetHistoryAsync(_currentUser.UserId, month, year, studentId);
        return Ok(ApiResponse<IEnumerable<AttendanceDto>>.Ok(result));
    }

    [HttpGet("summary")]
    [Authorize(Roles = "student")]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _attendanceService.GetSummaryAsync(_currentUser.UserId);
        return Ok(ApiResponse<AttendanceSummaryDto>.Ok(result));
    }

    [HttpPut("{id}/request-edit")]
    [Authorize(Roles = "student")]
    public async Task<IActionResult> RequestEdit(int id, [FromBody] RequestEditDto dto)
    {
        await _attendanceService.RequestEditAsync(id, _currentUser.UserId, dto);
        return Ok(ApiResponse<object>.Ok(null!, "ยื่นคำขอแก้ไขเวลาสำเร็จ กรุณารอการพิจารณา"));
    }

    [HttpPut("{id}/review")]
    [Authorize(Roles = "advisor")]
    public async Task<IActionResult> ReviewEdit(int id, [FromBody] ReviewEditDto dto)
    {
        await _attendanceService.ReviewEditAsync(id, _currentUser.UserId, dto);
        var msg = dto.IsApproved ? "อนุมัติการขอแก้ไขเวลาสำเร็จ" : "ปฏิเสธการขอแก้ไขเวลาสำเร็จ";
        return Ok(ApiResponse<object>.Ok(null!, msg));
    }
}
