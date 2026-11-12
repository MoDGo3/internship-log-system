using System.ComponentModel.DataAnnotations;

namespace InternLog.API.Modules.Auth.DTOs;

// ─── Request DTOs ────────────────────────────────────────────────────────────

public record LoginRequest(
    [Required][EmailAddress] string Email,
    [Required] string Password
);

public record RegisterRequest(
    [Required][EmailAddress] string Email,
    [Required][MinLength(8)] string Password,
    [Required][MaxLength(100)] string FirstName,
    [Required][MaxLength(100)] string LastName,
    [MaxLength(20)] string? StudentID,
    [MaxLength(20)] string? Phone,
    string Role = "student"   // default = student
);

public record ChangePasswordRequest(
    [Required] string OldPassword,
    [Required][MinLength(8)] string NewPassword,
    [Required] string ConfirmPassword
);

public record ForgotPasswordRequest(
    [Required][EmailAddress] string Email
);

public record ResetPasswordRequest(
    [Required] string Token,
    [Required][MinLength(8)] string NewPassword
);

public record UpdateProfileRequest(
    [Required][MaxLength(100)] string FirstName,
    [Required][MaxLength(100)] string LastName,
    [MaxLength(20)] string? Phone
);

// ─── Response DTOs ───────────────────────────────────────────────────────────

public record UserDto(
    int    UserID,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string? StudentID,
    string? Phone,
    string? ProfileImage
);

/// <summary>ผลลัพธ์ภายใน AuthService — RefreshToken ถูกส่งให้ browser ผ่าน httpOnly cookie เท่านั้น</summary>
public record LoginResponse(
    string  AccessToken,
    string  RefreshToken,
    UserDto User
);

/// <summary>Response body ที่ส่งให้ client (ไม่มี refresh token)</summary>
public record AuthResponse(
    string  AccessToken,
    UserDto User
);
