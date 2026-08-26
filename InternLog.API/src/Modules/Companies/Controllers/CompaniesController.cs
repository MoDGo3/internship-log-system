using InternLog.API.Core.Common;
using InternLog.API.Modules.Companies.DTOs;
using InternLog.API.Modules.Companies.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InternLog.API.Modules.Companies.Controllers;

[Authorize]
public class CompaniesController : AppControllerBase {
    private readonly ICompanyService _companyService;

    public CompaniesController(ICompanyService companyService) {
        _companyService = companyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCompanies() {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "";
        var list = await _companyService.GetCompaniesAsync(GetUserId(), role);
        return Ok(new ApiResponse<List<CompanyDto>>(true, "ดึงข้อมูลสำเร็จ", list));
    }

    [Authorize(Roles = "advisor,admin")]
    [HttpPost]
    public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyRequest request) {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "";
        var result = await _companyService.CreateCompanyAsync(GetUserId(), role, request);
        if (result == null) return Forbid();
        return Ok(new ApiResponse<CompanyDto>(true, "สร้างข้อมูลบริษัทสำเร็จ", result));
    }

    [Authorize(Roles = "advisor,admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCompany(int id, [FromBody] UpdateCompanyRequest request) {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "";
        var result = await _companyService.UpdateCompanyAsync(id, GetUserId(), role, request);
        if (result == null) return Forbid();
        return Ok(new ApiResponse<CompanyDto>(true, "แก้ไขข้อมูลบริษัทสำเร็จ", result));
    }
}
