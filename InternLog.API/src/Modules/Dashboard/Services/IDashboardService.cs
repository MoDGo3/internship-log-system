using InternLog.API.Modules.Dashboard.DTOs;

namespace InternLog.API.Modules.Dashboard.Services;

public interface IDashboardService {
    Task<StudentDashboardDto> GetStudentDashboardAsync(int studentId);
    Task<AdvisorDashboardDto> GetAdvisorDashboardAsync(int advisorId);
    Task<AdminDashboardDto> GetAdminDashboardAsync();
}
