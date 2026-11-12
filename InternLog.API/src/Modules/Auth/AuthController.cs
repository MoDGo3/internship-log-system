using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InternLog.API.Core.Common;
using InternLog.API.Core.Interfaces;
using InternLog.API.Modules.Auth.DTOs;

namespace InternLog.API.Modules.Auth;

/// <summary>
/// Auth Controller
/// POST /api/auth/login, /register, /refresh, /logout, /forgot-password, /reset-password
/// GET/PUT /api/auth/me · PUT /api/auth/change-password
///
/// Refresh token ส่งผ่าน httpOnly cookie เท่านั้น (JavaScript อ่านไม่ได้ → กันขโมยด้วย XSS)
/// Response body มีแค่ accessToken + user
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string RefreshCookieName = "refreshToken";
    private const string RefreshCookiePath = "/api/auth"; // ส่ง cookie เฉพาะ endpoint ของ auth

    private readonly IAuthService _auth;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IAuthService auth, ICurrentUserService currentUser)
    {
        _auth        = auth;
        _currentUser = currentUser;
    }

    // ─── POST /api/auth/login ────────────────────────────────────────────────
    /// <summary>เข้าสู่ระบบ → accessToken ใน body + refreshToken ใน cookie</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var result = await _auth.LoginAsync(req);
        return Ok(ApiResponse<AuthResponse>.Ok(IssueSession(result), "เข้าสู่ระบบสำเร็จ"));
    }

    // ─── POST /api/auth/register ─────────────────────────────────────────────
    /// <summary>สมัครสมาชิก (role = student เท่านั้น)</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var result = await _auth.RegisterAsync(req);
        return Created("/api/auth/me", ApiResponse<AuthResponse>.Ok(IssueSession(result), "สมัครสมาชิกสำเร็จ"));
    }

    // ─── POST /api/auth/refresh ──────────────────────────────────────────────
    /// <summary>ต่ออายุ accessToken ด้วย refresh token จาก cookie (Rotation)</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies[RefreshCookieName];
        if (string.IsNullOrEmpty(refreshToken))
            throw AppException.Unauthorized("กรุณาเข้าสู่ระบบใหม่");

        try
        {
            var result = await _auth.RefreshAsync(refreshToken);
            return Ok(ApiResponse<AuthResponse>.Ok(IssueSession(result), "ต่ออายุ token สำเร็จ"));
        }
        catch (AppException)
        {
            ClearRefreshCookie(); // cookie ใช้ไม่ได้แล้ว ลบทิ้ง
            throw;
        }
    }

    // ─── POST /api/auth/logout ───────────────────────────────────────────────
    /// <summary>ออกจากระบบ + revoke refresh token + ลบ cookie</summary>
    [HttpPost("logout")]
    [AllowAnonymous] // ใช้ได้แม้ access token หมดอายุแล้ว (graceful logout)
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies[RefreshCookieName];
        if (!string.IsNullOrEmpty(refreshToken))
            await _auth.LogoutAsync(refreshToken);

        ClearRefreshCookie();
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

    // ─── PUT /api/auth/change-password ───────────────────────────────────────
    /// <summary>เปลี่ยนรหัสผ่าน (ต้อง login แล้ว) — refresh token ทุกตัวถูก revoke</summary>
    [HttpPut("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
    {
        await _auth.ChangePasswordAsync(_currentUser.UserId, req);
        ClearRefreshCookie();
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
        ClearRefreshCookie();
        return Ok(ApiResponse<object>.Ok(null!, "รีเซ็ตรหัสผ่านสำเร็จ กรุณาเข้าสู่ระบบใหม่"));
    }

    // ─── Cookie helpers ──────────────────────────────────────────────────────

    /// <summary>ตั้ง refresh token ลง cookie แล้วคืน body ที่ไม่มี refresh token</summary>
    private AuthResponse IssueSession(LoginResponse result)
    {
        Response.Cookies.Append(RefreshCookieName, result.RefreshToken, RefreshCookieOptions(
            DateTimeOffset.UtcNow.AddDays(Constants.Jwt.RefreshTokenDays)));
        return new AuthResponse(result.AccessToken, result.User);
    }

    private void ClearRefreshCookie()
        => Response.Cookies.Delete(RefreshCookieName, RefreshCookieOptions(expires: null));

    private CookieOptions RefreshCookieOptions(DateTimeOffset? expires) => new()
    {
        HttpOnly = true,
        Secure   = Request.IsHttps,      // บังคับ HTTPS เมื่อรันบน HTTPS (production)
        SameSite = SameSiteMode.Strict,  // ไม่ส่ง cookie จากเว็บอื่น (กัน CSRF)
        Path     = RefreshCookiePath,
        Expires  = expires
    };
}
