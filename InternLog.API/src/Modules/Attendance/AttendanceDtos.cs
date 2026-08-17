using System.ComponentModel.DataAnnotations;

namespace InternLog.API.Modules.Attendance;

public class AttendanceDto
{
    public int AttendanceID { get; set; }
    public int StudentID { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public DateOnly AttDate { get; set; }
    public TimeOnly? CheckInTime { get; set; }
    public TimeOnly? CheckOutTime { get; set; }
    public decimal? TotalHours { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
}

public class AttendanceSummaryDto
{
    public int TotalDays { get; set; }
    public decimal TotalHours { get; set; }
    public int LateCount { get; set; }
    public int AbsentCount { get; set; }
    public int LeaveCount { get; set; }
}

public class RequestEditDto
{
    public TimeOnly? RequestedCheckInTime { get; set; }
    public TimeOnly? RequestedCheckOutTime { get; set; }
    
    [Required(ErrorMessage = "กรุณาระบุเหตุผลที่ขอแก้ไขเวลา")]
    public string Note { get; set; } = string.Empty;
}

public class ReviewEditDto
{
    [Required]
    public bool IsApproved { get; set; }
    
    // เหตุผลจากอาจารย์/พี่เลี้ยง (บันทึกต่อท้าย Note เดิม)
    public string? Remark { get; set; }
}
