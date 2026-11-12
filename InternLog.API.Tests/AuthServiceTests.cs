using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using InternLog.API.Core.Common;
using InternLog.API.Core.Security;
using InternLog.API.Infrastructure.Data.Models;
using InternLog.API.Modules.Auth;
using InternLog.API.Modules.Auth.DTOs;
using InternLog.API.Tests.Infrastructure;

namespace InternLog.API.Tests;

public class AuthServiceTests : IDisposable
{
    private const string Password = "Test@1234";

    private readonly TestDb _db = new();
    private readonly AuthService _auth;
    private readonly User _student;

    public AuthServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"]      = "test-signing-key-that-is-at-least-32-characters-long",
                ["Jwt:Issuer"]   = "InternLog.API",
                ["Jwt:Audience"] = "InternLog.Web",
            })
            .Build();

        _auth = new AuthService(_db.Context, config, NullLogger<AuthService>.Instance);
        _student = _db.AddUser("student@test.com", TestDb.StudentRoleId, PasswordHasher.Hash(Password));
    }

    public void Dispose() => _db.Dispose();

    // ─── Login ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WithCorrectPassword_ReturnsTokens()
    {
        var result = await _auth.LoginAsync(new LoginRequest("student@test.com", Password));

        Assert.False(string.IsNullOrEmpty(result.AccessToken));
        Assert.False(string.IsNullOrEmpty(result.RefreshToken));
        Assert.Equal("student", result.User.Role);
    }

    [Theory]
    [InlineData("student@test.com", "wrong-password")]
    [InlineData("nobody@test.com", Password)]
    public async Task Login_WithBadCredentials_ThrowsUnauthorized(string email, string password)
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => _auth.LoginAsync(new LoginRequest(email, password)));
        Assert.Equal(401, ex.StatusCode);
    }

    [Fact]
    public async Task Login_InactiveUser_ThrowsUnauthorized()
    {
        _db.AddUser("inactive@test.com", TestDb.StudentRoleId, PasswordHasher.Hash(Password), isActive: false);

        var ex = await Assert.ThrowsAsync<AppException>(() => _auth.LoginAsync(new LoginRequest("inactive@test.com", Password)));
        Assert.Equal(401, ex.StatusCode);
    }

    // ─── Refresh (rotation) ──────────────────────────────────────────────────

    [Fact]
    public async Task Refresh_RevokesOldToken_AndLinksItToTheNewOne()
    {
        var login = await _auth.LoginAsync(new LoginRequest("student@test.com", Password));

        var refreshed = await _auth.RefreshAsync(login.RefreshToken);

        await using var check = _db.NewContext();
        var oldToken = await check.RefreshTokens.SingleAsync(t => t.TokenHash == Sha256Hasher.Hash(login.RefreshToken));
        var newToken = await check.RefreshTokens.SingleAsync(t => t.TokenHash == Sha256Hasher.Hash(refreshed.RefreshToken));

        Assert.NotNull(oldToken.RevokedAt);
        Assert.Null(newToken.RevokedAt);
        Assert.Equal(newToken.RefreshTokenID, oldToken.ReplacedByTokenID);
    }

    [Fact]
    public async Task Refresh_WithMultipleSessions_LinksTheTokenThisRefreshIssued()
    {
        // ReplacedByTokenID ต้องชี้ไปที่ token ที่ refresh ครั้งนั้นออก ไม่ใช่ token ของ session อื่นของ user เดียวกัน
        // (โค้ดเดิมเดาจาก "token ล่าสุดของ user" ซึ่งผิดเมื่อมี login จากเครื่องอื่นแทรกเข้ามาพร้อมกัน)
        var sessionA = await _auth.LoginAsync(new LoginRequest("student@test.com", Password));
        var refreshedA = await _auth.RefreshAsync(sessionA.RefreshToken);
        var sessionB = await _auth.LoginAsync(new LoginRequest("student@test.com", Password)); // token ใหม่กว่า

        await using var check = _db.NewContext();
        var oldA = await check.RefreshTokens.SingleAsync(t => t.TokenHash == Sha256Hasher.Hash(sessionA.RefreshToken));
        var newA = await check.RefreshTokens.SingleAsync(t => t.TokenHash == Sha256Hasher.Hash(refreshedA.RefreshToken));
        var tokenB = await check.RefreshTokens.SingleAsync(t => t.TokenHash == Sha256Hasher.Hash(sessionB.RefreshToken));

        Assert.Equal(newA.RefreshTokenID, oldA.ReplacedByTokenID);
        Assert.NotEqual(tokenB.RefreshTokenID, oldA.ReplacedByTokenID);
    }

    [Fact]
    public async Task Refresh_ReusingARotatedToken_ThrowsUnauthorized()
    {
        var login = await _auth.LoginAsync(new LoginRequest("student@test.com", Password));
        await _auth.RefreshAsync(login.RefreshToken);

        var ex = await Assert.ThrowsAsync<AppException>(() => _auth.RefreshAsync(login.RefreshToken));
        Assert.Equal(401, ex.StatusCode);
    }

    [Fact]
    public async Task Refresh_AfterUserIsDeactivated_ThrowsUnauthorized()
    {
        var login = await _auth.LoginAsync(new LoginRequest("student@test.com", Password));
        await _db.Context.Users.Where(u => u.UserID == _student.UserID)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.IsActive, false));

        var ex = await Assert.ThrowsAsync<AppException>(() => _auth.RefreshAsync(login.RefreshToken));
        Assert.Equal(401, ex.StatusCode);
    }

    [Fact]
    public async Task Logout_RevokesRefreshToken()
    {
        var login = await _auth.LoginAsync(new LoginRequest("student@test.com", Password));

        await _auth.LogoutAsync(login.RefreshToken);

        await Assert.ThrowsAsync<AppException>(() => _auth.RefreshAsync(login.RefreshToken));
    }

    // ─── Reset password ──────────────────────────────────────────────────────

    [Fact]
    public async Task ResetPassword_SetsNewPassword_RevokesSessions_AndTokenIsSingleUse()
    {
        var login = await _auth.LoginAsync(new LoginRequest("student@test.com", Password));
        const string resetToken = "reset-token-from-email";
        _db.Context.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserID = _student.UserID,
            TokenHash = Sha256Hasher.Hash(resetToken),
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        });
        await _db.Context.SaveChangesAsync();

        await _auth.ResetPasswordAsync(new ResetPasswordRequest(resetToken, "NewPass@5678"));

        // รหัสใหม่ใช้ได้ รหัสเก่าใช้ไม่ได้
        await _auth.LoginAsync(new LoginRequest("student@test.com", "NewPass@5678"));
        await Assert.ThrowsAsync<AppException>(() => _auth.LoginAsync(new LoginRequest("student@test.com", Password)));
        // session เดิมถูก revoke
        await Assert.ThrowsAsync<AppException>(() => _auth.RefreshAsync(login.RefreshToken));
        // token ใช้ซ้ำไม่ได้
        var reuse = await Assert.ThrowsAsync<AppException>(
            () => _auth.ResetPasswordAsync(new ResetPasswordRequest(resetToken, "Another@9999")));
        Assert.Equal(400, reuse.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithExpiredToken_ThrowsBadRequest()
    {
        _db.Context.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserID = _student.UserID,
            TokenHash = Sha256Hasher.Hash("expired"),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        });
        await _db.Context.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<AppException>(
            () => _auth.ResetPasswordAsync(new ResetPasswordRequest("expired", "NewPass@5678")));
        Assert.Equal(400, ex.StatusCode);
    }
}
