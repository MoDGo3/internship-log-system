using Microsoft.EntityFrameworkCore;
using InternLog.API.Modules.Reports.DTOs;
using InternLog.API.Core.Interfaces;
using InternLog.API.Core.Common;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Infrastructure.Data.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InternLog.API.Modules.Reports;

public class ReportService : IReportService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IWebHostEnvironment _env;

    public ReportService(AppDbContext context, ICurrentUserService currentUser, IWebHostEnvironment env)
    {
        _context = context;
        _currentUser = currentUser;
        _env = env;
    }

    public async Task<IEnumerable<ReportResponseDto>> GetReportsAsync(int internshipId)
    {
        var userId = _currentUser.UserId;
        var role = _currentUser.Role;

        var internship = await _context.Internships.FirstOrDefaultAsync(i => i.InternshipID == internshipId);
        if (internship == null)
            throw AppException.NotFound("ไม่พบข้อมูลการฝึกงานนี้");

        if (role == "student" && internship.StudentID != userId)
            throw AppException.Forbidden("คุณไม่มีสิทธิ์เข้าถึงข้อมูลนี้");
            
        if (role == "advisor" && internship.AdvisorID != userId)
            throw AppException.Forbidden("คุณไม่มีสิทธิ์เข้าถึงข้อมูลนี้");

        var reports = await _context.Reports
            .Include(r => r.Student)
            .Where(r => r.InternshipID == internshipId)
            .OrderByDescending(r => r.StartDate)
            .ToListAsync();

        return reports.Select(MapToDto);
    }

    public async Task<ReportResponseDto> GetReportDetailAsync(int reportId)
    {
        var report = await _context.Reports
            .Include(r => r.Student)
            .Include(r => r.Internship)
            .FirstOrDefaultAsync(r => r.ReportID == reportId);

        if (report == null)
            throw AppException.NotFound("ไม่พบข้อมูลรายงานนี้");

        ValidateAccess(report.Internship);

        return MapToDto(report);
    }

    public async Task<ReportResponseDto> GenerateReportAsync(ReportGenerateRequestDto request)
    {
        var userId = _currentUser.UserId;
        var role = _currentUser.Role;

        if (role != "student")
            throw AppException.Forbidden("เฉพาะนักศึกษาเท่านั้นที่สามารถสร้างรายงานได้");

        var internship = await _context.Internships.FirstOrDefaultAsync(i => i.InternshipID == request.InternshipID);
        if (internship == null)
            throw AppException.NotFound("ไม่พบข้อมูลการฝึกงานนี้");

        if (internship.StudentID != userId)
            throw AppException.Forbidden("คุณไม่ใช่เจ้าของการฝึกงานนี้");

        // เช็คว่ามีรายงานซ้ำในช่วงนี้หรือไม่
        bool exists = await _context.Reports.AnyAsync(r => r.InternshipID == request.InternshipID && r.PeriodLabel == request.PeriodLabel);
        if (exists)
            throw AppException.BadRequest($"มีการสร้างรายงานสำหรับ {request.PeriodLabel} ไปแล้ว");

        var report = new Report
        {
            StudentID = userId,
            InternshipID = request.InternshipID,
            PeriodLabel = request.PeriodLabel,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = "draft",
            CreatedAt = DateTime.Now
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();
        
        report.Student = await _context.Users.FirstAsync(u => u.UserID == userId);

        return MapToDto(report);
    }

    public async Task<ReportResponseDto> SubmitReportAsync(int reportId)
    {
        var userId = _currentUser.UserId;

        var report = await _context.Reports
            .Include(r => r.Student)
            .Include(r => r.Internship)
            .FirstOrDefaultAsync(r => r.ReportID == reportId);

        if (report == null)
            throw AppException.NotFound("ไม่พบข้อมูลรายงานนี้");

        if (report.StudentID != userId)
            throw AppException.Forbidden("คุณไม่ใช่เจ้าของรายงานนี้");

        if (report.Status != "draft" && report.Status != "returned")
            throw AppException.BadRequest("รายงานอยู่ในสถานะที่ไม่สามารถส่งได้");

        report.Status = "submitted";
        await _context.SaveChangesAsync();

        return MapToDto(report);
    }

    public async Task<ReportResponseDto> ReviewReportAsync(int reportId, ReportReviewRequestDto request)
    {
        var advisorId = _currentUser.UserId;

        var report = await _context.Reports
            .Include(r => r.Student)
            .Include(r => r.Internship)
            .FirstOrDefaultAsync(r => r.ReportID == reportId);

        if (report == null)
            throw AppException.NotFound("ไม่พบข้อมูลรายงานนี้");

        if (report.Internship.AdvisorID != advisorId)
            throw AppException.Forbidden("คุณไม่ใช่ที่ปรึกษาของการฝึกงานนี้");

        if (report.Status != "submitted")
            throw AppException.BadRequest("ไม่สามารถประเมินรายงานที่ยังไม่ได้ส่งได้");

        if (request.Status != "approved" && request.Status != "returned")
            throw AppException.BadRequest("สถานะไม่ถูกต้อง (ต้องเป็น approved หรือ returned)");

        report.Status = request.Status;
        report.ReviewNote = request.ReviewNote;
        
        await _context.SaveChangesAsync();

        return MapToDto(report);
    }

    public async Task<string> ExportPdfAsync(int reportId)
    {
        var report = await _context.Reports
            .Include(r => r.Student)
            .Include(r => r.Internship)
            .FirstOrDefaultAsync(r => r.ReportID == reportId);

        if (report == null)
            throw AppException.NotFound("ไม่พบข้อมูลรายงานนี้");

        ValidateAccess(report.Internship);
        
        // ถ้าเคยออก PDF ไปแล้ว และไม่อยากให้ออกใหม่บ่อยๆ อาจจะคืนค่า FilePath เดิม
        // แต่ในที่นี้จะให้ Generate ทับไปเลย

        var folderPath = Path.Combine(_env.WebRootPath, "uploads", "reports");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var fileName = $"Report_{report.ReportID}_{DateTime.Now.Ticks}.pdf";
        var fullPath = Path.Combine(folderPath, fileName);

        // วาดหน้าตา PDF เบื้องต้นด้วย QuestPDF
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12)); // ควรโหลดฟอนต์ภาษาไทยด้วยถ้าเป็น Production
                
                page.Header()
                    .Text($"Internship Report: {report.PeriodLabel}")
                    .SemiBold().FontSize(24).FontColor(Colors.Blue.Darken2);
                
                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(x =>
                    {
                        x.Spacing(10);
                        
                        x.Item().Text($"Student: {report.Student.FirstName} {report.Student.LastName}");
                        x.Item().Text($"Period: {report.StartDate:dd/MM/yyyy} - {report.EndDate:dd/MM/yyyy}");
                        x.Item().Text($"Status: {report.Status}");
                        
                        if (!string.IsNullOrEmpty(report.ReviewNote))
                        {
                            x.Item().Text("Advisor Note:");
                            x.Item().Text(report.ReviewNote).Italic();
                        }
                    });
                
                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
            });
        })
        .GeneratePdf(fullPath);

        var fileUrl = $"/uploads/reports/{fileName}";
        
        report.FilePath = fileUrl;
        await _context.SaveChangesAsync();

        return fileUrl;
    }

    private void ValidateAccess(Internship internship)
    {
        var userId = _currentUser.UserId;
        var role = _currentUser.Role;

        if (role == "student" && internship.StudentID != userId)
            throw AppException.Forbidden("คุณไม่มีสิทธิ์เข้าถึงข้อมูลนี้");

        if (role == "advisor" && internship.AdvisorID != userId)
            throw AppException.Forbidden("คุณไม่มีสิทธิ์เข้าถึงข้อมูลนี้");
    }

    private static ReportResponseDto MapToDto(Report report)
    {
        return new ReportResponseDto(
            report.ReportID,
            report.InternshipID,
            report.StudentID,
            report.Student != null ? $"{report.Student.FirstName} {report.Student.LastName}" : string.Empty,
            report.PeriodLabel,
            report.StartDate,
            report.EndDate,
            report.Status,
            report.ReviewNote,
            report.FilePath,
            report.CreatedAt
        );
    }
}
