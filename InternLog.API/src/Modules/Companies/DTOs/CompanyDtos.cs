namespace InternLog.API.Modules.Companies.DTOs;

public record CompanyDto(
    int CompanyID,
    string CompanyName,
    string? Address,
    string? Phone
);
