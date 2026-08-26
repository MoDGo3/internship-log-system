using InternLog.API.Core.Common;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Modules.Leaves.DTOs;
using InternLog.API.Modules.Leaves.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

namespace InternLog.API.Modules.Leaves.Services;

public class LeaveService : ILeaveService {
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public LeaveService(AppDbContext db, IWebHostEnvironment env) {
        _db = db;
        _env = env;
    }

    public async Task<ApiResponse<IEnumerable<LeaveQuotaResponseDto>>> GetLeaveQuotaAsync(int studentId) {
        var leaveTypes = await _db.LeaveTypes.ToListAsync();
        var leaves = await _db.Leaves
            .Where(l => l.StudentID == studentId && l.Status == "approved")
            .ToListAsync();

        var result = leaveTypes.Select(lt => {
            var used = leaves.Where(l => l.LeaveTypeID == lt.LeaveTypeID).Sum(l => l.TotalDays);
            return new LeaveQuotaResponseDto {
                LeaveTypeID = lt.LeaveTypeID,
                TypeName = lt.TypeName,
                MaxDays = lt.MaxDays,
                UsedDays = used,
                RemainingDays = lt.MaxDays - used
            };
        });
        
        return ApiResponse<IEnumerable<LeaveQuotaResponseDto>>.SuccessResponse(result);
    }

    public async Task<ApiResponse<IEnumerable<LeaveResponseDto>>> GetLeavesAsync(int userId, string role, string? status) {
        var query = _db.Leaves
            .Include(l => l.Student)
            .Include(l => l.LeaveType)
            .Include(l => l.Approver)
            .AsQueryable();

        if (role == "Student") {
            query = query.Where(l => l.StudentID == userId);
        } else if (role == "Advisor") {
            var studentIds = await _db.Internships
                .Where(i => i.AdvisorID == userId && i.Status == "active")
                .Select(i => i.StudentID)
                .ToListAsync();
            query = query.Where(l => studentIds.Contains(l.StudentID));
        }
        
        if (!string.IsNullOrEmpty(status)) {
            query = query.Where(l => l.Status.ToLower() == status.ToLower());
        }

        var results = await query.OrderByDescending(l => l.CreatedAt).ToListAsync();

        var dtos = results.Select(l => new LeaveResponseDto {
            LeaveID = l.LeaveID,
            StudentID = l.StudentID,
            StudentName = $"{l.Student.FirstName} {l.Student.LastName}",
            LeaveTypeID = l.LeaveTypeID,
            LeaveTypeName = l.LeaveType.TypeName,
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            TotalDays = l.TotalDays,
            Reason = l.Reason,
            AttachmentPath = l.AttachmentPath,
            Status = l.Status,
            CreatedAt = l.CreatedAt,
            ApprovedBy = l.ApprovedBy,
            ApproverName = l.Approver != null ? $"{l.Approver.FirstName} {l.Approver.LastName}" : null,
            ApprovedAt = l.ApprovedAt
        });
        
        return ApiResponse<IEnumerable<LeaveResponseDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<LeaveResponseDto>> CreateLeaveAsync(int studentId, LeaveCreateRequestDto request) {
        if (request.StartDate > request.EndDate)
            return ApiResponse<LeaveResponseDto>.ErrorResponse("StartDate cannot be later than EndDate");

        // Calculate Days (skip weekends for simplicity)
        int workDays = 0;
        for (var date = request.StartDate.Date; date <= request.EndDate.Date; date = date.AddDays(1)) {
            if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday) {
                workDays++;
            }
        }

        if (workDays == 0) return ApiResponse<LeaveResponseDto>.ErrorResponse("Selected date range does not contain any working days");

        var lt = await _db.LeaveTypes.FindAsync(request.LeaveTypeID);
        if (lt == null) return ApiResponse<LeaveResponseDto>.ErrorResponse("Invalid LeaveType");

        var usedDays = await _db.Leaves
            .Where(l => l.StudentID == studentId && l.LeaveTypeID == request.LeaveTypeID && l.Status != "rejected")
            .SumAsync(l => l.TotalDays);
            
        if (usedDays + workDays > lt.MaxDays)
            return ApiResponse<LeaveResponseDto>.ErrorResponse("โควตาวันลาไม่เพียงพอ");

        string? attachmentPath = null;
        if (request.Attachment != null) {
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "leaves");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
            
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + request.Attachment.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            
            using (var fileStream = new FileStream(filePath, FileMode.Create)) {
                await request.Attachment.CopyToAsync(fileStream);
            }
            attachmentPath = $"/uploads/leaves/{uniqueFileName}";
        }

        var leave = new Leave {
            StudentID = studentId,
            LeaveTypeID = request.LeaveTypeID,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalDays = workDays,
            Reason = request.Reason,
            AttachmentPath = attachmentPath,
            Status = "pending",
            CreatedAt = DateTime.UtcNow
        };

        _db.Leaves.Add(leave);
        await _db.SaveChangesAsync();

        var created = await _db.Leaves
            .Include(l => l.Student)
            .Include(l => l.LeaveType)
            .FirstAsync(l => l.LeaveID == leave.LeaveID);

        var dto = new LeaveResponseDto {
            LeaveID = created.LeaveID,
            StudentID = created.StudentID,
            StudentName = $"{created.Student.FirstName} {created.Student.LastName}",
            LeaveTypeID = created.LeaveTypeID,
            LeaveTypeName = created.LeaveType.TypeName,
            StartDate = created.StartDate,
            EndDate = created.EndDate,
            TotalDays = created.TotalDays,
            Reason = created.Reason,
            AttachmentPath = created.AttachmentPath,
            Status = created.Status,
            CreatedAt = created.CreatedAt
        };
        
        return ApiResponse<LeaveResponseDto>.SuccessResponse(dto, "Leave requested successfully.");
    }

    public async Task<ApiResponse<LeaveResponseDto>> ReviewLeaveAsync(int leaveId, int advisorId, LeaveReviewRequestDto request) {
        var leave = await _db.Leaves
            .Include(l => l.Student)
            .Include(l => l.LeaveType)
            .FirstOrDefaultAsync(l => l.LeaveID == leaveId);
            
        if (leave == null) return ApiResponse<LeaveResponseDto>.ErrorResponse("Leave request not found");
        if (leave.Status != "pending") return ApiResponse<LeaveResponseDto>.ErrorResponse("Leave request is already processed");

        leave.Status = request.Status;
        leave.ApprovedBy = advisorId;
        leave.ApprovedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        
        var approver = await _db.Users.FindAsync(advisorId);

        var dto = new LeaveResponseDto {
            LeaveID = leave.LeaveID,
            StudentID = leave.StudentID,
            StudentName = $"{leave.Student.FirstName} {leave.Student.LastName}",
            LeaveTypeID = leave.LeaveTypeID,
            LeaveTypeName = leave.LeaveType.TypeName,
            StartDate = leave.StartDate,
            EndDate = leave.EndDate,
            TotalDays = leave.TotalDays,
            Reason = leave.Reason,
            AttachmentPath = leave.AttachmentPath,
            Status = leave.Status,
            CreatedAt = leave.CreatedAt,
            ApprovedBy = leave.ApprovedBy,
            ApproverName = approver != null ? $"{approver.FirstName} {approver.LastName}" : null,
            ApprovedAt = leave.ApprovedAt
        };
        
        return ApiResponse<LeaveResponseDto>.SuccessResponse(dto, $"Leave request has been {request.Status}.");
    }
}
