namespace InternLog.API.Modules.Attendance;

public interface IAttendanceService
{
    Task<AttendanceDto> CheckInAsync(int studentId, string? note);
    Task<AttendanceDto> CheckOutAsync(int studentId, string? note);
    Task<IEnumerable<AttendanceDto>> GetHistoryAsync(int userId, int? month, int? year);
    Task<AttendanceSummaryDto> GetSummaryAsync(int studentId);
    Task RequestEditAsync(int attendanceId, int studentId, RequestEditDto dto);
    Task ReviewEditAsync(int attendanceId, int reviewerId, ReviewEditDto dto);
}
