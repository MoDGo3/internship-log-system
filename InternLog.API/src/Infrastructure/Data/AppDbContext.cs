using Microsoft.EntityFrameworkCore;
using InternLog.API.Modules.Auth.Models;
using InternLog.API.Modules.Admin.Models;
using InternLog.API.Modules.Attendance.Models;
using InternLog.API.Modules.Leaves.Models;
using InternLog.API.Modules.WorkLogs.Models;
using InternLog.API.Modules.Tasks.Models;
using InternLog.API.Modules.AdvisorVisits.Models;
using InternLog.API.Modules.Messages.Models;
using InternLog.API.Modules.Notifications.Models;
using InternLog.API.Modules.Reports.Models;
using InternLog.API.Modules.Evaluations.Models;

namespace InternLog.API.Infrastructure.Data;
public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
    
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<TrainingPeriod> TrainingPeriods => Set<TrainingPeriod>();
    public DbSet<Internship> Internships => Set<Internship>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<Leave> Leaves => Set<Leave>();
    public DbSet<WorkCategory> WorkCategories => Set<WorkCategory>();
    public DbSet<WorkLog> WorkLogs => Set<WorkLog>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<LogComment> LogComments => Set<LogComment>();
    public DbSet<TaskModel> Tasks => Set<TaskModel>();
    public DbSet<AdvisorVisit> AdvisorVisits => Set<AdvisorVisit>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<Evaluation> Evaluations => Set<Evaluation>();

    protected override void OnModelCreating(ModelBuilder mb) {
        base.OnModelCreating(mb);
        
        mb.Entity<TrainingPeriod>().HasKey(t => t.PeriodID);
        mb.Entity<WorkCategory>().HasKey(c => c.CategoryID);
        mb.Entity<WorkLog>().HasKey(w => w.LogID);
        mb.Entity<LogComment>().HasKey(c => c.CommentID);
        mb.Entity<AdvisorVisit>().HasKey(v => v.VisitID);
        mb.Entity<PasswordResetToken>().HasKey(p => p.TokenID);
        mb.Entity<TaskModel>().HasKey(t => t.TaskID);

        mb.Entity<WorkLog>().Property(w => w.Hours).HasPrecision(4, 2);
        mb.Entity<Attendance>().Property(a => a.TotalHours).HasPrecision(4, 2);
        mb.Entity<Leave>().Property(l => l.TotalDays).HasPrecision(4, 2);
        mb.Entity<AdvisorVisit>().Property(v => v.Score).HasPrecision(5, 2);
        mb.Entity<Evaluation>().Property(e => e.TotalScore).HasPrecision(5, 2);

        mb.Entity<Attendance>().HasIndex(x => new { x.StudentID, x.AttDate }).IsUnique();
        mb.Entity<AdvisorVisit>().HasIndex(x => new { x.InternshipID, x.VisitNo }).IsUnique();
        mb.Entity<WorkLog>().HasIndex(x => new { x.StudentID, x.LogDate });
        mb.Entity<Message>().HasIndex(x => new { x.SenderID, x.ReceiverID });
        mb.Entity<Notification>().HasIndex(x => new { x.UserID, x.IsRead });
        mb.Entity<RefreshToken>().HasIndex(x => x.UserID);
        mb.Entity<PasswordResetToken>().HasIndex(x => x.UserID);
        mb.Entity<PasswordResetToken>().HasIndex(x => x.TokenHash);
        
        mb.Entity<User>().HasOne(u => u.Role).WithMany(r => r.Users).HasForeignKey(u => u.RoleID);
        mb.Entity<Leave>().HasOne(l => l.Approver).WithMany().HasForeignKey(l => l.ApprovedBy).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Report>().HasOne(r => r.Approver).WithMany().HasForeignKey(r => r.ApprovedBy).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<TaskModel>().HasOne(t => t.Assigner).WithMany().HasForeignKey(t => t.AssignedBy).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Internship>().HasOne(i => i.Advisor).WithMany().HasForeignKey(i => i.AdvisorID).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Internship>().HasOne(i => i.Supervisor).WithMany().HasForeignKey(i => i.SupervisorID).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Message>().HasOne(m => m.Sender).WithMany().HasForeignKey(m => m.SenderID).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Message>().HasOne(m => m.Receiver).WithMany().HasForeignKey(m => m.ReceiverID).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<AdvisorVisit>().HasOne(v => v.Advisor).WithMany().HasForeignKey(v => v.AdvisorID).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Evaluation>().HasOne(e => e.Evaluator).WithMany().HasForeignKey(e => e.EvaluatorID).OnDelete(DeleteBehavior.Restrict);

        mb.Entity<Attachment>().HasOne(a => a.WorkLog).WithMany().HasForeignKey(a => a.LogID).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<LogComment>().HasOne(c => c.WorkLog).WithMany().HasForeignKey(c => c.LogID).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<LogComment>().HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserID).OnDelete(DeleteBehavior.Restrict);
    }
}
