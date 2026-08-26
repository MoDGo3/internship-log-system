using System.Security.Claims;
using InternLog.API.Modules.Leaves.DTOs;
using InternLog.API.Modules.Leaves.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternLog.API.Modules.Leaves.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeavesController : ControllerBase {
    private readonly ILeaveService _leaveService;

    public LeavesController(ILeaveService leaveService) {
        _leaveService = leaveService;
    }

    [HttpGet("quota")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetLeaveQuota() {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int studentId)) {
            return Unauthorized(new { message = "Invalid user token." });
        }

        var result = await _leaveService.GetLeaveQuotaAsync(studentId);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetLeaves([FromQuery] string? status) {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

        if (!int.TryParse(userIdClaim, out int userId) || string.IsNullOrEmpty(roleClaim)) {
            return Unauthorized(new { message = "Invalid user token." });
        }

        var result = await _leaveService.GetLeavesAsync(userId, roleClaim, status);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> CreateLeave([FromForm] LeaveCreateRequestDto request) {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int studentId)) {
            return Unauthorized(new { message = "Invalid user token." });
        }

        var result = await _leaveService.CreateLeaveAsync(studentId, request);
        if (!result.Success) return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("{id}/review")]
    [Authorize(Roles = "Advisor,Admin")]
    public async Task<IActionResult> ReviewLeave(int id, [FromBody] LeaveReviewRequestDto request) {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int advisorId)) {
            return Unauthorized(new { message = "Invalid user token." });
        }

        var result = await _leaveService.ReviewLeaveAsync(id, advisorId, request);
        if (!result.Success) return BadRequest(result);

        return Ok(result);
    }
}
