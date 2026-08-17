using Microsoft.EntityFrameworkCore;
using InternLog.API.Modules.Dashboard.DTOs;
using InternLog.API.Core.Interfaces;
using InternLog.API.Infrastructure.Data;

namespace InternLog.API.Modules.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DashboardService(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var userId = _currentUser.UserId;
        var role = _currentUser.Role;

        int totalInternships = 0;
        int activeInternships = 0;
        int pendingLeaves = 0;
        int pendingReports = 0;
        
        int unreadMessages = await _context.Messages.CountAsync(m => m.ReceiverID == userId && !m.IsRead);
        int unreadNotifications = await _context.Notifications.CountAsync(n => n.UserID == userId && !n.IsRead);

        if (role == "student")
        {
            totalInternships = await _context.Internships.CountAsync(i => i.StudentID == userId);
            activeInternships = await _context.Internships.CountAsync(i => i.StudentID == userId && i.EndDate >= DateOnly.FromDateTime(DateTime.Today));
            
            pendingLeaves = await _context.Leaves.CountAsync(l => l.StudentID == userId && l.Status == "pending");
            pendingReports = await _context.Reports.CountAsync(r => r.Internship.StudentID == userId && (r.Status == "draft" || r.Status == "returned"));
        }
        else if (role == "advisor")
        {
            totalInternships = await _context.Internships.CountAsync(i => i.AdvisorID == userId);
            activeInternships = await _context.Internships.CountAsync(i => i.AdvisorID == userId && i.EndDate >= DateOnly.FromDateTime(DateTime.Today));
            
            pendingReports = await _context.Reports.CountAsync(r => r.Internship.AdvisorID == userId && r.Status == "submitted");
        }
        else if (role == "supervisor")
        {
            totalInternships = await _context.Internships.CountAsync(i => i.SupervisorID == userId);
            activeInternships = await _context.Internships.CountAsync(i => i.SupervisorID == userId && i.EndDate >= DateOnly.FromDateTime(DateTime.Today));
            
            pendingLeaves = await _context.Leaves.CountAsync(l => l.Internship.SupervisorID == userId && l.Status == "pending");
        }
        else if (role == "admin")
        {
            totalInternships = await _context.Internships.CountAsync();
            activeInternships = await _context.Internships.CountAsync(i => i.EndDate >= DateOnly.FromDateTime(DateTime.Today));
        }

        return new DashboardSummaryDto(
            Role: role,
            TotalInternships: totalInternships,
            ActiveInternships: activeInternships,
            PendingLeaves: pendingLeaves,
            PendingReports: pendingReports,
            UnreadMessages: unreadMessages,
            UnreadNotifications: unreadNotifications
        );
    }

    public async Task<IEnumerable<CalendarDayDto>> GetCalendarAsync(int year, int month)
    {
        var userId = _currentUser.UserId;
        var role = _currentUser.Role;

        // ดึงเฉพาะของนักศึกษา (ถ้าเป็น role อื่นให้คืนค่าว่างไปก่อน หรือส่ง user id มาเพิ่มในอนาคต)
        if (role != "student")
        {
            return Enumerable.Empty<CalendarDayDto>();
        }

        var startDate = new DateOnly(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var attendances = await _context.Attendances
            .Where(a => a.StudentID == userId && a.AttDate >= startDate && a.AttDate <= endDate)
            .ToListAsync();

        var leaves = await _context.Leaves
            .Where(l => l.StudentID == userId && l.StartDate <= endDate && l.EndDate >= startDate && l.Status == "approved")
            .ToListAsync();

        var result = new List<CalendarDayDto>();
        var today = DateOnly.FromDateTime(DateTime.Today);

        for (int day = 1; day <= endDate.Day; day++)
        {
            var currentDate = new DateOnly(year, month, day);
            string status = "Absent";

            if (currentDate > today)
            {
                status = "Future";
            }
            else if (leaves.Any(l => l.StartDate <= currentDate && l.EndDate >= currentDate))
            {
                status = "Leave";
            }
            else if (attendances.Any(a => a.AttDate == currentDate))
            {
                status = "Present";
            }
            else if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
            {
                status = "Holiday"; // วันหยุดเสาร์อาทิตย์เบื้องต้น
            }

            result.Add(new CalendarDayDto(currentDate, status));
        }

        return result;
    }
}
