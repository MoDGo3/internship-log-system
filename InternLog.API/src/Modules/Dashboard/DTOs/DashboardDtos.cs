namespace InternLog.API.Modules.Dashboard.DTOs;

public record DashboardSummaryDto(
    string Role,
    int TotalInternships = 0,
    int ActiveInternships = 0,
    int PendingLeaves = 0,
    int PendingReports = 0,
    int UnreadMessages = 0,
    int UnreadNotifications = 0
);

public record CalendarDayDto(
    DateOnly Date,
    string Status // "Present", "Absent", "Leave", "Holiday", "Future"
);
