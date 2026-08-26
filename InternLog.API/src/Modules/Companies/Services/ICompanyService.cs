using InternLog.API.Modules.Companies.DTOs;

namespace InternLog.API.Modules.Companies.Services;

public interface ICompanyService {
    Task<List<CompanyDto>> GetCompaniesAsync(int userId, string role);
    Task<CompanyDto?> CreateCompanyAsync(int userId, string role, CreateCompanyRequest request);
    Task<CompanyDto?> UpdateCompanyAsync(int companyId, int advisorId, string role, UpdateCompanyRequest request);
}
