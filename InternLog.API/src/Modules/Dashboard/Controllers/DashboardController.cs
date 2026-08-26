using InternLog.API.Core.Common;
using InternLog.API.Modules.Dashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InternLog.API.Modules.Dashboard.Controllers;

[Authorize]
public class DashboardController : AppControllerBase {
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService) {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard() {
        var role = User.FindFirstValue(ClaimTypes.Role);
        
        object result = role switch {
            "student" => await _dashboardService.GetStudentDashboardAsync(GetUserId()),
            "advisor" => await _dashboardService.GetAdvisorDashboardAsync(GetUserId()),
            "admin" => await _dashboardService.GetAdminDashboardAsync(),
            _ => null!
        };

        if (result == null) return Forbid();

        return Ok(new ApiResponse<object>(true, "ดึงข้อมูล Dashboard สำเร็จ", result));
    }
}
