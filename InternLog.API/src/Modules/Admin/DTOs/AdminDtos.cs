using System.ComponentModel.DataAnnotations;

namespace InternLog.API.Modules.Admin.DTOs;

// ─── Users ───────────────────────────────────────────────────────────────────
public record AdminCreateUserDto(
    [Required] string Email,
    [Required] string Password,
    [Required] string FirstName,
    [Required] string LastName,
    [Required] int RoleID,
    string? Phone,
    string? StudentCode
);

public record AdminUpdateUserDto(
    string? FirstName,
    string? LastName,
    string? Phone,
    string? StudentCode,
    int? RoleID,
    bool? IsActive
);

public record AdminUserResponseDto(
    int UserID,
    string Email,
    string FirstName,
    string LastName,
    string RoleName,
    string? Phone,
    string? StudentCode,
    bool IsActive,
    DateTime CreatedAt
);

// ─── Companies ───────────────────────────────────────────────────────────────
public record AdminCompanyDto(
    [Required] string CompanyName,
    string? Address,
    string? ContactPerson,
    string? Phone
);

public record AdminCompanyResponseDto(
    int CompanyID,
    string CompanyName,
    string? Address,
    string? ContactPerson,
    string? Phone
);

// ─── Internships ─────────────────────────────────────────────────────────────
public record AdminCreateInternshipDto(
    [Required] int StudentID,
    [Required] int CompanyID,
    [Required] int AdvisorID,
    int? SupervisorID,
    [Required] DateOnly StartDate,
    [Required] DateOnly EndDate,
    int RequiredDays = 60
);
