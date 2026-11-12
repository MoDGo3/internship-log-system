using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using InternLog.API.Modules.WorkLogs.DTOs;
using InternLog.API.Core.Interfaces;
using InternLog.API.Core.Common;
using InternLog.API.Core.Storage;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Infrastructure.Data.Models;

namespace InternLog.API.Modules.WorkLogs;

public class WorkLogService : IWorkLogService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorage _files;

    private const string FileCategory = "worklogs";
    private const long MaxAttachmentBytes = 5 * 1024 * 1024; // 5 MB

    public WorkLogService(AppDbContext context, ICurrentUserService currentUser, IFileStorage files)
    {
        _context = context;
        _currentUser = currentUser;
        _files = files;
    }

    private async Task<decimal?> GetComputedHoursAsync(int studentId, DateOnly logDate)
    {
        // Compute hours from Attendances table where Status is 'ontime' or 'late'
        var attendance = await _context.Attendances
            .FirstOrDefaultAsync(a => a.StudentID == studentId && a.AttDate == logDate && (a.Status == "ontime" || a.Status == "late"));
        
        return attendance?.TotalHours;
    }

    public async Task<IEnumerable<WorkCategoryDto>> GetCategoriesAsync()
    {
        var categories = await _context.WorkCategories.OrderBy(c => c.CategoryID).ToListAsync();
        return categories.Select(c => new WorkCategoryDto(c.CategoryID, c.CategoryName));
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

    public async Task<IEnumerable<WorkLogResponseDto>> GetAllLogsAsync()
    {
        var logs = await _context.WorkLogs
            .Include(l => l.Student)
            .Include(l => l.Category)
            .Include(l => l.Attachments)
            .Include(l => l.LogComments).ThenInclude(c => c.User)
            .Where(l => _currentUser.Role == Constants.Roles.Admin ||
                l.StudentID == _currentUser.UserId ||
                _context.Internships.Any(i => i.StudentID == l.StudentID && i.AdvisorID == _currentUser.UserId))
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

        // Enforce ownership for student and advisor access; admin is read-only.
        if (_currentUser.Role == Constants.Roles.Student && log.StudentID != _currentUser.UserId)
            throw AppException.Forbidden("คุณไม่มีสิทธิ์เข้าถึงบันทึกการทำงานของผู้อื่น");
        if (_currentUser.Role == Constants.Roles.Advisor &&
            !await _context.Internships.AnyAsync(i => i.StudentID == log.StudentID && i.AdvisorID == _currentUser.UserId))
            throw AppException.Forbidden("คุณไม่มีสิทธิ์เข้าถึงบันทึกการทำงานของนักศึกษาคนนี้");

        return MapToDto(log);
    }

    public async Task<WorkLogResponseDto> CreateLogAsync(WorkLogRequestDto request)
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
            CreatedAt = AppClock.Now
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
        log.UpdatedAt = AppClock.Now;

        await _context.SaveChangesAsync();

        return MapToDto(log);
    }

    public async Task DeleteLogAsync(int logId)
    {
        var studentId = _currentUser.UserId;
        var log = await _context.WorkLogs
            .Include(l => l.Attachments)
            .FirstOrDefaultAsync(l => l.LogID == logId);

        if (log == null)
            throw AppException.NotFound("ไม่พบบันทึกการทำงานนี้");

        if (log.StudentID != studentId)
            throw AppException.Forbidden("ลบได้เฉพาะบันทึกการทำงานของตนเองเท่านั้น");

        var storedPaths = log.Attachments.Select(a => a.FilePath).ToList();

        _context.WorkLogs.Remove(log);
        await _context.SaveChangesAsync();

        // ลบไฟล์จริงหลังลบข้อมูลใน DB สำเร็จแล้ว
        storedPaths.ForEach(_files.Delete);
    }



    public async Task<AttachmentResponseDto> UploadAttachmentAsync(int logId, IFormFile file)
    {
        var studentId = _currentUser.UserId;
        var log = await _context.WorkLogs.FindAsync(logId);

        if (log == null)
            throw AppException.NotFound("ไม่พบบันทึกการทำงานนี้");

        if (log.StudentID != studentId)
            throw AppException.Forbidden("อัปโหลดไฟล์ได้เฉพาะบันทึกการทำงานของตนเองเท่านั้น");

        var storedPath = await _files.SaveUploadAsync(file, FileCategory, MaxAttachmentBytes);

        var attachment = new Attachment
        {
            LogID = logId,
            FileName = Path.GetFileName(file.FileName),
            FilePath = storedPath,
            FileSize = file.Length,
            UploadedAt = AppClock.Now
        };

        _context.Attachments.Add(attachment);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch
        {
            _files.Delete(storedPath); // ไม่ทิ้งไฟล์กำพร้าถ้าบันทึก DB ไม่สำเร็จ
            throw;
        }

        return ToAttachmentDto(attachment);
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


        _context.Attachments.Remove(attachment);
        await _context.SaveChangesAsync();
        _files.Delete(attachment.FilePath);
    }

    public async Task<CommentResponseDto> AddCommentAsync(int logId, CommentRequestDto commentDto)
    {
        var userId = _currentUser.UserId;
        var log = await _context.WorkLogs.FindAsync(logId);

        if (log == null)
            throw AppException.NotFound("ไม่พบบันทึกการทำงานนี้");

        if (!await _context.Internships.AnyAsync(i => i.StudentID == log.StudentID && i.AdvisorID == userId))
            throw AppException.Forbidden("คุณไม่มีสิทธิ์คอมเมนต์บันทึกการทำงานของนักศึกษาคนนี้");

        var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserID == userId);
        if (user == null)
            throw AppException.NotFound("ไม่พบผู้ใช้งาน");

        var comment = new LogComment
        {
            LogID = logId,
            UserID = userId,
            Comment = commentDto.Message,
            CreatedAt = AppClock.Now
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

    public async Task<StoredFile> GetAttachmentAsync(int attachmentId)
    {
        var attachment = await _context.Attachments
            .Include(a => a.WorkLog)
            .FirstOrDefaultAsync(a => a.AttachmentID == attachmentId)
            ?? throw AppException.NotFound("ไม่พบไฟล์แนบนี้");

        var studentId = attachment.WorkLog.StudentID;
        var canAccess = _currentUser.Role switch
        {
            Constants.Roles.Admin   => true,
            Constants.Roles.Student => studentId == _currentUser.UserId,
            Constants.Roles.Advisor => await _context.Internships
                .AnyAsync(i => i.StudentID == studentId && i.AdvisorID == _currentUser.UserId),
            _ => false
        };
        if (!canAccess)
            throw AppException.Forbidden("คุณไม่มีสิทธิ์เข้าถึงไฟล์แนบนี้");

        return _files.Open(attachment.FilePath, attachment.FileName)
            ?? throw AppException.NotFound("ไม่พบไฟล์แนบ");
    }

    /// <summary>FileUrl ชี้ไปที่ endpoint ดาวน์โหลด (ตรวจสิทธิ์) ไม่ใช่ path จริงของไฟล์</summary>
    private static AttachmentResponseDto ToAttachmentDto(Attachment a) => new(
        a.AttachmentID,
        a.FileName,
        $"/api/worklogs/attachments/{a.AttachmentID}",
        a.FileSize,
        a.UploadedAt
    );

    private static WorkLogResponseDto MapToDto(WorkLog log)
    {
        var attachments = log.Attachments?.Select(ToAttachmentDto).ToList() ?? new List<AttachmentResponseDto>();

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
            log.CreatedAt,
            log.UpdatedAt,
            attachments,
            comments
        );
    }
}
