using InternLog.API.Modules.Auth.Models;
namespace InternLog.API.Modules.Admin.Models;
public class Internship {
    public int InternshipID { get; set; }
    public int StudentID { get; set; }
    public int CompanyID { get; set; }
    public int AdvisorID { get; set; }
    public int? SupervisorID { get; set; }
    public int PeriodID { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int RequiredDays { get; set; } = 60;
    public int RequiredHours { get; set; } = 480;
    public string Status { get; set; } = "active";
    public string? CompanyAddress { get; set; }
    public string? CompanyPhone { get; set; }
    
    public User Student { get; set; } = null!;
    public Company Company { get; set; } = null!;
    public User Advisor { get; set; } = null!;
    public User? Supervisor { get; set; }
    public TrainingPeriod Period { get; set; } = null!;
}
