using InternLog.API.Core.Common;
using InternLog.API.Modules.Auth.DTOs;
using InternLog.API.Modules.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternLog.API.Modules.Auth.Controllers;

public class AuthController : AppControllerBase {
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req) {
        var result = await _authService.LoginAsync(req);
        if (result == null) return Unauthorized(new ApiResponse<object>(false, "อีเมลหรือรหัสผ่านไม่ถูกต้อง", null));
        return Ok(new ApiResponse<LoginResponse>(true, "เข้าสู่ระบบสำเร็จ", result));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req) {
        var result = await _authService.RefreshAsync(req);
        if (result == null) return Unauthorized(new ApiResponse<object>(false, "Session หมดอายุ กรุณาเข้าสู่ระบบใหม่", null));
        return Ok(new ApiResponse<LoginResponse>(true, "รีเฟรช Token สำเร็จ", result));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req) {
        if (await _authService.EmailExistsAsync(req.Email))
            return Conflict(new ApiResponse<object>(false, "อีเมลนี้ถูกใช้งานแล้ว", null));
            
        var user = await _authService.RegisterAsync(req);
        return Ok(new ApiResponse<UserDto>(true, "สมัครสมาชิกสำเร็จ", user));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req) {
        await _authService.SendPasswordResetEmailAsync(req.Email);
        return Ok(new ApiResponse<object>(true, "หากอีเมลมีอยู่ในระบบ จะได้รับลิงก์รีเซ็ตรหัสผ่าน", null));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req) {
        var success = await _authService.ResetPasswordAsync(req.Token, req.NewPassword);
        if (!success) return BadRequest(new ApiResponse<object>(false, "ลิงก์ไม่ถูกต้องหรือหมดอายุ", null));
        return Ok(new ApiResponse<object>(true, "รีเซ็ตรหัสผ่านสำเร็จ กรุณาล็อกอินใหม่", null));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe() {
        var user = await _authService.GetMeAsync(GetUserId());
        if (user == null) return Unauthorized();
        return Ok(new ApiResponse<UserDto>(true, "Success", user));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest req) {
        await _authService.LogoutAsync(GetUserId(), req.RefreshToken);
        return Ok(new ApiResponse<object>(true, "ออกจากระบบสำเร็จ", null));
    }

    [Authorize]
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req) {
        var success = await _authService.ChangePasswordAsync(GetUserId(), req);
        if (!success) return BadRequest(new ApiResponse<object>(false, "รหัสผ่านเดิมไม่ถูกต้อง หรือรหัสผ่านใหม่ไม่ตรงกัน", null));
        return Ok(new ApiResponse<object>(true, "เปลี่ยนรหัสผ่านสำเร็จ กรุณาเข้าสู่ระบบใหม่", null));
    }
}
