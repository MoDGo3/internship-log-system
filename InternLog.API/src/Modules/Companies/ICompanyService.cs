using InternLog.API.Modules.Companies.DTOs;

namespace InternLog.API.Modules.Companies;

public interface ICompanyService
{
    /// <summary>advisor: เฉพาะบริษัทที่มีนักศึกษาในความดูแล · admin: ทั้งหมด</summary>
    Task<IEnumerable<CompanyDto>> GetCompaniesAsync();

    Task<CompanyDto> GetCompanyAsync(int companyId);
}
