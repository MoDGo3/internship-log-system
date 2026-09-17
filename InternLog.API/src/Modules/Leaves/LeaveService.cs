using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using InternLog.API.Modules.Leaves.DTOs;
using InternLog.API.Core.Interfaces;
using InternLog.API.Core.Common;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Infrastructure.Data.Models;

namespace InternLog.API.Modules.Leaves;

public class LeaveService : ILeaveService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IWebHostEnvironment _env;

    public LeaveService(AppDbContext context, ICurrentUserService currentUser, IWebHostEnvironment env)
    {
        _context = context;
        _currentUser = currentUser;
        _env = env;
    }

    private static int CalculateWeekdays(DateOnly startDate, DateOnly endDate)
    {
        int days = 0;
        var current = startDate;
        while (current <= endDate)
        {
            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
            {
                days++;
            }
            current = current.AddDays(1);
        }
        return days;
    }

    public async Task<IEnumerable<LeaveQuotaDto>> GetQuotasAsync()
    {
        var studentId = _currentUser.UserId;
        var leaveTypes = await _context.LeaveTypes.ToListAsync();
        
        var leaves = await _context.Leaves
            .Where(l => l.StudentID == studentId && (l.Status == "pending" || l.Status == "approved"))
            .ToListAsync();

        var quotas = new List<LeaveQuotaDto>();
        foreach (var lt in leaveTypes)
        {
            var used = leaves.Where(l => l.LeaveTypeID == lt.LeaveTypeID && l.Status == "approved").Sum(l => l.DayCount);
            var pending = leaves.Where(l => l.LeaveTypeID == lt.LeaveTypeID && l.Status == "pending").Sum(l => l.DayCount);
            var remaining = lt.MaxDays - used - pending;

            quotas.Add(new LeaveQuotaDto(
                lt.LeaveTypeID,
                lt.TypeName,
                lt.MaxDays,
                used,
                pending,
                remaining
            ));
        }

        return quotas;
    }

    public async Task<IEnumerable<LeaveResponseDto>> GetStudentLeavesAsync()
    {
        var studentId = _currentUser.UserId;
        var leaves = await _context.Leaves
            .Include(l => l.Student)
            .Include(l => l.LeaveType)
            .Where(l => l.StudentID == studentId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        return leaves.Select(MapToDto);
    }

    public async Task<IEnumerable<LeaveResponseDto>> GetPendingLeavesAsync()
    {
        var leaves = await _context.Leaves
            .Include(l => l.Student)
            .Include(l => l.LeaveType)
            .Include(l => l.Internship)
            .Where(l => l.Status == "pending" &&
                (_currentUser.Role == "admin" || l.Internship.AdvisorID == _currentUser.UserId))
            .OrderBy(l => l.CreatedAt)
            .ToListAsync();

        return leaves.Select(MapToDto);
    }

    public async Task<LeaveResponseDto> SubmitLeaveAsync(LeaveRequestDto request, IFormFile? attachment)
    {
        var studentId = _currentUser.UserId;

        if (request.EndDate < request.StartDate)
            throw AppException.BadRequest("วันที่สิ้นสุดต้องไม่ก่อนวันที่เริ่มต้น");

        // เปรียบเทียบ DateOnly กับ DateOnly เพื่ออนุญาตให้ลาวันนี้ได้
        var today = DateOnly.FromDateTime(DateTime.Now);
        if (request.StartDate < today)
            throw AppException.BadRequest("ไม่สามารถขอลาย้อนหลังได้");

        // กรอง internship ที่ active เท่านั้น
        var internship = await _context.Internships
            .FirstOrDefaultAsync(i => i.StudentID == studentId && i.Status == "active");

        if (internship == null)
            throw AppException.BadRequest("ไม่พบข้อมูลการฝึกงานที่กำลังดำเนินการอยู่");

        var leaveType = await _context.LeaveTypes.FindAsync(request.LeaveTypeID);
        if (leaveType == null)
            throw AppException.NotFound("ไม่พบประเภทการลานี้");

        // Calculate days (skipping weekends)
        decimal dayCount = CalculateWeekdays(request.StartDate, request.EndDate);
        if (dayCount == 0)
            throw AppException.BadRequest("วันที่เลือกไม่มีวันทำการ (เสาร์-อาทิตย์)");

        // Check quota
        var existingLeaves = await _context.Leaves
            .Where(l => l.StudentID == studentId && l.LeaveTypeID == request.LeaveTypeID && (l.Status == "pending" || l.Status == "approved"))
            .ToListAsync();
            
        decimal usedAndPending = existingLeaves.Sum(l => l.DayCount);
        if (usedAndPending + dayCount > leaveType.MaxDays)
            throw AppException.BadRequest($"สิทธิ์การ{leaveType.TypeName}ของคุณเหลือไม่พอ (เหลือ {leaveType.MaxDays - usedAndPending} วัน)");

        // Handle File Upload
        string? attachmentPath = null;
        if (attachment != null && attachment.Length > 0)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "leaves");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(attachment.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await attachment.CopyToAsync(fileStream);
            }
            attachmentPath = $"/uploads/leaves/{uniqueFileName}";
        }

        var leave = new Leave
        {
            StudentID = studentId,
            LeaveTypeID = request.LeaveTypeID,
            InternshipID = internship.InternshipID,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            DayCount = dayCount,
            Reason = request.Reason,
            AttachmentPath = attachmentPath,
            Status = "pending",
            CreatedAt = DateTime.Now
        };

        _context.Leaves.Add(leave);
        await _context.SaveChangesAsync();

        leave.Student = await _context.Users.FirstAsync(u => u.UserID == studentId);
        leave.LeaveType = leaveType;

        return MapToDto(leave);
    }

    public async Task<LeaveResponseDto> ReviewLeaveAsync(int leaveId, ReviewLeaveDto reviewDto)
    {
        var leave = await _context.Leaves
            .Include(l => l.Student)
            .Include(l => l.LeaveType)
            .Include(l => l.Internship)
            .FirstOrDefaultAsync(l => l.LeaveID == leaveId);

        if (leave == null)
            throw AppException.NotFound("ไม่พบข้อมูลการลา");

        if (leave.Status != "pending")
            throw AppException.BadRequest($"ไม่สามารถรีวิวได้ เนื่องจากสถานะปัจจุบันคือ {leave.Status}");

        if (_currentUser.Role != "admin" && leave.Internship.AdvisorID != _currentUser.UserId)
            throw AppException.Forbidden("คุณไม่มีสิทธิ์รีวิวใบลาของนักศึกษาคนนี้");

        leave.Status = reviewDto.IsApproved ? "approved" : "rejected";
        leave.ReviewNote = reviewDto.ReviewNote;

        await _context.SaveChangesAsync();

        return MapToDto(leave);
    }

    private static LeaveResponseDto MapToDto(Leave leave)
    {
        return new LeaveResponseDto(
            leave.LeaveID,
            leave.StudentID,
            $"{leave.Student.FirstName} {leave.Student.LastName}",
            leave.LeaveTypeID,
            leave.LeaveType.TypeName,
            leave.StartDate,
            leave.EndDate,
            leave.DayCount,
            leave.Reason,
            leave.AttachmentPath,
            leave.Status,
            leave.ReviewNote,
            leave.CreatedAt
        );
    }
}
