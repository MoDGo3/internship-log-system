using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using InternLog.API.Modules.WorkLogs.DTOs;
using InternLog.API.Core.Interfaces;
using InternLog.API.Core.Common;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Infrastructure.Data.Models;
using System.IO;

namespace InternLog.API.Modules.WorkLogs;

public class WorkLogService : IWorkLogService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IWebHostEnvironment _env;

    public WorkLogService(AppDbContext context, ICurrentUserService currentUser, IWebHostEnvironment env)
    {
        _context = context;
        _currentUser = currentUser;
        _env = env;
    }

    private async Task<decimal?> GetComputedHoursAsync(int studentId, DateOnly logDate)
    {
        // Compute hours from Attendances table where Status is 'ontime' or 'late'
        var attendance = await _context.Attendances
            .FirstOrDefaultAsync(a => a.StudentID == studentId && a.AttDate == logDate && (a.Status == "ontime" || a.Status == "late"));
        
        return attendance?.TotalHours;
    }

    public async Task<IEnumerable<WorkLogResponseDto>> GetStudentLogsAsync()
    {
        var studentId = _currentUser.UserId;
        var logs = await _context.WorkLogs
            .Include(l => l.Student)
            .Include(l => l.Category)
            .Include(l => l.Attachments)
            .Include(l => l.LogComments).ThenInclude(c => c.User)
            .Where(l => l.StudentID == studentId)
            .OrderByDescending(l => l.LogDate)
            .ToListAsync();

        return logs.Select(MapToDto);
    }

    public async Task<IEnumerable<WorkLogResponseDto>> GetPendingLogsAsync()
    {
        // Currently returns all submitted logs. Can be filtered by Advisor in future.
        var logs = await _context.WorkLogs
            .Include(l => l.Student)
            .Include(l => l.Category)
            .Include(l => l.Attachments)
            .Include(l => l.LogComments).ThenInclude(c => c.User)
            .Where(l => l.Status == "submitted")
            .OrderBy(l => l.LogDate)
            .ToListAsync();

        return logs.Select(MapToDto);
    }

    public async Task<WorkLogResponseDto> GetLogDetailAsync(int logId)
    {
        var log = await _context.WorkLogs
            .Include(l => l.Student)
            .Include(l => l.Category)
            .Include(l => l.Attachments)
            .Include(l => l.LogComments).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(l => l.LogID == logId);

        if (log == null)
            throw AppException.NotFound("ไม่พบบันทึกการทำงานนี้");

        // Permission check: if student, can only see own logs
        if (_currentUser.Role == "student" && log.StudentID != _currentUser.UserId)
            throw AppException.Forbidden("คุณไม่มีสิทธิ์เข้าถึงบันทึกการทำงานของผู้อื่น");

        return MapToDto(log);
    }

    public async Task<WorkLogResponseDto> CreateLogAsync(WorkLogRequestDto request, bool isSubmit)
    {
        var studentId = _currentUser.UserId;

        // Check if log already exists for this date and category (Optional business rule, we allow multiple logs per day for now, but usually it's one per day)
        
        var category = await _context.WorkCategories.FindAsync(request.CategoryID);
        if (category == null)
            throw AppException.NotFound("ไม่พบหมวดหมู่งานนี้");

        decimal? computedHours = await GetComputedHoursAsync(studentId, request.LogDate);

        var log = new WorkLog
        {
            StudentID = studentId,
            CategoryID = request.CategoryID,
            LogDate = request.LogDate,
            Hours = computedHours,
            Title = request.Title,
            Detail = request.Detail,
            Problem = request.Problem,
            Solution = request.Solution,
            Status = isSubmit ? "submitted" : "draft",
            CreatedAt = DateTime.Now
        };

        _context.WorkLogs.Add(log);
        await _context.SaveChangesAsync();

        // Load navigations for DTO mapping
        log.Student = await _context.Users.FirstAsync(u => u.UserID == studentId);
        log.Category = category;

        return MapToDto(log);
    }

    public async Task<WorkLogResponseDto> UpdateLogAsync(int logId, WorkLogRequestDto request)
    {
        var studentId = _currentUser.UserId;
        var log = await _context.WorkLogs
            .Include(l => l.Student)
            .Include(l => l.Category)
            .Include(l => l.Attachments)
            .Include(l => l.LogComments).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(l => l.LogID == logId);

        if (log == null)
            throw AppException.NotFound("ไม่พบบันทึกการทำงานนี้");

        if (log.StudentID != studentId)
            throw AppException.Forbidden("แก้ไขได้เฉพาะบันทึกการทำงานของตนเองเท่านั้น");

        if (log.Status != "draft" && log.Status != "returned")
            throw AppException.BadRequest($"ไม่สามารถแก้ไขได้ เนื่องจากสถานะปัจจุบันคือ {log.Status}");

        var category = await _context.WorkCategories.FindAsync(request.CategoryID);
        if (category == null)
            throw AppException.NotFound("ไม่พบหมวดหมู่งานนี้");

        log.CategoryID = request.CategoryID;
        log.Category = category;
        log.LogDate = request.LogDate;
        log.Hours = await GetComputedHoursAsync(studentId, request.LogDate);
        log.Title = request.Title;
        log.Detail = request.Detail;
        log.Problem = request.Problem;
        log.Solution = request.Solution;
        log.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return MapToDto(log);
    }

    public async Task<WorkLogResponseDto> SubmitLogAsync(int logId)
    {
        var studentId = _currentUser.UserId;
        var log = await _context.WorkLogs
            .Include(l => l.Student)
            .Include(l => l.Category)
            .Include(l => l.Attachments)
            .Include(l => l.LogComments).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(l => l.LogID == logId);

        if (log == null)
            throw AppException.NotFound("ไม่พบบันทึกการทำงานนี้");

        if (log.StudentID != studentId)
            throw AppException.Forbidden("ดำเนินการได้เฉพาะบันทึกการทำงานของตนเองเท่านั้น");

        if (log.Status != "draft" && log.Status != "returned")
            throw AppException.BadRequest($"ไม่สามารถส่งตรวจได้ เนื่องจากสถานะคือ {log.Status}");

        log.Status = "submitted";
        log.UpdatedAt = DateTime.Now;

        // Re-compute hours just in case attendance was updated
        log.Hours = await GetComputedHoursAsync(studentId, log.LogDate);

        await _context.SaveChangesAsync();

        return MapToDto(log);
    }

    public async Task<WorkLogResponseDto> ReviewLogAsync(int logId, ReviewLogDto reviewDto)
    {
        var reviewerId = _currentUser.UserId;
        var log = await _context.WorkLogs
            .Include(l => l.Student)
            .Include(l => l.Category)
            .Include(l => l.Attachments)
            .Include(l => l.LogComments).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(l => l.LogID == logId);

        if (log == null)
            throw AppException.NotFound("ไม่พบบันทึกการทำงานนี้");

        if (log.Status != "submitted")
            throw AppException.BadRequest($"ไม่สามารถตรวจได้ เนื่องจากสถานะคือ {log.Status}");

        log.Status = reviewDto.IsApproved ? "approved" : "returned";
        log.UpdatedAt = DateTime.Now;

        if (!string.IsNullOrWhiteSpace(reviewDto.Comment))
        {
            var comment = new LogComment
            {
                LogID = log.LogID,
                UserID = reviewerId,
                Comment = reviewDto.Comment ?? string.Empty,
                CreatedAt = DateTime.Now
            };
            _context.LogComments.Add(comment);
        }

        await _context.SaveChangesAsync();

        // Reload log comments to include the newly added one
        await _context.Entry(log).Collection(l => l.LogComments).Query().Include(c => c.User).LoadAsync();

        return MapToDto(log);
    }

    public async Task<AttachmentResponseDto> UploadAttachmentAsync(int logId, IFormFile file)
    {
        var studentId = _currentUser.UserId;
        var log = await _context.WorkLogs.FindAsync(logId);

        if (log == null)
            throw AppException.NotFound("ไม่พบบันทึกการทำงานนี้");

        if (log.StudentID != studentId)
            throw AppException.Forbidden("อัปโหลดไฟล์ได้เฉพาะบันทึกการทำงานของตนเองเท่านั้น");

        if (log.Status != "draft" && log.Status != "returned")
            throw AppException.BadRequest("ไม่สามารถอัปโหลดไฟล์ได้ในสถานะปัจจุบัน");

        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "worklogs");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        var attachment = new Attachment
        {
            LogID = logId,
            FileName = file.FileName,
            FilePath = $"/uploads/worklogs/{uniqueFileName}",
            FileSize = file.Length,
            UploadedAt = DateTime.Now
        };

        _context.Attachments.Add(attachment);
        await _context.SaveChangesAsync();

        return new AttachmentResponseDto(
            attachment.AttachmentID,
            attachment.FileName,
            attachment.FilePath,
            attachment.FileSize,
            attachment.UploadedAt
        );
    }

    public async Task DeleteAttachmentAsync(int attachmentId)
    {
        var studentId = _currentUser.UserId;
        var attachment = await _context.Attachments
            .Include(a => a.WorkLog)
            .FirstOrDefaultAsync(a => a.AttachmentID == attachmentId);

        if (attachment == null)
            throw AppException.NotFound("ไม่พบไฟล์แนบนี้");

        if (attachment.WorkLog.StudentID != studentId)
            throw AppException.Forbidden("สามารถลบได้เฉพาะไฟล์ของตนเองเท่านั้น");

        if (attachment.WorkLog.Status != "draft" && attachment.WorkLog.Status != "returned")
            throw AppException.BadRequest("ไม่สามารถลบไฟล์ได้ในสถานะปัจจุบัน");

        // Optionally delete the physical file
        var physicalPath = Path.Combine(_env.WebRootPath, attachment.FilePath.TrimStart('/'));
        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }

        _context.Attachments.Remove(attachment);
        await _context.SaveChangesAsync();
    }

    public async Task<CommentResponseDto> AddCommentAsync(int logId, CommentRequestDto commentDto)
    {
        var userId = _currentUser.UserId;
        var log = await _context.WorkLogs.FindAsync(logId);

        if (log == null)
            throw AppException.NotFound("ไม่พบบันทึกการทำงานนี้");

        var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserID == userId);
        if (user == null)
            throw AppException.NotFound("ไม่พบผู้ใช้งาน");

        var comment = new LogComment
        {
            LogID = logId,
            UserID = userId,
            Comment = commentDto.Message,
            CreatedAt = DateTime.Now
        };

        _context.LogComments.Add(comment);
        await _context.SaveChangesAsync();

        return new CommentResponseDto(
            comment.CommentID,
            user.UserID,
            $"{user.FirstName} {user.LastName}",
            user.Role.RoleName,
            comment.Comment,
            comment.CreatedAt
        );
    }

    private static WorkLogResponseDto MapToDto(WorkLog log)
    {
        var attachments = log.Attachments?.Select(a => new AttachmentResponseDto(
            a.AttachmentID,
            a.FileName,
            a.FilePath,
            a.FileSize,
            a.UploadedAt
        )).ToList() ?? new List<AttachmentResponseDto>();

        var comments = log.LogComments?.Select(c => new CommentResponseDto(
            c.CommentID,
            c.UserID,
            $"{c.User.FirstName} {c.User.LastName}",
            c.User.RoleID.ToString(), // Role Name resolution could be improved
            c.Comment,
            c.CreatedAt
        )).ToList() ?? new List<CommentResponseDto>();

        return new WorkLogResponseDto(
            log.LogID,
            log.StudentID,
            log.Student != null ? $"{log.Student.FirstName} {log.Student.LastName}" : string.Empty,
            log.CategoryID,
            log.Category != null ? log.Category.CategoryName : string.Empty,
            log.LogDate,
            log.Hours,
            log.Title,
            log.Detail,
            log.Problem,
            log.Solution,
            log.Status,
            log.CreatedAt,
            log.UpdatedAt,
            attachments,
            comments
        );
    }
}
