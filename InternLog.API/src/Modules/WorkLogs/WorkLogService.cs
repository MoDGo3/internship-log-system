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
            .Where(l => _currentUser.Role == "admin" ||
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
        if (_currentUser.Role == "student" && log.StudentID != _currentUser.UserId)
            throw AppException.Forbidden("คุณไม่มีสิทธิ์เข้าถึงบันทึกการทำงานของผู้อื่น");
        if (_currentUser.Role == "advisor" &&
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

    public async Task DeleteLogAsync(int logId)
    {
        var studentId = _currentUser.UserId;
        var log = await _context.WorkLogs.FirstOrDefaultAsync(l => l.LogID == logId);

        if (log == null)
            throw AppException.NotFound("ไม่พบบันทึกการทำงานนี้");

        if (log.StudentID != studentId)
            throw AppException.Forbidden("ลบได้เฉพาะบันทึกการทำงานของตนเองเท่านั้น");

        _context.WorkLogs.Remove(log);
        await _context.SaveChangesAsync();
    }


    private bool IsValidFileSignature(IFormFile file, string extension)
    {
        using var reader = new BinaryReader(file.OpenReadStream());
        var signatures = new Dictionary<string, List<byte[]>>
        {
            { ".jpeg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".jpg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".pdf", new List<byte[]> { new byte[] { 0x25, 0x50, 0x44, 0x46 } } },
            { ".doc", new List<byte[]> { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } } },
            { ".docx", new List<byte[]> { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } }
        };

        if (!signatures.ContainsKey(extension)) return false;
        
        var allowedSignatures = signatures[extension];
        var maxHeaderBytes = allowedSignatures.Max(m => m.Length);
        
        if (file.Length < maxHeaderBytes) return false;
        
        var headerBytes = reader.ReadBytes(maxHeaderBytes);
        
        return allowedSignatures.Any(signature => 
            headerBytes.Take(signature.Length).SequenceEqual(signature));
    }

    public async Task<AttachmentResponseDto> UploadAttachmentAsync(int logId, IFormFile file)
    {
        var studentId = _currentUser.UserId;
        var log = await _context.WorkLogs.FindAsync(logId);

        if (log == null)
            throw AppException.NotFound("ไม่พบบันทึกการทำงานนี้");

        if (log.StudentID != studentId)
            throw AppException.Forbidden("อัปโหลดไฟล์ได้เฉพาะบันทึกการทำงานของตนเองเท่านั้น");

        var maxFileSize = 5 * 1024 * 1024; // 5 MB
        if (file.Length > maxFileSize)
            throw AppException.BadRequest("ขนาดไฟล์ต้องไม่เกิน 5 MB");

        var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            throw AppException.BadRequest("รองรับเฉพาะไฟล์ .pdf, .jpg, .png, .doc, .docx เท่านั้น");

        if (!IsValidFileSignature(file, extension))
            throw AppException.BadRequest("ไฟล์ไม่ถูกต้องตามนามสกุล (Invalid magic bytes)");

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
            log.CreatedAt,
            log.UpdatedAt,
            attachments,
            comments
        );
    }
}
