using System.Security.Claims;
using InternLog.API.Core.Interfaces;

namespace InternLog.API.Core.Security;

/// <summary>
/// Implementation ของ ICurrentUserService
/// ดึง UserID, Role, Name จาก HttpContext.User (JWT Claims)
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    public int UserId { get; }
    public string Role { get; }
    public string Name { get; }
    public bool IsAuthenticated { get; }

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;
        IsAuthenticated = user?.Identity?.IsAuthenticated ?? false;

        if (IsAuthenticated)
        {
            // "sub" claim = UserID (ตาม JWT config)
            var sub = user!.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? user!.FindFirstValue("sub");
            UserId = int.TryParse(sub, out var id) ? id : 0;

            Role = user!.FindFirstValue(ClaimTypes.Role)
                ?? user!.FindFirstValue("role")
                ?? string.Empty;

            Name = user!.FindFirstValue(ClaimTypes.Name)
                ?? user!.FindFirstValue("name")
                ?? string.Empty;
        }
        else
        {
            UserId = 0;
            Role = string.Empty;
            Name = string.Empty;
        }
    }
}
