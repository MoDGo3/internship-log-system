using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InternLog.API.Modules.Reports.DTOs;
using InternLog.API.Core.Common;

namespace InternLog.API.Modules.Reports;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet]
    [Authorize(Roles = "student,advisor")]
    public async Task<IActionResult> GetReports([FromQuery] int internshipId)
    {
        var reports = await _reportService.GetReportsAsync(internshipId);
        return Ok(ApiResponse<IEnumerable<ReportResponseDto>>.Ok(reports));
    }

    [HttpPost("generate")]
    [Authorize(Roles = "student")]
    public async Task<IActionResult> GenerateReport([FromBody] ReportGenerateRequestDto request)
    {
        var report = await _reportService.GenerateReportAsync(request);
        return CreatedAtAction(nameof(GetReports), new { internshipId = report.InternshipID }, ApiResponse<ReportResponseDto>.Ok(report, "สร้างร่างรายงานเรียบร้อย"));
    }

    [HttpPut("{id}/submit")]
    [Authorize(Roles = "student")]
    public async Task<IActionResult> SubmitReport(int id)
    {
        var report = await _reportService.SubmitReportAsync(id);
        return Ok(ApiResponse<ReportResponseDto>.Ok(report, "ส่งรายงานให้ที่ปรึกษาเรียบร้อย"));
    }

    [HttpPut("{id}/review")]
    [Authorize(Roles = "advisor")]
    public async Task<IActionResult> ReviewReport(int id, [FromBody] ReportReviewRequestDto request)
    {
        var report = await _reportService.ReviewReportAsync(id, request);
        return Ok(ApiResponse<ReportResponseDto>.Ok(report, "ประเมินรายงานเรียบร้อย"));
    }

    [HttpGet("{id}/export-pdf")]
    [Authorize(Roles = "student,advisor")]
    public async Task<IActionResult> ExportPdf(int id)
    {
        var fileUrl = await _reportService.ExportPdfAsync(id);
        return Ok(ApiResponse<string>.Ok(fileUrl, "สร้างไฟล์ PDF เรียบร้อย"));
    }
}
