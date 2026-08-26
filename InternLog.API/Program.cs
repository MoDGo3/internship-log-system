using System.Text;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Infrastructure.Middleware;
using InternLog.API.Modules.Auth.Services;
using InternLog.API.Modules.Attendance.Services;
using InternLog.API.Modules.WorkLogs.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// DbContext Configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// DI
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IWorkLogService, WorkLogService>();
builder.Services.AddScoped<InternLog.API.Modules.Leaves.Services.ILeaveService, InternLog.API.Modules.Leaves.Services.LeaveService>();
builder.Services.AddScoped<InternLog.API.Modules.Tasks.Services.ITaskService, InternLog.API.Modules.Tasks.Services.TaskService>();
builder.Services.AddScoped<InternLog.API.Modules.Calendar.Services.ICalendarService, InternLog.API.Modules.Calendar.Services.CalendarService>();
builder.Services.AddScoped<InternLog.API.Modules.AdvisorVisits.Services.IAdvisorVisitService, InternLog.API.Modules.AdvisorVisits.Services.AdvisorVisitService>();
builder.Services.AddScoped<InternLog.API.Modules.Notifications.Services.INotificationService, InternLog.API.Modules.Notifications.Services.NotificationService>();
builder.Services.AddScoped<InternLog.API.Modules.Reports.Services.IReportService, InternLog.API.Modules.Reports.Services.ReportService>();
builder.Services.AddScoped<InternLog.API.Modules.Admin.Services.IAdminService, InternLog.API.Modules.Admin.Services.AdminService>();
builder.Services.AddScoped<InternLog.API.Modules.Profile.Services.IProfileService, InternLog.API.Modules.Profile.Services.ProfileService>();
builder.Services.AddScoped<InternLog.API.Modules.Dashboard.Services.IDashboardService, InternLog.API.Modules.Dashboard.Services.DashboardService>();
builder.Services.AddScoped<InternLog.API.Modules.Companies.Services.ICompanyService, InternLog.API.Modules.Companies.Services.CompanyService>();
builder.Services.AddScoped<InternLog.API.Modules.Evaluations.Services.IEvaluationService, InternLog.API.Modules.Evaluations.Services.EvaluationService>();

// CORS
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAngular",
        b => b.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "InternLog API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbSeeder.Seed(db);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
