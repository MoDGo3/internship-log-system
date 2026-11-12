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
    // hash จำลอง ใช้ verify เมื่อไม่พบ email เพื่อให้เวลาตอบเท่ากับกรณีพบ user (กันเดา email จากเวลา)
    private static readonly string DummyPasswordHash = PasswordHasher.Hash(Guid.NewGuid().ToString());

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

        // Timing-safe: รัน BCrypt ทุกครั้ง แม้ไม่พบ user (กันเดา email จากเวลาตอบ)
        var passwordMatches = PasswordHasher.Verify(req.Password, user?.PasswordHash ?? DummyPasswordHash);
        var passwordValid   = user != null && passwordMatches;

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
            CreatedAt    = AppClock.Now
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
        // Hash ครั้งเดียว — O(1) DB lookup แทนการ BCrypt.Verify วนทุก token (กัน DoS)
        var tokenHash = Sha256Hasher.Hash(refreshToken);
        var now       = DateTime.UtcNow;

        var storedToken = await _db.RefreshTokens
            .AsNoTracking()
            .Include(rt => rt.User)
                .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(rt =>
                rt.TokenHash == tokenHash &&
                rt.RevokedAt == null      &&
                rt.ExpiresAt > now);

        if (storedToken is null || !storedToken.User.IsActive)
            throw AppException.Unauthorized("Refresh token ไม่ถูกต้องหรือหมดอายุ");

        // Revoke แบบ atomic (UPDATE ... WHERE RevokedAt IS NULL)
        // ถ้ามี request อื่นใช้ token ตัวเดียวกันไปก่อนแล้ว จะได้ 0 แถว → ไม่ออก token ใหม่ซ้ำ
        var revoked = await _db.RefreshTokens
            .Where(rt => rt.RefreshTokenID == storedToken.RefreshTokenID && rt.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.RevokedAt, now));

        if (revoked == 0)
            throw AppException.Unauthorized("Refresh token ไม่ถูกต้องหรือหมดอายุ");

        var (response, newToken) = await IssueTokenPairAsync(storedToken.User);

        // Rotation chain: ผูก token เก่ากับตัวใหม่ที่เพิ่งออก (ไม่เดาจาก token ล่าสุดของ user)
        await _db.RefreshTokens
            .Where(rt => rt.RefreshTokenID == storedToken.RefreshTokenID)
            .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.ReplacedByTokenID, newToken.RefreshTokenID));

        return response;
    }

    // ─── Logout ─────────────────────────────────────────────────────────────
    public async Task LogoutAsync(string refreshToken)
    {
        var tokenHash = Sha256Hasher.Hash(refreshToken);

        var storedToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(rt =>
                rt.TokenHash == tokenHash &&
                rt.RevokedAt == null      &&
                rt.ExpiresAt > DateTime.UtcNow);

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

    // ─── Update Profile ──────────────────────────────────────────────────────
    public async Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileRequest req)
    {
        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserID == userId && u.IsActive)
            ?? throw AppException.NotFound("ไม่พบข้อมูลผู้ใช้");

        user.FirstName = req.FirstName.Trim();
        user.LastName  = req.LastName.Trim();
        user.Phone     = req.Phone?.Trim();

        await _db.SaveChangesAsync();
        _logger.LogInformation("Profile updated for UserID: {UserId}", userId);

        return ToDto(user);
    }


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
        var tokenHash = Sha256Hasher.Hash(token);

        var resetToken = new PasswordResetToken
        {
            UserID    = user.UserID,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(Constants.PasswordReset.TokenExpiryHours),
            IsUsed    = false,
            CreatedAt = DateTime.UtcNow
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
        // ดึง token ที่ยังไม่ใช้และยังไม่หมดอายุ (hash once → O(1) DB lookup)
        var tokenHash = Sha256Hasher.Hash(req.Token);

        var resetToken = await _db.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t =>
                t.TokenHash == tokenHash &&
                !t.IsUsed               &&
                t.ExpiresAt > DateTime.UtcNow);

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
        => (await IssueTokenPairAsync(user)).Response;

    /// <summary>เหมือน <see cref="GenerateTokenPairAsync"/> แต่คืน entity ของ refresh token ที่บันทึกด้วย</summary>
    private async Task<(LoginResponse Response, RefreshToken Entity)> IssueTokenPairAsync(User user)
    {
        var accessToken  = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        // บันทึก hash ของ refresh token (ไม่บันทึกตัวดิบ) — เวลาของ token ใช้ UTC ทั้งหมด
        var entity = new RefreshToken
        {
            UserID    = user.UserID,
            TokenHash = Sha256Hasher.Hash(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(Constants.Jwt.RefreshTokenDays),
            CreatedAt = DateTime.UtcNow
        };
        _db.RefreshTokens.Add(entity);
        await _db.SaveChangesAsync();

        return (new LoginResponse(accessToken, refreshToken, ToDto(user)), entity);
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
