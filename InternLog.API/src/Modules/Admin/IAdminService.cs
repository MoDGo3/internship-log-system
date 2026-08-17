using InternLog.API.Modules.Admin.DTOs;

namespace InternLog.API.Modules.Admin;

public interface IAdminService
{
    // Users
    Task<IEnumerable<AdminUserResponseDto>> GetUsersAsync(string? role, string? search);
    Task<AdminUserResponseDto> GetUserDetailAsync(int userId);
    Task<AdminUserResponseDto> CreateUserAsync(AdminCreateUserDto request);
    Task<AdminUserResponseDto> UpdateUserAsync(int userId, AdminUpdateUserDto request);
    Task DeleteUserAsync(int userId);

    // Companies
    Task<IEnumerable<AdminCompanyResponseDto>> GetCompaniesAsync();
    Task<AdminCompanyResponseDto> CreateCompanyAsync(AdminCompanyDto request);
    Task<AdminCompanyResponseDto> UpdateCompanyAsync(int companyId, AdminCompanyDto request);
    Task DeleteCompanyAsync(int companyId);

    // Internships
    Task<object> GetInternshipsAsync();
    Task<object> CreateInternshipAsync(AdminCreateInternshipDto request);
}
