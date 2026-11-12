using InternLog.API.Core.Common;
using InternLog.API.Core.Security;
using InternLog.API.Infrastructure.Data.Models;

namespace InternLog.API.Infrastructure.Data;

/// <summary>
/// ข้อมูลทดสอบสำหรับ Development เท่านั้น (เรียกจาก Program.cs หลัง migrate)
///
/// ข้อมูลตั้งต้นที่ทุก environment ต้องมี (roles, ประเภทการลา, หมวดงาน, admin ระบบ)
/// อยู่ใน AppDbContext.HasData และถูกใส่ผ่าน migration แล้ว — ไม่ต้อง seed ซ้ำที่นี่
///
/// ทุกขั้นตอนตรวจก่อนเพิ่ม จึงรันซ้ำได้ (idempotent)
/// </summary>
public static class DevDataSeeder
{
    private const string TestPassword = "Test@1234";

    public static void Seed(AppDbContext db, ILogger logger)
    {
        var student = EnsureUser(db, logger, "student@test.com", Constants.Roles.Student, "สมชาย", "ใจดี", studentId: "6501001");
        var advisor = EnsureUser(db, logger, "advisor@test.com", Constants.Roles.Advisor, "สมหญิง", "รักเรียน");
        EnsureUser(db, logger, "admin@test.com", Constants.Roles.Admin, "ผู้ดูแล", "ระบบ");

        if (!db.Companies.Any())
        {
            db.Companies.AddRange(
                new Company { CompanyName = "บริษัท ทดสอบ ABC", Address = "123 ถ.ทดสอบ กทม.", Phone = "02-123-4567", WorkStartTime = new TimeOnly(9, 0), WorkEndTime = new TimeOnly(18, 0) },
                new Company { CompanyName = "บริษัท ทดสอบ XYZ", Address = "456 ถ.ตัวอย่าง กทม.", Phone = "02-765-4321", WorkStartTime = new TimeOnly(8, 30), WorkEndTime = new TimeOnly(17, 30) }
            );
            db.SaveChanges();
            logger.LogInformation("Seeded test companies");
        }

        if (!db.TrainingPeriods.Any())
        {
            // ช่วงฝึกงานทดสอบครอบคลุมวันนี้เสมอ: ต้นเดือนที่แล้ว → สิ้นเดือนถัดไปอีก 3 เดือน
            var today = AppClock.Today;
            var start = new DateOnly(today.Year, today.Month, 1).AddMonths(-1);
            var end   = start.AddMonths(4).AddDays(-1);

            db.TrainingPeriods.Add(new TrainingPeriod
            {
                PeriodName = $"ช่วงฝึกงานทดสอบ {start:MM/yyyy}–{end:MM/yyyy}",
                StartDate  = start,
                EndDate    = end,
                IsActive   = true
            });
            db.SaveChanges();
            logger.LogInformation("Seeded test training period {Start} – {End}", start, end);
        }

        if (!db.Internships.Any())
        {
            var company = db.Companies.OrderBy(c => c.CompanyID).First();
            var period  = db.TrainingPeriods.OrderByDescending(p => p.EndDate).First();

            db.Internships.Add(new Internship
            {
                StudentID = student.UserID,
                AdvisorID = advisor.UserID,
                CompanyID = company.CompanyID,
                PeriodID  = period.PeriodID,
                Status    = Constants.InternshipStatus.Active
            });
            db.SaveChanges();
            logger.LogInformation("Seeded test internship (student ↔ advisor)");
        }
    }

    /// <summary>คืนผู้ใช้ที่มีอยู่แล้ว หรือสร้างใหม่ — หา role จากชื่อ ไม่ใช้ RoleID ตายตัว</summary>
    private static User EnsureUser(AppDbContext db, ILogger logger, string email, string roleName,
                                   string firstName, string lastName, string? studentId = null)
    {
        var existing = db.Users.FirstOrDefault(u => u.Email == email);
        if (existing != null) return existing;

        var role = db.Roles.FirstOrDefault(r => r.RoleName == roleName)
            ?? throw new InvalidOperationException($"ไม่พบ role '{roleName}' — ตรวจว่า migration รันครบแล้ว");

        var user = new User
        {
            RoleID       = role.RoleID,
            Email        = email,
            PasswordHash = PasswordHasher.Hash(TestPassword),
            FirstName    = firstName,
            LastName     = lastName,
            StudentID    = studentId,
            IsActive     = true,
            CreatedAt    = AppClock.Now
        };
        db.Users.Add(user);
        db.SaveChanges();
        logger.LogInformation("Seeded test user {Email}", email);
        return user;
    }
}
