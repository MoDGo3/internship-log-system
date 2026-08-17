using InternLog.API.Modules.Auth.DTOs;

namespace InternLog.API.Modules.Auth;

/// <summary>
/// Interface สำหรับ Auth business logic
/// ทุกโมดูลที่ต้องการ Auth ให้ inject interface นี้ (ห้าม inject AuthService โดยตรง)
/// </summary>
public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest req);
    Task<LoginResponse> RegisterAsync(RegisterRequest req);
    Task<LoginResponse> RefreshAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
    Task<UserDto> GetMeAsync(int userId);
    Task ChangePasswordAsync(int userId, ChangePasswordRequest req);
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(ResetPasswordRequest req);
    Task<bool> EmailExistsAsync(string email);
}
