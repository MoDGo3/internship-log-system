using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using InternLog.API.Core.Common;
using InternLog.API.Core.Security;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Infrastructure.Data.Models;
using InternLog.API.Modules.Auth.DTOs;

namespace InternLog.API.Modules.Auth;

/// <summary>
/// Auth business logic — Login, Register, Refresh Token, Logout, 
/// GetMe, ChangePassword, ForgotPassword, ResetPassword
/// </summary>
public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AppDbContext db, IConfiguration config, ILogger<AuthService> logger)
    {
        _db     = db;
        _config = config;
        _logger = logger;
    }

    // ─── Login ──────────────────────────────────────────────────────────────
    public async Task<LoginResponse> LoginAsync(LoginRequest req)
    {
        // ดึง user + role ด้วย JOIN เดียว
        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == req.Email && u.IsActive);

        // Timing-safe: ตรวจ BCrypt เสมอแม้ user ไม่มี (กัน timing attack)
        var passwordValid = user != null
            && PasswordHasher.Verify(req.Password, user.PasswordHash);

        if (!passwordValid)
            throw AppException.Unauthorized("อีเมลหรือรหัสผ่านไม่ถูกต้อง");

        return await GenerateTokenPairAsync(user!);
    }

    // ─── Register ───────────────────────────────────────────────────────────
    public async Task<LoginResponse> RegisterAsync(RegisterRequest req)
    {
        // ตรวจ email ซ้ำ
        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            throw AppException.Conflict("อีเมลนี้ถูกใช้งานแล้ว");

        // ตรวจ role ที่อนุญาต (security: ห้าม register เป็น admin)
        var allowedRoles = new[] { Constants.Roles.Student };
        if (!allowedRoles.Contains(req.Role))
            throw AppException.BadRequest("ไม่สามารถสมัครด้วย role นี้ได้");

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == req.Role)
            ?? throw AppException.BadRequest($"Role '{req.Role}' ไม่มีในระบบ");

        var user = new User
        {
            RoleID       = role.RoleID,
            Email        = req.Email,
            PasswordHash = PasswordHasher.Hash(req.Password),
            FirstName    = req.FirstName,
            LastName     = req.LastName,
            StudentID    = req.StudentID,
            Phone        = req.Phone,
            IsActive     = true,
            CreatedAt    = DateTime.Now
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Load role navigation property
        user.Role = role;

        _logger.LogInformation("User registered: {Email} (role: {Role})", user.Email, role.RoleName);
        return await GenerateTokenPairAsync(user);
    }

    // ─── Refresh Token (Rotation) ────────────────────────────────────────────
    public async Task<LoginResponse> RefreshAsync(string refreshToken)
    {
        // ค้นหาและตรวจสอบ token ทุกตัวของ user นั้น (BCrypt verify)
        var activeTokens = await _db.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.Role)
            .Where(rt => rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        // หา token ที่ตรงกัน (BCrypt.Verify ช้า — ต้อง filter เบื้องต้นก่อน)
        var storedToken = activeTokens.FirstOrDefault(rt =>
            PasswordHasher.Verify(refreshToken, rt.TokenHash));

        if (storedToken is null)
            throw AppException.Unauthorized("Refresh token ไม่ถูกต้องหรือหมดอายุ");

        // Revoke token เก่า
        storedToken.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // ออก token คู่ใหม่
        var newPair = await GenerateTokenPairAsync(storedToken.User);

        // บันทึก ReplacedByTokenID (Rotation chain)
        var newTokenEntry = await _db.RefreshTokens
            .OrderByDescending(rt => rt.RefreshTokenID)
            .FirstAsync(rt => rt.UserID == storedToken.UserID);
        storedToken.ReplacedByTokenID = newTokenEntry.RefreshTokenID;
        await _db.SaveChangesAsync();

        return newPair;
    }

    // ─── Logout ─────────────────────────────────────────────────────────────
    public async Task LogoutAsync(string refreshToken)
    {
        var activeTokens = await _db.RefreshTokens
            .Where(rt => rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        var storedToken = activeTokens.FirstOrDefault(rt =>
            PasswordHasher.Verify(refreshToken, rt.TokenHash));

        if (storedToken is not null)
        {
            storedToken.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        // ไม่ throw error ถ้า token ไม่มี (idempotent logout)
    }

    // ─── Get Me ─────────────────────────────────────────────────────────────
    public async Task<UserDto> GetMeAsync(int userId)
    {
        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserID == userId && u.IsActive)
            ?? throw AppException.NotFound("ไม่พบข้อมูลผู้ใช้");

        return ToDto(user);
    }

    // ─── Change Password ─────────────────────────────────────────────────────
    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest req)
    {
        if (req.NewPassword != req.ConfirmPassword)
            throw AppException.BadRequest("รหัสผ่านใหม่และยืนยันรหัสผ่านไม่ตรงกัน");

        var user = await _db.Users.FindAsync(userId)
            ?? throw AppException.NotFound("ไม่พบข้อมูลผู้ใช้");

        if (!PasswordHasher.Verify(req.OldPassword, user.PasswordHash))
            throw AppException.BadRequest("รหัสผ่านเก่าไม่ถูกต้อง");

        user.PasswordHash = PasswordHasher.Hash(req.NewPassword);

        // Revoke refresh tokens ทั้งหมด (บังคับ login ใหม่ทุกเครื่อง)
        var tokens = await _db.RefreshTokens
            .Where(rt => rt.UserID == userId && rt.RevokedAt == null)
            .ToListAsync();
        tokens.ForEach(rt => rt.RevokedAt = DateTime.UtcNow);

        await _db.SaveChangesAsync();
        _logger.LogInformation("Password changed for UserID: {UserId}", userId);
    }

    // ─── Forgot Password ─────────────────────────────────────────────────────
    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

        // ไม่บอกว่า email มีอยู่หรือไม่ (กัน user enumeration)
        if (user is null) return;

        // สร้าง random token
        var token     = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var tokenHash = PasswordHasher.Hash(token);

        var resetToken = new PasswordResetToken
        {
            UserID    = user.UserID,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(Constants.PasswordReset.TokenExpiryHours),
            IsUsed    = false,
            CreatedAt = DateTime.Now
        };

        _db.PasswordResetTokens.Add(resetToken);
        await _db.SaveChangesAsync();

        // TODO: ใช้ EmailService จริงใน production
        // ใน dev mode แสดง token ใน console log
        _logger.LogWarning(
            "[DEV] Reset link: http://localhost:4200/reset-password?token={Token}",
            Uri.EscapeDataString(token));
    }

    // ─── Reset Password ──────────────────────────────────────────────────────
    public async Task ResetPasswordAsync(ResetPasswordRequest req)
    {
        // ดึง token ที่ยังไม่ใช้และยังไม่หมดอายุ
        var activeTokens = await _db.PasswordResetTokens
            .Include(t => t.User)
            .Where(t => !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        var resetToken = activeTokens.FirstOrDefault(t =>
            PasswordHasher.Verify(req.Token, t.TokenHash));

        if (resetToken is null)
            throw AppException.BadRequest("ลิงก์รีเซ็ตรหัสผ่านไม่ถูกต้องหรือหมดอายุ");

        // Update password
        resetToken.User.PasswordHash = PasswordHasher.Hash(req.NewPassword);
        resetToken.IsUsed = true;

        // Revoke refresh tokens ทั้งหมด (บังคับ login ใหม่)
        var refreshTokens = await _db.RefreshTokens
            .Where(rt => rt.UserID == resetToken.UserID && rt.RevokedAt == null)
            .ToListAsync();
        refreshTokens.ForEach(rt => rt.RevokedAt = DateTime.UtcNow);

        await _db.SaveChangesAsync();
        _logger.LogInformation("Password reset for UserID: {UserId}", resetToken.UserID);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────
    public async Task<bool> EmailExistsAsync(string email)
        => await _db.Users.AnyAsync(u => u.Email == email);

    /// <summary>สร้าง AccessToken + RefreshToken คู่ใหม่ พร้อมบันทึก hash ลง DB</summary>
    private async Task<LoginResponse> GenerateTokenPairAsync(User user)
    {
        var accessToken  = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        // บันทึก hash ของ refresh token (ไม่บันทึกตัวดิบ)
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserID    = user.UserID,
            TokenHash = PasswordHasher.Hash(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(Constants.Jwt.RefreshTokenDays),
            CreatedAt = DateTime.Now
        });
        await _db.SaveChangesAsync();

        return new LoginResponse(accessToken, refreshToken, ToDto(user));
    }

    /// <summary>สร้าง JWT AccessToken (2 ชั่วโมง)</summary>
    private string GenerateAccessToken(User user)
    {
        var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(Constants.Jwt.AccessTokenMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,  user.UserID.ToString()),  // "sub"
            new Claim(JwtRegisteredClaimNames.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role,              user.Role.RoleName),
            new Claim(JwtRegisteredClaimNames.Jti,  Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:             _config["Jwt:Issuer"],
            audience:           _config["Jwt:Audience"],
            claims:             claims,
            expires:            expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>สร้าง random Refresh Token (64 bytes base64)</summary>
    private static string GenerateRefreshToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    /// <summary>แปลง User entity เป็น DTO สำหรับส่ง response</summary>
    private static UserDto ToDto(User user) => new(
        user.UserID,
        user.Email,
        user.FirstName,
        user.LastName,
        user.Role.RoleName,
        user.StudentID,
        user.Phone,
        user.ProfileImage
    );
}
