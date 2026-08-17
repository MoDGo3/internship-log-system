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
            RoleClaimType            = "role",
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
              .AllowCredentials();
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

    // เพิ่ม Bearer token input ใน Swagger UI
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Type         = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = Microsoft.OpenApi.ParameterLocation.Header,
        Description  = "ใส่ accessToken ที่ได้จาก POST /api/auth/login"
    });

    /*
    c.AddSecurityRequirement(new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.OpenApiReference
                {
                    Type = Microsoft.OpenApi.ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    */
});

// ─── 5. Controllers ──────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ─── 6. HTTP Context Accessor (สำหรับ CurrentUserService) ───────────────────
builder.Services.AddHttpContextAccessor();

// ─── 7. Core Services ────────────────────────────────────────────────────────
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// ─── 8. Module Services (เพิ่มทีละโมดูลตาม development) ─────────────────────
builder.Services.AddScoped<InternLog.API.Modules.Auth.IAuthService, InternLog.API.Modules.Auth.AuthService>();
builder.Services.AddScoped<InternLog.API.Modules.Attendance.IAttendanceService, InternLog.API.Modules.Attendance.AttendanceService>();
builder.Services.AddScoped<InternLog.API.Modules.Leaves.ILeaveService, InternLog.API.Modules.Leaves.LeaveService>();
// TODO: เพิ่ม module อื่นๆ เมื่อพัฒนาแล้ว
builder.Services.AddScoped<InternLog.API.Modules.AdvisorVisits.IAdvisorVisitService, InternLog.API.Modules.AdvisorVisits.AdvisorVisitService>();
builder.Services.AddScoped<InternLog.API.Modules.Reports.IReportService, InternLog.API.Modules.Reports.ReportService>();
builder.Services.AddScoped<InternLog.API.Modules.Tasks.ITaskService, InternLog.API.Modules.Tasks.TaskService>();
builder.Services.AddScoped<InternLog.API.Modules.WorkLogs.IWorkLogService, InternLog.API.Modules.WorkLogs.WorkLogService>();
builder.Services.AddScoped<InternLog.API.Modules.Messages.IMessageService, InternLog.API.Modules.Messages.MessageService>();
builder.Services.AddScoped<InternLog.API.Modules.Notifications.INotificationService, InternLog.API.Modules.Notifications.NotificationService>();
builder.Services.AddScoped<InternLog.API.Modules.Evaluations.IEvaluationService, InternLog.API.Modules.Evaluations.EvaluationService>();
builder.Services.AddScoped<InternLog.API.Modules.Admin.IAdminService, InternLog.API.Modules.Admin.AdminService>();
builder.Services.AddScoped<InternLog.API.Modules.Dashboard.IDashboardService, InternLog.API.Modules.Dashboard.DashboardService>();

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
app.UseStaticFiles();
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

        // Seed Roles
        if (!db.Roles.Any())
        {
            db.Roles.AddRange(
                new InternLog.API.Infrastructure.Data.Models.Role { RoleName = "student" },
                new InternLog.API.Infrastructure.Data.Models.Role { RoleName = "advisor" },
                new InternLog.API.Infrastructure.Data.Models.Role { RoleName = "supervisor" },
                new InternLog.API.Infrastructure.Data.Models.Role { RoleName = "admin" }
            );
            db.SaveChanges();
            Console.WriteLine("✅ Seed Roles สำเร็จ");
        }

        // Seed Users
        if (!db.Users.Any())
        {
            var testPasswordHash = InternLog.API.Core.Security.PasswordHasher.Hash("Test@1234");
            db.Users.AddRange(
                new InternLog.API.Infrastructure.Data.Models.User { RoleID = 1, Email = "student@test.com", PasswordHash = testPasswordHash, FirstName = "สมชาย", LastName = "ใจดี", StudentID = "6501001", IsActive = true, CreatedAt = DateTime.Now },
                new InternLog.API.Infrastructure.Data.Models.User { RoleID = 2, Email = "advisor@test.com", PasswordHash = testPasswordHash, FirstName = "สมหญิง", LastName = "รักเรียน", IsActive = true, CreatedAt = DateTime.Now },
                new InternLog.API.Infrastructure.Data.Models.User { RoleID = 3, Email = "supervisor@test.com", PasswordHash = testPasswordHash, FirstName = "วิชัย", LastName = "ทำงาน", IsActive = true, CreatedAt = DateTime.Now },
                new InternLog.API.Infrastructure.Data.Models.User { RoleID = 4, Email = "admin@internlog.dev", PasswordHash = testPasswordHash, FirstName = "Admin", LastName = "System", IsActive = true, CreatedAt = DateTime.Now }
            );
            db.SaveChanges();
            Console.WriteLine("✅ Seed Users สำเร็จ");
        }

        // Seed Companies
        if (!db.Companies.Any())
        {
            db.Companies.AddRange(
                new InternLog.API.Infrastructure.Data.Models.Company { CompanyName = "บริษัท ทดสอบ ABC", Address = "123 ถ.ทดสอบ กทม.", Phone = "02-123-4567", WorkStartTime = new TimeOnly(9,0,0), WorkEndTime = new TimeOnly(18,0,0) },
                new InternLog.API.Infrastructure.Data.Models.Company { CompanyName = "บริษัท ทดสอบ XYZ", Address = "456 ถ.ตัวอย่าง กทม.", Phone = "02-765-4321", WorkStartTime = new TimeOnly(8,30,0), WorkEndTime = new TimeOnly(17,30,0) }
            );
            db.SaveChanges();
            Console.WriteLine("✅ Seed Companies สำเร็จ");
        }

        // Seed TrainingPeriods
        if (!db.TrainingPeriods.Any())
        {
            db.TrainingPeriods.Add(
                new InternLog.API.Infrastructure.Data.Models.TrainingPeriod { PeriodName = "ภาคเรียนที่ 1/2569", StartDate = new DateOnly(2026, 6, 1), EndDate = new DateOnly(2026, 8, 31), IsActive = true }
            );
            db.SaveChanges();
            Console.WriteLine("✅ Seed TrainingPeriods สำเร็จ");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  Migration/Seed error: {ex.Message}");
    }
}

app.Run();
