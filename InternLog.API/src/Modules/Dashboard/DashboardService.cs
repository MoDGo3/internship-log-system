using Microsoft.EntityFrameworkCore;
using InternLog.API.Modules.Dashboard.DTOs;
using InternLog.API.Core.Interfaces;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Core.Common;

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
        
        int unreadMessages = 0;
        int unreadNotifications = await _context.Notifications.CountAsync(n => n.UserID == userId && !n.IsRead);

        if (role == Constants.Roles.Student)
        {
            totalInternships = await _context.Internships.CountAsync(i => i.StudentID == userId);
            activeInternships = await _context.Internships.CountAsync(i => i.StudentID == userId && i.EndDate >= AppClock.Today && i.Status.ToLower() == "active");
            
            pendingLeaves = await _context.Leaves.CountAsync(l => l.StudentID == userId && l.Status == "pending");
            pendingReports = await _context.Reports.CountAsync(r => r.Internship.StudentID == userId && (r.Status == "draft" || r.Status == "returned"));
        }
        else if (role == Constants.Roles.Advisor)
        {
            totalInternships = await _context.Internships.CountAsync(i => i.AdvisorID == userId);
            activeInternships = await _context.Internships.CountAsync(i => i.AdvisorID == userId && i.EndDate >= AppClock.Today && i.Status.ToLower() == "active");
            
            pendingReports = await _context.Reports.CountAsync(r => r.Internship.AdvisorID == userId && r.Status == "submitted");
            // ใบลาที่รออาจารย์อนุมัติ (เฉพาะนักศึกษาที่ตนดูแล)
            pendingLeaves = await _context.Leaves.CountAsync(l => l.Internship.AdvisorID == userId && l.Status == "pending");
        }
        else if (role == Constants.Roles.Admin)
        {
            totalInternships = await _context.Internships.CountAsync();
            activeInternships = await _context.Internships.CountAsync(i => i.EndDate >= AppClock.Today && i.Status.ToLower() == "active");
            pendingLeaves = await _context.Leaves.CountAsync(l => l.Status == "pending");
            pendingReports = await _context.Reports.CountAsync(r => r.Status == "submitted");
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
        if (role != Constants.Roles.Student)
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
        var today = AppClock.Today;

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
