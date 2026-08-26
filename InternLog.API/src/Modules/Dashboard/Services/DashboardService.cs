using InternLog.API.Infrastructure.Data;
using InternLog.API.Modules.Dashboard.DTOs;
using Microsoft.EntityFrameworkCore;

namespace InternLog.API.Modules.Dashboard.Services;

public class DashboardService : IDashboardService {
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context) {
        _context = context;
    }

    public async Task<StudentDashboardDto> GetStudentDashboardAsync(int studentId) {
        var internship = await _context.Internships.FirstOrDefaultAsync(i => i.StudentID == studentId && i.Status == "active");
        
        var dto = new StudentDashboardDto {
            RequiredDays = internship?.RequiredDays ?? 60,
            RequiredHours = internship?.RequiredHours ?? 480
        };

        if (internship != null) {
            dto.WorkedDays = await _context.Attendances.CountAsync(a => a.StudentID == studentId && a.Status != "absent");
            dto.WorkedHours = (double)(await _context.Attendances.Where(a => a.StudentID == studentId).SumAsync(a => (decimal?)a.TotalHours) ?? 0m);
            // Just dummy queries to fulfill structure
        }

        return dto;
    }

    public async Task<AdvisorDashboardDto> GetAdvisorDashboardAsync(int advisorId) {
        var dto = new AdvisorDashboardDto {
            TotalStudents = await _context.Internships.CountAsync(i => i.AdvisorID == advisorId && i.Status == "active"),
            PendingLeaveRequests = await _context.Leaves.CountAsync(l => l.Status == "pending" && _context.Internships.Any(i => i.StudentID == l.StudentID && i.AdvisorID == advisorId && i.Status == "active"))
        };
        return dto;
    }

    public async Task<AdminDashboardDto> GetAdminDashboardAsync() {
        var dto = new AdminDashboardDto {
            TotalStudents = await _context.Users.CountAsync(u => u.Role.Name == "student"),
            TotalAdvisors = await _context.Users.CountAsync(u => u.Role.Name == "advisor"),
            TotalCompanies = await _context.Companies.CountAsync(),
            UnmatchedStudents = await _context.Users.CountAsync(u => u.Role.Name == "student" && !_context.Internships.Any(i => i.StudentID == u.UserID && i.Status == "active"))
        };
        return dto;
    }
}
