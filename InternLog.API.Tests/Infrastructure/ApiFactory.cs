using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using InternLog.API.Infrastructure.Data;

namespace InternLog.API.Tests.Infrastructure;

/// <summary>
/// รัน API ทั้ง pipeline (middleware, auth, controllers) ใน memory
/// แทน SQL Server ด้วย SQLite in-memory และเก็บไฟล์ในโฟลเดอร์ชั่วคราว
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail = "admin@internlog.dev"; // มาจาก HasData
    public const string AdminPassword = "Admin@1234!";

    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly string _fileRoot = Path.Combine(Path.GetTempPath(), "internlog-api-tests", Guid.NewGuid().ToString("N"));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.UseEnvironment("Testing"); // ไม่ใช่ Development → ไม่ migrate/seed SQL Server
        builder.UseSetting("Jwt:Key", "integration-test-signing-key-at-least-32-chars");
        builder.UseSetting("FileStorage:Root", _fileRoot);

        builder.ConfigureServices(services =>
        {
            // ถอดการตั้งค่า SQL Server ออกทั้งหมด แล้วใช้ SQLite แทน
            foreach (var descriptor in services.Where(d =>
                         d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                         d.ServiceType == typeof(AppDbContext) ||
                         d.ServiceType == typeof(IDbContextOptionsConfiguration<AppDbContext>)).ToList())
            {
                services.Remove(descriptor);
            }

            var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
            services.AddScoped<AppDbContext>(_ => new SqliteAppDbContext(options));

            using var db = new SqliteAppDbContext(options);
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;
        _connection.Dispose();
        if (Directory.Exists(_fileRoot)) Directory.Delete(_fileRoot, recursive: true);
    }
}
