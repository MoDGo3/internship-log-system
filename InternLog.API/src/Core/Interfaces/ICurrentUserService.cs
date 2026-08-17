namespace InternLog.API.Core.Interfaces;

/// <summary>
/// ดึงข้อมูล User ปัจจุบันจาก JWT Claims
/// Inject ผ่าน DI ทุกโมดูลที่ต้องการ UserId
/// </summary>
public interface ICurrentUserService
{
    int UserId { get; }
    string Role { get; }
    string Name { get; }
    bool IsAuthenticated { get; }
}
