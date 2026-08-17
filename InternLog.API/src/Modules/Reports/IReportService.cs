using InternLog.API.Modules.Reports.DTOs;

namespace InternLog.API.Modules.Reports;

public interface IReportService
{
    Task<IEnumerable<ReportResponseDto>> GetReportsAsync(int internshipId);
    Task<ReportResponseDto> GetReportDetailAsync(int reportId);
    Task<ReportResponseDto> GenerateReportAsync(ReportGenerateRequestDto request);
    Task<ReportResponseDto> SubmitReportAsync(int reportId);
    Task<ReportResponseDto> ReviewReportAsync(int reportId, ReportReviewRequestDto request);
    Task<string> ExportPdfAsync(int reportId);
}
