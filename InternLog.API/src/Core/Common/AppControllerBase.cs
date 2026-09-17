using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace InternLog.API.Core.Common;

[ApiController]
[Route("api/[controller]")]
public abstract class AppControllerBase : ControllerBase
{
    protected int GetUserId()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdStr, out var userId)) return userId;
        
        userIdStr = User.FindFirstValue("id");
        if (int.TryParse(userIdStr, out userId)) return userId;

        return 0;
    }

    protected string? GetUserRole()
    {
        return User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role");
    }
}