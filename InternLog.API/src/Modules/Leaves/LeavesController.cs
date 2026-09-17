using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using InternLog.API.Modules.Leaves.DTOs;
using InternLog.API.Core.Common;

namespace InternLog.API.Modules.Leaves;

[ApiController]
[Route("api/[controller]")]
public class LeavesController : ControllerBase
{
    private readonly ILeaveService _leaveService;

    public LeavesController(ILeaveService leaveService)
    {
        _leaveService = leaveService;
    }

    [HttpGet("quota")]
    [Authorize(Roles = "student")]
    public async Task<IActionResult> GetQuotas()
    {
        var quotas = await _leaveService.GetQuotasAsync();
        return Ok(ApiResponse<IEnumerable<LeaveQuotaDto>>.Ok(quotas));
    }

    [HttpGet]
    [Authorize(Roles = "student")]
    public async Task<IActionResult> GetStudentLeaves()
    {
        var leaves = await _leaveService.GetStudentLeavesAsync();
        return Ok(ApiResponse<IEnumerable<LeaveResponseDto>>.Ok(leaves));
    }

    [HttpGet("pending")]
    [Authorize(Roles = "advisor")]
    public async Task<IActionResult> GetPendingLeaves()
    {
        var leaves = await _leaveService.GetPendingLeavesAsync();
        return Ok(ApiResponse<IEnumerable<LeaveResponseDto>>.Ok(leaves));
    }

    [HttpPost]
    [Authorize(Roles = "student")]
    public async Task<IActionResult> SubmitLeave([FromForm] LeaveRequestDto request, IFormFile? attachment)
    {
        var result = await _leaveService.SubmitLeaveAsync(request, attachment);
        return CreatedAtAction(nameof(GetStudentLeaves), null, ApiResponse<LeaveResponseDto>.Ok(result, "ยื่นคำขอลาเรียบร้อยแล้ว"));
    }

    [HttpPut("{id}/review")]
    [Authorize(Roles = "advisor,admin")]
    public async Task<IActionResult> ReviewLeave(int id, [FromBody] ReviewLeaveDto request)
    {
        var result = await _leaveService.ReviewLeaveAsync(id, request);
        var msg = request.IsApproved ? "อนุมัติคำขอลา" : "ไม่อนุมัติคำขอลา";
        return Ok(ApiResponse<LeaveResponseDto>.Ok(result, msg));
    }
}
