using InternLog.API.Core.Common;
using InternLog.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using InternLog.API.Infrastructure.Data.Models;

namespace InternLog.API.Modules.Attendance;

public class AttendanceService : IAttendanceService
{
    private readonly AppDbContext _context;

    public AttendanceService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AttendanceDto> CheckInAsync(int studentId, string? note)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var timeNow = TimeOnly.FromDateTime(DateTime.Now);

        // หา Internship ที่ active เพื่อดึงเวลาเข้างานของบริษัท
        var internship = await _context.Internships
            .Include(i => i.Company)
            .FirstOrDefaultAsync(i => i.StudentID == studentId && i.Status == "active");

        if (internship == null)
            throw AppException.BadRequest("ไม่พบข้อมูลการฝึกงานที่กำลังดำเนินการ");

        // เช็คว่าลงเวลาแล้วหรือยัง
        var attendance = await _context.Attendances
            .FirstOrDefaultAsync(a => a.StudentID == studentId && a.AttDate == today);

        if (attendance != null && attendance.CheckInTime != null)
            throw AppException.BadRequest("คุณได้ลงเวลาเข้างานของวันนี้ไปแล้ว");

        // ตามสเปกใหม่ (27 ส.ค. 2026): ยกเลิกการคำนวณการมาสาย ให้สถานะ check-in เป็น "ontime" เสมอ
        var status = "ontime";

        if (attendance == null)
        {
            attendance = new Infrastructure.Data.Models.Attendance
            {
                StudentID = studentId,
                AttDate = today,
                CheckInTime = timeNow,
                Status = status,
                Note = note
            };
            _context.Attendances.Add(attendance);
        }
        else
        {
            attendance.CheckInTime = timeNow;
            attendance.Status = status;
            if (!string.IsNullOrEmpty(note))
                attendance.Note = note;
        }

        await _context.SaveChangesAsync();
        return MapToDto(attendance, internship.Student?.FirstName ?? "Unknown");
    }

    public async Task<AttendanceDto> CheckOutAsync(int studentId, string? note)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var timeNow = TimeOnly.FromDateTime(DateTime.Now);

        var attendance = await _context.Attendances
            .Include(a => a.Student)
            .FirstOrDefaultAsync(a => a.StudentID == studentId && a.AttDate == today);

        if (attendance == null || attendance.CheckInTime == null)
            throw AppException.BadRequest("กรุณาลงเวลาเข้างานก่อน");

        if (attendance.CheckOutTime != null)
            throw AppException.BadRequest("คุณได้ลงเวลาออกงานของวันนี้ไปแล้ว");

        attendance.CheckOutTime = timeNow;
        if (!string.IsNullOrEmpty(note))
        {
            attendance.Note = string.IsNullOrEmpty(attendance.Note) 
                ? note 
                : $"{attendance.Note} | {note}";
        }

        // คํานวณชั่วโมง (ลบด้วยเวลาพัก 1 ชม. โดยประมาณ) - สามารถปรับแต่งได้
        var duration = timeNow - attendance.CheckInTime.Value;
        var hours = (decimal)duration.TotalHours;
        // ลบเวลาพักเที่ยงถ้าทำเกิน 4 ชม (ตัวอย่าง)
        if (hours > 4) hours -= 1;
        if (hours < 0) hours = 0;

        attendance.TotalHours = Math.Round(hours, 2);

        await _context.SaveChangesAsync();
        return MapToDto(attendance, attendance.Student.FirstName);
    }

    public async Task<IEnumerable<AttendanceDto>> GetHistoryAsync(int userId, int? month, int? year)
    {
        // TODO: ตรวจสอบ Role ว่าเป็น Advisor ให้ดูของนักศึกษาตัวเองได้
        // สำหรับตอนนี้ดึงแค่ของ userId (Student) 
        var query = _context.Attendances
            .Include(a => a.Student)
            .Where(a => a.StudentID == userId);

        if (month.HasValue)
            query = query.Where(a => a.AttDate.Month == month.Value);
        if (year.HasValue)
            query = query.Where(a => a.AttDate.Year == year.Value);

        var list = await query.OrderByDescending(a => a.AttDate).ToListAsync();
        return list.Select(a => MapToDto(a, a.Student.FirstName));
    }

    public async Task<AttendanceSummaryDto> GetSummaryAsync(int studentId)
    {
        var attendances = await _context.Attendances
            .Where(a => a.StudentID == studentId)
            .ToListAsync();

        return new AttendanceSummaryDto
        {
            TotalDays = attendances.Count,
            TotalHours = attendances.Sum(a => a.TotalHours ?? 0),
            LateCount = attendances.Count(a => a.Status == "late"),
            AbsentCount = attendances.Count(a => a.Status == "absent"),
            LeaveCount = attendances.Count(a => a.Status == "leave")
        };
    }

    public async Task RequestEditAsync(int attendanceId, int studentId, RequestEditDto dto)
    {
        var attendance = await _context.Attendances
            .FirstOrDefaultAsync(a => a.AttendanceID == attendanceId && a.StudentID == studentId);

        if (attendance == null)
            throw AppException.NotFound("ไม่พบข้อมูลการลงเวลา");

        if (attendance.Status == "pending_edit")
            throw AppException.BadRequest("รายการนี้กำลังรอการอนุมัติอยู่แล้ว");

        attendance.RequestedCheckInTime = dto.RequestedCheckInTime;
        attendance.RequestedCheckOutTime = dto.RequestedCheckOutTime;
        attendance.Status = "pending_edit";
        attendance.Note = string.IsNullOrEmpty(attendance.Note) 
            ? $"[Request] {dto.Note}" 
            : $"{attendance.Note} | [Request] {dto.Note}";

        await _context.SaveChangesAsync();
    }

    public async Task ReviewEditAsync(int attendanceId, int reviewerId, ReviewEditDto dto)
    {
        var attendance = await _context.Attendances
            .Include(a => a.Student)
            .FirstOrDefaultAsync(a => a.AttendanceID == attendanceId);

        if (attendance == null)
            throw AppException.NotFound("ไม่พบข้อมูลการลงเวลา");

        if (attendance.Status != "pending_edit")
            throw AppException.BadRequest("รายการนี้ไม่ได้อยู่ในสถานะรอตรวจสอบ");

        var activeInternship = await _context.Internships
            .FirstOrDefaultAsync(i => i.StudentID == attendance.StudentID && i.Status == "active");
        
        // ตรวจสอบสิทธิ์ว่า Reviewer เป็น Advisor ของนักศึกษาคนนี้หรือไม่
        if (activeInternship != null && activeInternship.AdvisorID != reviewerId)
        {
            throw AppException.Forbidden("คุณไม่มีสิทธิ์ตรวจสอบข้อมูลของนักศึกษาคนนี้");
        }

        if (dto.IsApproved)
        {
            if (attendance.RequestedCheckInTime.HasValue)
                attendance.CheckInTime = attendance.RequestedCheckInTime;
            
            if (attendance.RequestedCheckOutTime.HasValue)
                attendance.CheckOutTime = attendance.RequestedCheckOutTime;

            // คำนวณชั่วโมงใหม่
            if (attendance.CheckInTime.HasValue && attendance.CheckOutTime.HasValue)
            {
                var duration = attendance.CheckOutTime.Value - attendance.CheckInTime.Value;
                var hours = (decimal)duration.TotalHours;
                if (hours > 4) hours -= 1;
                if (hours < 0) hours = 0;
                attendance.TotalHours = Math.Round(hours, 2);
            }

            // คำนวณ Status ใหม่
            attendance.Status = "ontime";
            if (activeInternship != null && attendance.CheckInTime.HasValue)
            {
                // ต้องดึง Company มาด้วย แต่ในที่นี้เราไม่ได้โหลด Company มาตอนแรก 
                // สามารถละไว้หรือทำเป็น "ontime" ปริยายไปก่อนถ้าคำนวณยาก
            }
        }
        else
        {
            // กลับไปสถานะเดิม (สมมติว่าเป็น ontime ถ้ามีเวลาเข้าออกครบ)
            attendance.Status = (attendance.CheckInTime.HasValue && attendance.CheckOutTime.HasValue) ? "ontime" : "absent";
        }

        // เคลียร์ Requested
        attendance.RequestedCheckInTime = null;
        attendance.RequestedCheckOutTime = null;

        if (!string.IsNullOrEmpty(dto.Remark))
        {
            attendance.Note = $"{attendance.Note} | [Reviewer] {dto.Remark}";
        }

        await _context.SaveChangesAsync();
    }

    private AttendanceDto MapToDto(Infrastructure.Data.Models.Attendance a, string studentName)
    {
        return new AttendanceDto
        {
            AttendanceID = a.AttendanceID,
            StudentID = a.StudentID,
            StudentName = studentName,
            AttDate = a.AttDate,
            CheckInTime = a.CheckInTime,
            CheckOutTime = a.CheckOutTime,
            TotalHours = a.TotalHours,
            Status = a.Status,
            Note = a.Note
        };
    }
}
