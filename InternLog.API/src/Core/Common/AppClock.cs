namespace InternLog.API.Core.Common;

/// <summary>
/// เวลาของระบบตามเวลาประเทศไทย (UTC+7) — ใช้แทน DateTime.Now / DateTime.Today
///
/// เหตุผล: DateTime.Now ขึ้นกับ timezone ของเครื่อง server ถ้า deploy บน server ที่ตั้งเป็น UTC
/// วันที่ลงเวลา/วันลา/เวลาบันทึกจะเพี้ยน 7 ชั่วโมง
///
/// ข้อตกลงในโปรเจกต์:
/// - เวลาธุรกิจ (ลงเวลา, วันลา, CreatedAt ที่แสดงให้ผู้ใช้เห็น) → <see cref="Now"/> / <see cref="Today"/> (เวลาไทย)
/// - เวลาด้านความปลอดภัย (อายุ token, RevokedAt) → DateTime.UtcNow
/// </summary>
public static class AppClock
{
    private static readonly TimeZoneInfo ThailandTimeZone = ResolveThailandTimeZone();

    /// <summary>วันเวลาปัจจุบันตามเวลาประเทศไทย</summary>
    public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ThailandTimeZone);

    /// <summary>วันที่ปัจจุบันตามเวลาประเทศไทย</summary>
    public static DateOnly Today => DateOnly.FromDateTime(Now);

    /// <summary>เวลาปัจจุบัน (ชั่วโมง:นาที:วินาที) ตามเวลาประเทศไทย</summary>
    public static TimeOnly TimeOfDay => TimeOnly.FromDateTime(Now);

    private static TimeZoneInfo ResolveThailandTimeZone()
    {
        // IANA id ใช้ได้บน Linux และ Windows (.NET 6+ ที่มี ICU), id แบบ Windows ใช้เป็นตัวสำรอง
        foreach (var id in new[] { "Asia/Bangkok", "SE Asia Standard Time" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }
        // ประเทศไทยไม่มี daylight saving — fallback เป็น UTC+7 คงที่
        return TimeZoneInfo.CreateCustomTimeZone("ICT", TimeSpan.FromHours(7), "Indochina Time", "Indochina Time");
    }
}
