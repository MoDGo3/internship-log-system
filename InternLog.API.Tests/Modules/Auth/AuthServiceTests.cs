using System;
using System.Threading.Tasks;
using InternLog.API.Core.Common;
using InternLog.API.Core.Security;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Infrastructure.Data.Models;
using InternLog.API.Modules.Auth;
using InternLog.API.Modules.Auth.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace InternLog.API.Tests.Modules.Auth;

public class AuthServiceTests
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _db = new AppDbContext(options);
        
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Jwt:Key"]).Returns("TEST_SECRET_KEY_MIN_32_CHARS_FOR_TESTING_ONLY!!");
        configMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        configMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        _config = configMock.Object;

        _loggerMock = new Mock<ILogger<AuthService>>();

        _authService = new AuthService(_db, _config, _loggerMock.Object);

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        var role = new Role { RoleID = 1, RoleName = "student" };
        _db.Roles.Add(role);

        var user = new User
        {
            UserID = 1,
            RoleID = 1,
            Email = "student@test.com",
            PasswordHash = PasswordHasher.Hash("Test@1234"),
            FirstName = "Test",
            LastName = "Student",
            IsActive = true
        };
        _db.Users.Add(user);
        _db.SaveChanges();
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokenPair()
    {
        // Arrange
        var req = new LoginRequest("student@test.com", "Test@1234");

        // Act
        var result = await _authService.LoginAsync(req);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
        Assert.Equal("student@test.com", result.User.Email);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsUnauthorized()
    {
        // Arrange
        var req = new LoginRequest("student@test.com", "WrongPassword");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AppException>(() => _authService.LoginAsync(req));
        Assert.Equal(401, ex.StatusCode);
        Assert.Equal("อีเมลหรือรหัสผ่านไม่ถูกต้อง", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_NonExistentEmail_ThrowsUnauthorized()
    {
        // Arrange
        var req = new LoginRequest("notfound@test.com", "Test@1234");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AppException>(() => _authService.LoginAsync(req));
        Assert.Equal(401, ex.StatusCode);
        Assert.Equal("อีเมลหรือรหัสผ่านไม่ถูกต้อง", ex.Message);
    }
    // ─── Register ───────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_ValidData_ReturnsTokenPairAndCreatesUser()
    {
        var req = new RegisterRequest("newuser@test.com", "Password@123", "New", "User", "6501002", "0812345678", "student");

        var result = await _authService.RegisterAsync(req);

        Assert.NotNull(result);
        Assert.Equal("newuser@test.com", result.User.Email);
        var dbUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == "newuser@test.com");
        Assert.NotNull(dbUser);
        Assert.Equal("student", dbUser.Role.RoleName);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsConflict()
    {
        var req = new RegisterRequest("student@test.com", "Password@123", "New", "User", null, null, "student");

        var ex = await Assert.ThrowsAsync<AppException>(() => _authService.RegisterAsync(req));
        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task RegisterAsync_InvalidRole_ThrowsBadRequest()
    {
        var req = new RegisterRequest("admin2@test.com", "Password@123", "New", "User", null, null, "admin");

        var ex = await Assert.ThrowsAsync<AppException>(() => _authService.RegisterAsync(req));
        Assert.Equal(400, ex.StatusCode);
        Assert.Equal("ไม่สามารถสมัครด้วย role นี้ได้", ex.Message);
    }

    // ─── Refresh Token ────────────────────────────────────────────────────────

    [Fact]
    public async Task RefreshAsync_ValidToken_ReturnsNewTokens()
    {
        // Arrange
        var loginReq = new LoginRequest("student@test.com", "Test@1234");
        var loginRes = await _authService.LoginAsync(loginReq);
        var oldRefreshToken = loginRes.RefreshToken;

        // Act
        var refreshRes = await _authService.RefreshAsync(oldRefreshToken);

        // Assert
        Assert.NotNull(refreshRes);
        Assert.NotEqual(oldRefreshToken, refreshRes.RefreshToken);
        Assert.NotEqual(loginRes.AccessToken, refreshRes.AccessToken);

        // Verify old token is revoked
        var dbToken = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.ReplacedByTokenID != null);
        Assert.NotNull(dbToken);
        Assert.NotNull(dbToken.RevokedAt);
    }

    [Fact]
    public async Task RefreshAsync_InvalidToken_ThrowsUnauthorized()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => _authService.RefreshAsync("invalid_token"));
        Assert.Equal(401, ex.StatusCode);
    }

    // ─── Logout ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task LogoutAsync_ValidToken_RevokesToken()
    {
        var loginRes = await _authService.LoginAsync(new LoginRequest("student@test.com", "Test@1234"));
        var activeTokenCountBefore = await _db.RefreshTokens.CountAsync(rt => rt.RevokedAt == null);

        await _authService.LogoutAsync(loginRes.RefreshToken);

        var activeTokenCountAfter = await _db.RefreshTokens.CountAsync(rt => rt.RevokedAt == null);
        Assert.Equal(activeTokenCountBefore - 1, activeTokenCountAfter);
    }

    // ─── Get Me ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMeAsync_ValidUserId_ReturnsUserDto()
    {
        var result = await _authService.GetMeAsync(1);
        Assert.NotNull(result);
        Assert.Equal("student@test.com", result.Email);
        Assert.Equal("student", result.Role);
    }

    [Fact]
    public async Task GetMeAsync_InvalidUserId_ThrowsNotFound()
    {
        var ex = await Assert.ThrowsAsync<AppException>(() => _authService.GetMeAsync(999));
        Assert.Equal(404, ex.StatusCode);
    }

    // ─── Change Password ─────────────────────────────────────────────────────

    [Fact]
    public async Task ChangePasswordAsync_ValidData_ChangesPasswordAndRevokesTokens()
    {
        var loginRes = await _authService.LoginAsync(new LoginRequest("student@test.com", "Test@1234"));
        var req = new ChangePasswordRequest("Test@1234", "NewPass@123", "NewPass@123");

        await _authService.ChangePasswordAsync(1, req);

        // Verify old password fails
        var ex = await Assert.ThrowsAsync<AppException>(() => _authService.LoginAsync(new LoginRequest("student@test.com", "Test@1234")));
        Assert.Equal(401, ex.StatusCode);

        // Verify new password works
        var newLoginRes = await _authService.LoginAsync(new LoginRequest("student@test.com", "NewPass@123"));
        Assert.NotNull(newLoginRes);

        // Verify old refresh token is revoked
        var refreshEx = await Assert.ThrowsAsync<AppException>(() => _authService.RefreshAsync(loginRes.RefreshToken));
        Assert.Equal(401, refreshEx.StatusCode);
    }

    [Fact]
    public async Task ChangePasswordAsync_PasswordMismatch_ThrowsBadRequest()
    {
        var req = new ChangePasswordRequest("Test@1234", "NewPass@123", "Mismatch@123");
        var ex = await Assert.ThrowsAsync<AppException>(() => _authService.ChangePasswordAsync(1, req));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task ChangePasswordAsync_WrongOldPassword_ThrowsBadRequest()
    {
        var req = new ChangePasswordRequest("Wrong@1234", "NewPass@123", "NewPass@123");
        var ex = await Assert.ThrowsAsync<AppException>(() => _authService.ChangePasswordAsync(1, req));
        Assert.Equal(400, ex.StatusCode);
        Assert.Equal("รหัสผ่านเก่าไม่ถูกต้อง", ex.Message);
    }

    // ─── Forgot & Reset Password ─────────────────────────────────────────────

    [Fact]
    public async Task ForgotPasswordAsync_ValidEmail_CreatesResetToken()
    {
        await _authService.ForgotPasswordAsync("student@test.com");

        var token = await _db.PasswordResetTokens.FirstOrDefaultAsync(t => t.UserID == 1 && !t.IsUsed);
        Assert.NotNull(token);
    }

    [Fact]
    public async Task ResetPasswordAsync_InvalidToken_ThrowsBadRequest()
    {
        var req = new ResetPasswordRequest("invalid_token", "NewPass@123");
        var ex = await Assert.ThrowsAsync<AppException>(() => _authService.ResetPasswordAsync(req));
        Assert.Equal(400, ex.StatusCode);
    }
}
