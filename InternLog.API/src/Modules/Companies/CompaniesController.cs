using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InternLog.API.Core.Common;
using InternLog.API.Modules.Companies.DTOs;

namespace InternLog.API.Modules.Companies;

/// <summary>
/// Companies endpoint สำหรับ Advisor ดูบริษัทที่ตนเองดูแล
/// Admin จัดการบริษัทผ่าน /api/admin/companies
/// </summary>
[Authorize(Roles = $"{Constants.Roles.Advisor},{Constants.Roles.Admin}")]
public class CompaniesController : AppControllerBase
{
    private readonly ICompanyService _companies;

    public CompaniesController(ICompanyService companies)
    {
        _companies = companies;
    }

    /// <summary>GET /api/companies — advisor เห็นเฉพาะของตนเอง, admin เห็นทั้งหมด</summary>
    [HttpGet]
    public async Task<IActionResult> GetCompanies()
        => Ok(ApiResponse<IEnumerable<CompanyDto>>.Ok(await _companies.GetCompaniesAsync()));

    /// <summary>GET /api/companies/{id}</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCompany(int id)
        => Ok(ApiResponse<CompanyDto>.Ok(await _companies.GetCompanyAsync(id)));
}
