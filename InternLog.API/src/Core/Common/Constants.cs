namespace InternLog.API.Core.Common;

/// <summary>
/// ค่าคงที่ทั้งระบบ — ห้ามเปลี่ยนชื่อ (ตาม ERD)
/// </summary>
public static class Constants
{
    // ===== Roles =====
    public static class Roles
    {
        public const string Student    = "student";
        public const string Advisor    = "advisor";
        public const string Admin      = "admin";
    }

    // ===== Attendance Status =====
    public static class AttendanceStatus
    {
        public const string OnTime      = "ontime";
        public const string Late        = "late";
        public const string Leave       = "leave";
        public const string Absent      = "absent";
        public const string PendingEdit = "pending_edit";
    }

    // ===== Leave Status =====
    public static class LeaveStatus
    {
        public const string Pending  = "pending";
        public const string Approved = "approved";
        public const string Rejected = "rejected";
    }

    // ===== WorkLog Status =====
    public static class WorkLogStatus
    {
        public const string Draft     = "draft";
        public const string Submitted = "submitted";
        public const string Approved  = "approved";
        public const string Returned  = "returned";
    }

    // ===== Task Status =====
    public static class TaskStatus
    {
        public const string Todo  = "todo";
        public const string Doing = "doing";
        public const string Done  = "done";
    }

    // ===== Task Priority =====
    public static class TaskPriority
    {
        public const string Low    = "low";
        public const string Medium = "medium";
        public const string High   = "high";
    }

    // ===== Internship Status =====
    public static class InternshipStatus
    {
        public const string Active    = "active";
        public const string Completed = "completed";
    }

    // ===== Report Status =====
    public static class ReportStatus
    {
        public const string Draft     = "draft";
        public const string Submitted = "submitted";
        public const string Approved  = "approved";
        public const string Returned  = "returned";
    }

    // ===== JWT =====
    public static class Jwt
    {
        public const int AccessTokenMinutes  = 120;  // 2 ชั่วโมง
        public const int RefreshTokenDays    = 7;    // 7 วัน
        public const string ClaimUserId      = "sub";
        public const string ClaimName        = "name";
        public const string ClaimRole        = "role";
    }

    // ===== Password Reset =====
    public static class PasswordReset
    {
        public const int TokenExpiryHours = 1; // หมดอายุ 1 ชั่วโมง
    }

    // ===== File Upload =====
    public static class FileUpload
    {
        public const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB
        public static readonly string[] AllowedExtensions = [".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx"];
    }
}
