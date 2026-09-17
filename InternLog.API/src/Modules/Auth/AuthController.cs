using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InternLog.API.Core.Common;
using InternLog.API.Core.Interfaces;
using InternLog.API.Modules.Auth.DTOs;

namespace InternLog.API.Modules.Auth;

/// <summary>
/// Auth Controller — 8 endpoints
/// POST /api/auth/login, /register, /refresh, /logout, /forgot-password, /reset-password
/// GET  /api/auth/me
/// PUT  /api/auth/change-password
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IAuthService auth, ICurrentUserService currentUser)
    {
        _auth        = auth;
        _currentUser = currentUser;
    }

    // ─── POST /api/auth/login ────────────────────────────────────────────────
    /// <summary>เข้าสู่ระบบ → ได้ accessToken + refreshToken</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var result = await _auth.LoginAsync(req);
        return Ok(ApiResponse<LoginResponse>.Ok(result, "เข้าสู่ระบบสำเร็จ"));
    }

    // ─── POST /api/auth/register ─────────────────────────────────────────────
    /// <summary>สมัครสมาชิก (role = student เท่านั้น)</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var result = await _auth.RegisterAsync(req);
        return Created($"/api/auth/me", ApiResponse<LoginResponse>.Ok(result, "สมัครสมาชิกสำเร็จ"));
    }

    // ─── POST /api/auth/refresh ──────────────────────────────────────────────
    /// <summary>ต่ออายุ accessToken ด้วย refreshToken (Rotation)</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
    {
        var result = await _auth.RefreshAsync(req.RefreshToken);
        return Ok(ApiResponse<LoginResponse>.Ok(result, "ต่ออายุ token สำเร็จ"));
    }

    // ─── POST /api/auth/logout ───────────────────────────────────────────────
    /// <summary>ออกจากระบบ + revoke refreshToken</summary>
    [HttpPost("logout")]
    [AllowAnonymous] // รับ token ที่หมดอายุแล้วได้ (graceful logout)
    public async Task<IActionResult> Logout([FromBody] RefreshRequest req)
    {
        await _auth.LogoutAsync(req.RefreshToken);
        return Ok(ApiResponse<object>.Ok(null!, "ออกจากระบบสำเร็จ"));
    }

    // ─── GET /api/auth/me ────────────────────────────────────────────────────
    /// <summary>ดึงข้อมูล User ปัจจุบันจาก JWT</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var result = await _auth.GetMeAsync(_currentUser.UserId);
        return Ok(ApiResponse<UserDto>.Ok(result));
    }

    // ─── PUT /api/auth/me ────────────────────────────────────────────────────
    /// <summary>อัปเดตข้อมูลส่วนตัว (ชื่อ, นามสกุล, เบอร์โทร)</summary>
    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest req)
    {
        var result = await _auth.UpdateProfileAsync(_currentUser.UserId, req);
        return Ok(ApiResponse<UserDto>.Ok(result, "อัปเดตข้อมูลสำเร็จ"));
    }


    /// <summary>เปลี่ยนรหัสผ่าน (ต้อง login แล้ว)</summary>
    [HttpPut("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
    {
        await _auth.ChangePasswordAsync(_currentUser.UserId, req);
        return Ok(ApiResponse<object>.Ok(null!, "เปลี่ยนรหัสผ่านสำเร็จ กรุณาเข้าสู่ระบบใหม่"));
    }

    // ─── POST /api/auth/forgot-password ─────────────────────────────────────
    /// <summary>ขอรีเซ็ตรหัสผ่าน (ส่ง email)</summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
    {
        await _auth.ForgotPasswordAsync(req.Email);
        // Message เหมือนกันเสมอ (กัน user enumeration)
        return Ok(ApiResponse<object>.Ok(null!, "หากอีเมลมีอยู่ในระบบ จะได้รับลิงก์รีเซ็ตรหัสผ่าน"));
    }

    // ─── POST /api/auth/reset-password ──────────────────────────────────────
    /// <summary>รีเซ็ตรหัสผ่านด้วย token จาก email</summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        await _auth.ResetPasswordAsync(req);
        return Ok(ApiResponse<object>.Ok(null!, "รีเซ็ตรหัสผ่านสำเร็จ กรุณาเข้าสู่ระบบใหม่"));
    }
}
