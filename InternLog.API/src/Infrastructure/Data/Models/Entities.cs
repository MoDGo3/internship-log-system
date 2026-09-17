// ===================================================
// Entity Models — ตรงกับ ERD 20 ตาราง (ห้ามเปลี่ยนชื่อ)
// Namespace: InternLog.API.Infrastructure.Data.Models
// ===================================================

namespace InternLog.API.Infrastructure.Data.Models;

// ─── ตารางที่ 1: Roles ───────────────────────────────────
public class Role
{
    public int RoleID { get; set; }
    public string RoleName { get; set; } = string.Empty; // student|advisor|supervisor|admin
    public string? Description { get; set; }

    // Navigation
    public ICollection<User> Users { get; set; } = [];
}

// ─── ตารางที่ 2: Users ───────────────────────────────────
public class User
{
    public int UserID { get; set; }
    public int RoleID { get; set; }
    public string Email { get; set; } = string.Empty;      // UNIQUE
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? StudentID { get; set; }
    public string? Phone { get; set; }
    public string? ProfileImage { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public Role Role { get; set; } = null!;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
    public ICollection<LogComment> LogComments { get; set; } = [];
}

// ─── ตารางที่ 3: Companies ───────────────────────────────
public class Company
{
    public int CompanyID { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public TimeOnly WorkStartTime { get; set; } = new TimeOnly(9, 0);  // 09:00 default
    public TimeOnly WorkEndTime { get; set; } = new TimeOnly(18, 0);   // 18:00 default
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public ICollection<Internship> Internships { get; set; } = [];
}

// ─── ตารางที่ 4: TrainingPeriods ─────────────────────────
public class TrainingPeriod
{
    public int PeriodID { get; set; }
    public string PeriodName { get; set; } = string.Empty; // เช่น "ภาคเรียนที่ 1/2569"
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Internship> Internships { get; set; } = [];
}

// ─── ตารางที่ 5: Internships (ตารางแกนกลาง) ─────────────
public class Internship
{
    public int InternshipID { get; set; }
    public int StudentID { get; set; }
    public int CompanyID { get; set; }
    public int AdvisorID { get; set; }
    public int PeriodID { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int RequiredDays { get; set; }
    public int RequiredHours { get; set; }
    public string Status { get; set; } = "active"; // active|completed

    // Navigation
    public User Student { get; set; } = null!;
    public User Advisor { get; set; } = null!;
    public Company Company { get; set; } = null!;
    public TrainingPeriod TrainingPeriod { get; set; } = null!;
    public ICollection<Leave> Leaves { get; set; } = [];
    public ICollection<TaskItem> Tasks { get; set; } = [];
    public ICollection<AdvisorVisit> AdvisorVisits { get; set; } = [];
    public ICollection<Report> Reports { get; set; } = [];
    public ICollection<Evaluation> Evaluations { get; set; } = [];
}

// ─── ตารางที่ 6: Attendances ─────────────────────────────
public class Attendance
{
    public int AttendanceID { get; set; }
    public int StudentID { get; set; }
    public DateOnly AttDate { get; set; }              // UNIQUE (StudentID, AttDate)
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    public TimeOnly? RequestedCheckInTime { get; set; } // สำหรับขอแก้ไขเวลาเข้า
    public TimeOnly? RequestedCheckOutTime { get; set; } // สำหรับขอแก้ไขเวลาออก
    public decimal? TotalHours { get; set; }
    public string Status { get; set; } = "ontime"; // ontime|late|leave|absent|pending_edit
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public User Student { get; set; } = null!;
}

// ─── ตารางที่ 7: LeaveTypes ──────────────────────────────
public class LeaveType
{
    public int LeaveTypeID { get; set; }
    public string TypeName { get; set; } = string.Empty; // ป่วย|กิจ|พักร้อน
    public int MaxDays { get; set; }

    // Navigation
    public ICollection<Leave> Leaves { get; set; } = [];
}

// ─── ตารางที่ 8: Leaves ──────────────────────────────────
public class Leave
{
    public int LeaveID { get; set; }
    public int StudentID { get; set; }
    public int LeaveTypeID { get; set; }
    public int InternshipID { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal DayCount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? AttachmentPath { get; set; }
    public string Status { get; set; } = "pending"; // pending|approved|rejected
    public string? ReviewNote { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public User Student { get; set; } = null!;
    public LeaveType LeaveType { get; set; } = null!;
    public Internship Internship { get; set; } = null!;
}

// ─── ตารางที่ 9: WorkCategories ──────────────────────────
public class WorkCategory
{
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty; // Development|Meeting|Doc|Testing|อื่นๆ

    // Navigation
    public ICollection<WorkLog> WorkLogs { get; set; } = [];
}

// ─── ตารางที่ 10: WorkLogs ───────────────────────────────
public class WorkLog
{
    public int LogID { get; set; }
    public int StudentID { get; set; }
    public int CategoryID { get; set; }
    public DateOnly LogDate { get; set; }
    public decimal? Hours { get; set; }     // Backend computed จาก Attendances (ส่วนที่ 4 มติ #2)
    public string Title { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public string? Problem { get; set; }
    public string? Solution { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public User Student { get; set; } = null!;
    public WorkCategory Category { get; set; } = null!;
    public ICollection<Attachment> Attachments { get; set; } = [];
    public ICollection<LogComment> LogComments { get; set; } = [];
}

// ─── ตารางที่ 11: Attachments ────────────────────────────
public class Attachment
{
    public int AttachmentID { get; set; }
    public int LogID { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.Now;

    // Navigation
    public WorkLog WorkLog { get; set; } = null!;
}

// ─── ตารางที่ 12: LogComments ────────────────────────────
public class LogComment
{
    public int CommentID { get; set; }
    public int LogID { get; set; }
    public int UserID { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public WorkLog WorkLog { get; set; } = null!;
    public User User { get; set; } = null!;
}

// ─── ตารางที่ 13: Tasks (InternshipID FK — มติ #5) ───────
public class TaskItem
{
    public int TaskID { get; set; }
    public int InternshipID { get; set; }   // FK→Internships (แทน InternID เดิม)
    public int AssignedBy { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public DateOnly? DueDate { get; set; }
    public string Priority { get; set; } = "medium"; // low|medium|high
    public string Status { get; set; } = "todo";     // todo|doing|done
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public Internship Internship { get; set; } = null!;
    public User Assigner { get; set; } = null!;
}

// ─── ตารางที่ 14: AdvisorVisits ──────────────────────────
public class AdvisorVisit
{
    public int VisitID { get; set; }
    public int InternshipID { get; set; }
    public int PersonNo { get; set; }   // ลำดับนศ.ในกลุ่มนิเทศ
    public int VisitNo { get; set; }    // ครั้งที่นิเทศ — UNIQUE(InternshipID, VisitNo)
    public DateOnly VisitDate { get; set; }
    public int AdvisorID { get; set; }
    public string VisitType { get; set; } = "Onsite"; // Onsite|Online
    public string? Summary { get; set; }
    public int? Score { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public Internship Internship { get; set; } = null!;
    public User Advisor { get; set; } = null!;
}


// ─── ตารางที่ 16: Notifications ──────────────────────────
public class Notification
{
    public int NotificationID { get; set; }
    public int UserID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Link { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public User User { get; set; } = null!;
}

// ─── ตารางที่ 17: Reports ────────────────────────────────
public class Report
{
    public int ReportID { get; set; }
    public int StudentID { get; set; }
    public int InternshipID { get; set; }
    public string PeriodLabel { get; set; } = string.Empty; // เช่น "สัปดาห์ที่ 1"
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = "draft"; // draft|submitted|approved|returned
    public string? ReviewNote { get; set; }
    public string? FilePath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public User Student { get; set; } = null!;
    public Internship Internship { get; set; } = null!;
}

// ─── ตารางที่ 18: Evaluations ────────────────────────────
public class Evaluation
{
    public int EvaluationID { get; set; }
    public int InternshipID { get; set; }
    public string EvaluatorRole { get; set; } = string.Empty; // supervisor|student
    public string? CriteriaJson { get; set; }  // JSON array ของ criteria + scores
    public decimal? TotalScore { get; set; }
    public string? Grade { get; set; }          // A|B|C|D|F
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public Internship Internship { get; set; } = null!;
}

// ─── ตารางที่ 19: RefreshTokens (มติ #4) ─────────────────
public class RefreshToken
{
    public int RefreshTokenID { get; set; }
    public int UserID { get; set; }
    public string TokenHash { get; set; } = string.Empty;  // เก็บ hash เท่านั้น
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public int? ReplacedByTokenID { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public User User { get; set; } = null!;

    // Helper
    public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
}

// ─── ตารางที่ 20: PasswordResetTokens (มติ #6-7 ส่วนที่ 7) ──
public class PasswordResetToken
{
    public int TokenID { get; set; }
    public int UserID { get; set; }
    public string TokenHash { get; set; } = string.Empty;  // เก็บ hash เท่านั้น
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public User User { get; set; } = null!;

    // Helper
    public bool IsValid => !IsUsed && DateTime.UtcNow < ExpiresAt;
}
