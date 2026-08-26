namespace InternLog.API.Modules.Profile.DTOs;

public class UpdateProfileRequest {
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? ProfilePicture { get; set; }
    public string? Faculty { get; set; }
    public string? Major { get; set; }
    public int? Year { get; set; }
}

public class UpdateCompanyContactRequest {
    public string CompanyAddress { get; set; } = string.Empty;
    public string CompanyPhone { get; set; } = string.Empty;
}
