namespace InternLog.API.Modules.Dashboard.DTOs;

public class StudentDashboardDto {
    public int WorkedDays { get; set; }
    public int RequiredDays { get; set; }
    public double WorkedHours { get; set; }
    public int RequiredHours { get; set; }
    public double RemainingLeaves { get; set; }
    public string TodayStatus { get; set; } = string.Empty;
    public List<object> RecentNotifications { get; set; } = new();
    public List<object> RecentWorkLogs { get; set; } = new();
    public List<object> DueTasks { get; set; } = new();
}

public class AdvisorDashboardDto {
    public int TotalStudents { get; set; }
    public int PendingLeaveRequests { get; set; }
    public List<object> RecentStudentLogs { get; set; } = new();
    public List<object> PendingVisits { get; set; } = new();
    public List<object> StudentProgress { get; set; } = new();
}

public class AdminDashboardDto {
    public int TotalStudents { get; set; }
    public int TotalAdvisors { get; set; }
    public int TotalCompanies { get; set; }
    public int UnmatchedStudents { get; set; }
    public int InactiveUsers { get; set; }
    public object SystemStats { get; set; } = new();
}
