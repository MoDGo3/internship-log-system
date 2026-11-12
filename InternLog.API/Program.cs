using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using InternLog.API.Core.Interfaces;
using InternLog.API.Core.Security;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Infrastructure.Middleware;
using InternLog.API.Modules.Auth;
using QuestPDF.Infrastructure;
using InternLog.API.Core.Common;

var builder = WebApplication.CreateBuilder(args);

// QuestPDF License setup
QuestPDF.Settings.License = LicenseType.Community;

// ─── 1. Database (EF Core + SQL Server) ─────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(3)   // retry 3 ครั้ง ถ้า network ขัดข้อง
    )
);

// ─── 2. JWT Authentication ───────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key ไม่ได้ตั้งค่าใน appsettings");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ClockSkew                = TimeSpan.Zero, // ไม่มี grace period
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            RoleClaimType            = ClaimTypes.Role,
            NameClaimType            = "name"
        };

        // ส่ง error message ภาษาไทยใน response
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async ctx =>
            {
                ctx.HandleResponse();
                ctx.Response.StatusCode  = 401;
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsync(
                    """{"success":false,"message":"กรุณาเข้าสู่ระบบก่อนใช้งาน","data":null}"""
                );
            },
            OnForbidden = async ctx =>
            {
                ctx.Response.StatusCode  = 403;
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsync(
                    """{"success":false,"message":"ไม่มีสิทธิ์ดำเนินการ","data":null}"""
                );
            }
        };
    });

builder.Services.AddAuthorization();

// ─── 3. CORS (Angular dev server) ───────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithExposedHeaders("Content-Disposition"); // ให้ Angular อ่านชื่อไฟล์ตอนดาวน์โหลดได้
    });
});

// ─── 4. Swagger + Bearer Token Support ──────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title   = "InternLog API",
        Version = "v1",
        Description = "ระบบจดบันทึกการฝึกงาน — Backend API"
    });
});

// ─── 5. Controllers ──────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ─── 6. HTTP Context Accessor (สำหรับ CurrentUserService) ───────────────────
builder.Services.AddHttpContextAccessor();

// ─── 7. Core Services ────────────────────────────────────────────────────────
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
// ไฟล์อัปโหลดเก็บใน App_Data/uploads (นอก wwwroot) — ดาวน์โหลดได้เฉพาะผ่าน endpoint ที่ตรวจสิทธิ์
builder.Services.AddSingleton<InternLog.API.Core.Storage.IFileStorage, InternLog.API.Core.Storage.LocalFileStorage>();

// ─── 8. Module Services (เพิ่มทีละโมดูลตาม development) ─────────────────────
builder.Services.AddScoped<InternLog.API.Modules.Auth.IAuthService, InternLog.API.Modules.Auth.AuthService>();
builder.Services.AddScoped<InternLog.API.Modules.Attendance.IAttendanceService, InternLog.API.Modules.Attendance.AttendanceService>();
builder.Services.AddScoped<InternLog.API.Modules.Leaves.ILeaveService, InternLog.API.Modules.Leaves.LeaveService>();
// TODO: เพิ่ม module อื่นๆ เมื่อพัฒนาแล้ว
builder.Services.AddScoped<InternLog.API.Modules.AdvisorVisits.IAdvisorVisitService, InternLog.API.Modules.AdvisorVisits.AdvisorVisitService>();
builder.Services.AddScoped<InternLog.API.Modules.Reports.IReportService, InternLog.API.Modules.Reports.ReportService>();
builder.Services.AddScoped<InternLog.API.Modules.Tasks.ITaskService, InternLog.API.Modules.Tasks.TaskService>();
builder.Services.AddScoped<InternLog.API.Modules.WorkLogs.IWorkLogService, InternLog.API.Modules.WorkLogs.WorkLogService>();
builder.Services.AddScoped<InternLog.API.Modules.Notifications.INotificationService, InternLog.API.Modules.Notifications.NotificationService>();
builder.Services.AddScoped<InternLog.API.Modules.Evaluations.IEvaluationService, InternLog.API.Modules.Evaluations.EvaluationService>();
builder.Services.AddScoped<InternLog.API.Modules.Admin.IAdminService, InternLog.API.Modules.Admin.AdminService>();
builder.Services.AddScoped<InternLog.API.Modules.Dashboard.IDashboardService, InternLog.API.Modules.Dashboard.DashboardService>();
builder.Services.AddScoped<InternLog.API.Modules.Companies.ICompanyService, InternLog.API.Modules.Companies.CompanyService>();

// ─── Build App ───────────────────────────────────────────────────────────────
var app = builder.Build();

// ─── Middleware Pipeline (ลำดับสำคัญมาก!) ───────────────────────────────────
app.UseMiddleware<ExceptionMiddleware>(); // ต้องอยู่ก่อนสุด

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "InternLog API v1");
        c.RoutePrefix = "swagger"; // เปิดที่ /swagger
    });
}

app.UseHttpsRedirection();
app.UseCors("Angular");
app.UseAuthentication(); // JWT ก่อน
app.UseAuthorization();  // RBAC หลัง
app.MapControllers();

// ─── Auto Migration + Seed (Development only) ────────────────────────────────
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate();
        Console.WriteLine("✅ Database migration สำเร็จ");

        // ข้อมูลทดสอบ (ผู้ใช้ทดสอบ, บริษัท, ช่วงฝึกงาน, internship) — ดู DevDataSeeder
        DevDataSeeder.Seed(db, app.Logger);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  Migration/Seed error: {ex.Message}");
    }
}

app.Run();
