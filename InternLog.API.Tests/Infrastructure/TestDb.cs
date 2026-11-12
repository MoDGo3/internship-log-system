using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Infrastructure.Data.Models;

namespace InternLog.API.Tests.Infrastructure;

/// <summary>
/// AppDbContext บน SQLite in-memory — ใช้ provider แบบ relational จริง
/// (EF InMemory ไม่รองรับ ExecuteUpdate และไม่ตรวจ foreign key)
/// </summary>
public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection;
    public AppDbContext Context { get; }

    public TestDb()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open(); // database อยู่ตราบที่ connection เปิด

        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        Context = new SqliteAppDbContext(options);
        Context.Database.EnsureCreated(); // ใส่ HasData (roles, leave types, categories, admin) ด้วย
    }

    /// <summary>context ใหม่บน database เดียวกัน — ใช้ตรวจผลลัพธ์โดยไม่ติด change tracker ตัวเดิม</summary>
    public AppDbContext NewContext()
        => new SqliteAppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }

    // ─── Data builders ───────────────────────────────────────────────────────

    public const int StudentRoleId = 1;
    public const int AdvisorRoleId = 2;
    public const int AdminRoleId   = 4;

    public User AddUser(string email, int roleId, string passwordHash = "x", bool isActive = true)
    {
        var user = new User
        {
            Email = email, RoleID = roleId, PasswordHash = passwordHash,
            FirstName = "Test", LastName = email, IsActive = isActive
        };
        Context.Users.Add(user);
        Context.SaveChanges();
        return user;
    }

    public Internship AddInternship(User student, User advisor)
    {
        var company = new Company { CompanyName = "Test Co" };
        var period  = new TrainingPeriod { PeriodName = "Test", StartDate = new DateOnly(2026, 1, 1), EndDate = new DateOnly(2026, 12, 31) };
        Context.AddRange(company, period);
        Context.SaveChanges();

        var internship = new Internship
        {
            StudentID = student.UserID, AdvisorID = advisor.UserID,
            CompanyID = company.CompanyID, PeriodID = period.PeriodID,
            StartDate = period.StartDate, EndDate = period.EndDate, Status = "active"
        };
        Context.Internships.Add(internship);
        Context.SaveChanges();
        return internship;
    }

}

/// <summary>SQLite ไม่มี GETDATE() — แทนด้วย CURRENT_TIMESTAMP เฉพาะใน test</summary>
internal sealed class SqliteAppDbContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);
        foreach (var property in mb.Model.GetEntityTypes().SelectMany(t => t.GetProperties()))
        {
            if (property.GetDefaultValueSql() == "GETDATE()")
                property.SetDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}
