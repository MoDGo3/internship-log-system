using System.Net;
using System.Text.Json;
using InternLog.API.Core.Common;

namespace InternLog.API.Infrastructure.Middleware;

/// <summary>
/// Global exception handler — แปลง AppException และ Exception ทั่วไปเป็น JSON response
/// ป้องกัน stack trace รั่วออก production
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next  = next;
        _logger = logger;
        _env   = env;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (AppException ex)
        {
            // Business logic errors — log as Warning (ไม่ใช่ error จริง)
            _logger.LogWarning("AppException [{StatusCode}]: {Message}", ex.StatusCode, ex.Message);
            await WriteJsonResponse(ctx, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            // Unexpected errors — log as Error
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

            // ซ่อน detail ใน production
            var message = _env.IsDevelopment()
                ? ex.Message
                : "เกิดข้อผิดพลาดภายในระบบ กรุณาลองใหม่อีกครั้ง";

            await WriteJsonResponse(ctx, (int)HttpStatusCode.InternalServerError, message);
        }
    }

    private static async Task WriteJsonResponse(HttpContext ctx, int statusCode, string message)
    {
        ctx.Response.ContentType = "application/json";
        ctx.Response.StatusCode  = statusCode;

        var response = ApiResponse<object>.Fail(message);
        var json     = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await ctx.Response.WriteAsync(json);
    }
}
