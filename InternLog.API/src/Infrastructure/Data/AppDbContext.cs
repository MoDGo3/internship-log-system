using Microsoft.EntityFrameworkCore;
using InternLog.API.Infrastructure.Data.Models;

namespace InternLog.API.Infrastructure.Data;

/// <summary>
/// AppDbContext รวม 20 ตาราง — ใช้ Fluent API กำหนด constraints
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ===== DbSets (20 ตาราง) =====
    public DbSet<Role>               Roles                => Set<Role>();
    public DbSet<User>               Users                => Set<User>();
    public DbSet<Company>            Companies            => Set<Company>();
    public DbSet<TrainingPeriod>     TrainingPeriods      => Set<TrainingPeriod>();
    public DbSet<Internship>         Internships          => Set<Internship>();
    public DbSet<Attendance>         Attendances          => Set<Attendance>();
    public DbSet<LeaveType>          LeaveTypes           => Set<LeaveType>();
    public DbSet<Leave>              Leaves               => Set<Leave>();
    public DbSet<WorkCategory>       WorkCategories       => Set<WorkCategory>();
    public DbSet<WorkLog>            WorkLogs             => Set<WorkLog>();
    public DbSet<Attachment>         Attachments          => Set<Attachment>();
    public DbSet<LogComment>         LogComments          => Set<LogComment>();
    public DbSet<TaskItem>           Tasks                => Set<TaskItem>();
    public DbSet<AdvisorVisit>       AdvisorVisits        => Set<AdvisorVisit>();
    public DbSet<Notification>       Notifications        => Set<Notification>();
    public DbSet<Report>             Reports              => Set<Report>();
    public DbSet<Evaluation>         Evaluations          => Set<Evaluation>();
    public DbSet<RefreshToken>       RefreshTokens        => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens  => Set<PasswordResetToken>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings => 
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // ─── Roles ──────────────────────────────────────────────────────
        mb.Entity<Role>(e =>
        {
            e.ToTable("Roles");
            e.HasKey(x => x.RoleID);
            e.Property(x => x.RoleName).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.RoleName).IsUnique();
        });

        // ─── Users ──────────────────────────────────────────────────────
        mb.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasKey(x => x.UserID);
            e.Property(x => x.Email).HasMaxLength(150).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();  // UNIQUE constraint
            e.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            e.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            e.Property(x => x.StudentID).HasMaxLength(20);
            e.Property(x => x.Phone).HasMaxLength(20);
            e.Property(x => x.ProfileImage).HasMaxLength(500);
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            // FK: Users.RoleID → Roles.RoleID
            e.HasOne(x => x.Role)
             .WithMany(r => r.Users)
             .HasForeignKey(x => x.RoleID)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── Companies ──────────────────────────────────────────────────
        mb.Entity<Company>(e =>
        {
            e.ToTable("Companies");
            e.HasKey(x => x.CompanyID);
            e.Property(x => x.CompanyName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Address).HasMaxLength(500);
            e.Property(x => x.Phone).HasMaxLength(20);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");
        });

        // ─── TrainingPeriods ────────────────────────────────────────────
        mb.Entity<TrainingPeriod>(e =>
        {
            e.ToTable("TrainingPeriods");
            e.HasKey(x => x.PeriodID);
            e.Property(x => x.PeriodName).HasMaxLength(100).IsRequired();
            e.Property(x => x.IsActive).HasDefaultValue(true);
        });

        // ─── Internships ────────────────────────────────────────────────
        mb.Entity<Internship>(e =>
        {
            e.ToTable("Internships");
            e.HasKey(x => x.InternshipID);
            e.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("active");

            e.HasOne(x => x.Student)
             .WithMany()
             .HasForeignKey(x => x.StudentID)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Advisor)
             .WithMany()
             .HasForeignKey(x => x.AdvisorID)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Company)
             .WithMany(c => c.Internships)
             .HasForeignKey(x => x.CompanyID)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.TrainingPeriod)
             .WithMany(p => p.Internships)
             .HasForeignKey(x => x.PeriodID)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── Attendances ────────────────────────────────────────────────
        mb.Entity<Attendance>(e =>
        {
            e.ToTable("Attendances");
            e.HasKey(x => x.AttendanceID);
            e.HasIndex(x => new { x.StudentID, x.AttDate }).IsUnique(); // UNIQUE(StudentID, AttDate)
            e.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("ontime");
            e.Property(x => x.Note).HasMaxLength(500);
            e.Property(x => x.TotalHours).HasColumnType("decimal(5,2)");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            e.HasOne(x => x.Student)
             .WithMany()
             .HasForeignKey(x => x.StudentID)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── LeaveTypes ─────────────────────────────────────────────────
        mb.Entity<LeaveType>(e =>
        {
            e.ToTable("LeaveTypes");
            e.HasKey(x => x.LeaveTypeID);
            e.Property(x => x.TypeName).HasMaxLength(50).IsRequired();
        });

        // ─── Leaves ─────────────────────────────────────────────────────
        mb.Entity<Leave>(e =>
        {
            e.ToTable("Leaves");
            e.HasKey(x => x.LeaveID);
            e.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("pending");
            e.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
            e.Property(x => x.ReviewNote).HasMaxLength(500);
            e.Property(x => x.AttachmentPath).HasMaxLength(500);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            e.HasOne(x => x.Student)
             .WithMany()
             .HasForeignKey(x => x.StudentID)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.LeaveType)
             .WithMany(lt => lt.Leaves)
             .HasForeignKey(x => x.LeaveTypeID)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Internship)
             .WithMany(i => i.Leaves)
             .HasForeignKey(x => x.InternshipID)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── WorkCategories ─────────────────────────────────────────────
        mb.Entity<WorkCategory>(e =>
        {
            e.ToTable("WorkCategories");
            e.HasKey(x => x.CategoryID);
            e.Property(x => x.CategoryName).HasMaxLength(100).IsRequired();
        });

        // ─── WorkLogs ───────────────────────────────────────────────────
        mb.Entity<WorkLog>(e =>
        {
            e.ToTable("WorkLogs");
            e.HasKey(x => x.LogID);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Hours).HasColumnType("decimal(5,2)"); // Backend computed
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            e.HasOne(x => x.Student)
             .WithMany()
             .HasForeignKey(x => x.StudentID)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Category)
             .WithMany(c => c.WorkLogs)
             .HasForeignKey(x => x.CategoryID)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── Attachments ────────────────────────────────────────────────
        mb.Entity<Attachment>(e =>
        {
            e.ToTable("Attachments");
            e.HasKey(x => x.AttachmentID);
            e.Property(x => x.FileName).HasMaxLength(255).IsRequired();
            e.Property(x => x.FilePath).HasMaxLength(500).IsRequired();
            e.Property(x => x.UploadedAt).HasDefaultValueSql("GETDATE()");

            e.HasOne(x => x.WorkLog)
             .WithMany(w => w.Attachments)
             .HasForeignKey(x => x.LogID)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── LogComments ────────────────────────────────────────────────
        mb.Entity<LogComment>(e =>
        {
            e.ToTable("LogComments");
            e.HasKey(x => x.CommentID);
            e.Property(x => x.Comment).IsRequired();
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            e.HasOne(x => x.WorkLog)
             .WithMany(w => w.LogComments)
             .HasForeignKey(x => x.LogID)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.User)
             .WithMany(u => u.LogComments)
             .HasForeignKey(x => x.UserID)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── Tasks ──────────────────────────────────────────────────────
        mb.Entity<TaskItem>(e =>
        {
            e.ToTable("Tasks");
            e.HasKey(x => x.TaskID);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Priority).HasMaxLength(10).HasDefaultValue("medium");
            e.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("todo");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            // FK: Tasks.InternshipID → Internships (มติ #5)
            e.HasOne(x => x.Internship)
             .WithMany(i => i.Tasks)
             .HasForeignKey(x => x.InternshipID)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Assigner)
             .WithMany()
             .HasForeignKey(x => x.AssignedBy)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── AdvisorVisits ──────────────────────────────────────────────
        mb.Entity<AdvisorVisit>(e =>
        {
            e.ToTable("AdvisorVisits");
            e.HasKey(x => x.VisitID);
            // UNIQUE(InternshipID, VisitNo) — UQ_Visit_No ตามมติ #3
            e.HasIndex(x => new { x.InternshipID, x.VisitNo })
             .IsUnique()
             .HasDatabaseName("UQ_Visit_No");
            e.Property(x => x.VisitType).HasMaxLength(20).HasDefaultValue("Onsite");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            e.HasOne(x => x.Internship)
             .WithMany(i => i.AdvisorVisits)
             .HasForeignKey(x => x.InternshipID)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Advisor)
             .WithMany()
             .HasForeignKey(x => x.AdvisorID)
             .OnDelete(DeleteBehavior.Restrict);
        });


        // ─── Notifications ──────────────────────────────────────────────
        mb.Entity<Notification>(e =>
        {
            e.ToTable("Notifications");
            e.HasKey(x => x.NotificationID);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Link).HasMaxLength(300);
            e.Property(x => x.IsRead).HasDefaultValue(false);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            e.HasOne(x => x.User)
             .WithMany(u => u.Notifications)
             .HasForeignKey(x => x.UserID)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── Reports ────────────────────────────────────────────────────
        mb.Entity<Report>(e =>
        {
            e.ToTable("Reports");
            e.HasKey(x => x.ReportID);
            e.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("draft");
            e.Property(x => x.PeriodLabel).HasMaxLength(100).IsRequired();
            e.Property(x => x.FilePath).HasMaxLength(500);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            e.HasOne(x => x.Student)
             .WithMany()
             .HasForeignKey(x => x.StudentID)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Internship)
             .WithMany(i => i.Reports)
             .HasForeignKey(x => x.InternshipID)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── Evaluations ────────────────────────────────────────────────
        mb.Entity<Evaluation>(e =>
        {
            e.ToTable("Evaluations");
            e.HasKey(x => x.EvaluationID);
            e.Property(x => x.EvaluatorRole).HasMaxLength(20).IsRequired();
            e.Property(x => x.Grade).HasMaxLength(2);
            e.Property(x => x.TotalScore).HasColumnType("decimal(5,2)");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            e.HasOne(x => x.Internship)
             .WithMany(i => i.Evaluations)
             .HasForeignKey(x => x.InternshipID)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── RefreshTokens (มติ #4) ─────────────────────────────────────
        mb.Entity<RefreshToken>(e =>
        {
            e.ToTable("RefreshTokens");
            e.HasKey(x => x.RefreshTokenID);
            e.Property(x => x.TokenHash).HasMaxLength(255).IsRequired();
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");
            e.HasIndex(x => x.UserID).HasDatabaseName("IX_RefreshTokens_User");

            e.HasOne(x => x.User)
             .WithMany(u => u.RefreshTokens)
             .HasForeignKey(x => x.UserID)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── PasswordResetTokens (มติ #7) ───────────────────────────────
        mb.Entity<PasswordResetToken>(e =>
        {
            e.ToTable("PasswordResetTokens");
            e.HasKey(x => x.TokenID);
            e.Property(x => x.TokenHash).HasMaxLength(255).IsRequired();
            e.Property(x => x.IsUsed).HasDefaultValue(false);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");
            e.HasIndex(x => x.UserID).HasDatabaseName("IX_PasswordResetTokens_User");
            e.HasIndex(x => x.TokenHash).HasDatabaseName("IX_PasswordResetTokens_Hash");

            e.HasOne(x => x.User)
             .WithMany(u => u.PasswordResetTokens)
             .HasForeignKey(x => x.UserID)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ===== Seed Data =====
        SeedData(mb);
    }

    private static void SeedData(ModelBuilder mb)
    {
        // Roles (4 แถว)
        mb.Entity<Role>().HasData(
            new Role { RoleID = 1, RoleName = "student",    Description = "นักศึกษาฝึกงาน" },
            new Role { RoleID = 2, RoleName = "advisor",    Description = "อาจารย์นิเทศก์" },
            new Role { RoleID = 3, RoleName = "supervisor", Description = "พี่เลี้ยงบริษัท" },
            new Role { RoleID = 4, RoleName = "admin",      Description = "ผู้ดูแลระบบ" }
        );

        // LeaveTypes (3 แถว)
        mb.Entity<LeaveType>().HasData(
            new LeaveType { LeaveTypeID = 1, TypeName = "ลาป่วย",    MaxDays = 30 },
            new LeaveType { LeaveTypeID = 2, TypeName = "ลากิจ",     MaxDays = 3 },
            new LeaveType { LeaveTypeID = 3, TypeName = "ลาพักร้อน", MaxDays = 6 }
        );

        // WorkCategories (5 แถว)
        mb.Entity<WorkCategory>().HasData(
            new WorkCategory { CategoryID = 1, CategoryName = "Development" },
            new WorkCategory { CategoryID = 2, CategoryName = "Meeting" },
            new WorkCategory { CategoryID = 3, CategoryName = "Documentation" },
            new WorkCategory { CategoryID = 4, CategoryName = "Testing" },
            new WorkCategory { CategoryID = 5, CategoryName = "อื่นๆ" }
        );

        // Admin user (1 คน) — BCrypt hash ของ "Admin@1234!"
        mb.Entity<User>().HasData(new User
        {
            UserID       = 1,
            RoleID       = 4, // admin
            Email        = "admin@internlog.dev",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@1234!"),
            FirstName    = "System",
            LastName     = "Admin",
            IsActive     = true,
            CreatedAt    = new DateTime(2026, 8, 14)
        });
    }
}
