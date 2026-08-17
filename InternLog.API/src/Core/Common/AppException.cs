namespace InternLog.API.Core.Common;

/// <summary>
/// Custom exception สำหรับ business logic errors
/// ExceptionMiddleware จะแปลงเป็น HTTP response
/// </summary>
public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 400)
        : base(message)
    {
        StatusCode = statusCode;
    }

    // Static factory methods
    public static AppException BadRequest(string msg) => new(msg, 400);
    public static AppException Unauthorized(string msg = "ไม่มีสิทธิ์เข้าถึง") => new(msg, 401);
    public static AppException Forbidden(string msg = "ไม่มีสิทธิ์ดำเนินการ") => new(msg, 403);
    public static AppException NotFound(string msg) => new(msg, 404);
    public static AppException Conflict(string msg) => new(msg, 409);
}
