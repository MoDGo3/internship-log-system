using InternLog.API.Modules.Dashboard.DTOs;

namespace InternLog.API.Modules.Dashboard;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
    Task<IEnumerable<CalendarDayDto>> GetCalendarAsync(int year, int month);
}
