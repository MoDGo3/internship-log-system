using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InternLog.API.Modules.Dashboard.DTOs;
using InternLog.API.Core.Common;

namespace InternLog.API.Modules.Dashboard;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>ดูภาพรวม Dashboard (ใช้ได้ทุก Role)</summary>
    [HttpGet]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _dashboardService.GetSummaryAsync();
        return Ok(ApiResponse<DashboardSummaryDto>.Ok(summary));
    }

    /// <summary>ดูปฏิทินสถานะเข้างาน (เฉพาะ Student)</summary>
    [HttpGet("calendar")]
    public async Task<IActionResult> GetCalendar([FromQuery] int year, [FromQuery] int month)
    {
        if (year == 0) year = AppClock.Now.Year;
        if (month == 0) month = AppClock.Now.Month;

        var calendar = await _dashboardService.GetCalendarAsync(year, month);
        return Ok(ApiResponse<IEnumerable<CalendarDayDto>>.Ok(calendar));
    }
}
