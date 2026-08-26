namespace InternLog.API.Modules.Admin.Models;
public class Company {
    public int CompanyID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public TimeSpan WorkStartTime { get; set; } = new TimeSpan(9, 0, 0);
    public TimeSpan WorkEndTime { get; set; } = new TimeSpan(18, 0, 0);
    public int? AdvisorID { get; set; }
}
