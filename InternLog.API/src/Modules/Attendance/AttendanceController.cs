using InternLog.API.Core.Common;
using InternLog.API.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternLog.API.Modules.Attendance;

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
    public async Task<IActionResult> CheckIn([FromBody] string? note)
    {
        var result = await _attendanceService.CheckInAsync(_currentUser.UserId, note);
        return Ok(ApiResponse<AttendanceDto>.Ok(result, "ลงเวลาเข้างานสำเร็จ"));
    }

    [HttpPost("check-out")]
    [Authorize(Roles = "student")]
    public async Task<IActionResult> CheckOut([FromBody] string? note)
    {
        var result = await _attendanceService.CheckOutAsync(_currentUser.UserId, note);
        return Ok(ApiResponse<AttendanceDto>.Ok(result, "ลงเวลาออกงานสำเร็จ"));
    }

    [HttpGet]
    [Authorize] // Student, Advisor, Supervisor
    public async Task<IActionResult> GetHistory([FromQuery] int? month, [FromQuery] int? year)
    {
        var result = await _attendanceService.GetHistoryAsync(_currentUser.UserId, month, year);
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
    [Authorize(Roles = "advisor,supervisor")]
    public async Task<IActionResult> ReviewEdit(int id, [FromBody] ReviewEditDto dto)
    {
        await _attendanceService.ReviewEditAsync(id, _currentUser.UserId, dto);
        var msg = dto.IsApproved ? "อนุมัติการขอแก้ไขเวลาสำเร็จ" : "ปฏิเสธการขอแก้ไขเวลาสำเร็จ";
        return Ok(ApiResponse<object>.Ok(null!, msg));
    }
}
