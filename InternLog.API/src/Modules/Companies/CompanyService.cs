using Microsoft.EntityFrameworkCore;
using InternLog.API.Core.Common;
using InternLog.API.Core.Interfaces;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Infrastructure.Data.Models;
using InternLog.API.Modules.Companies.DTOs;

namespace InternLog.API.Modules.Companies;

public class CompanyService : ICompanyService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CompanyService(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<CompanyDto>> GetCompaniesAsync()
    {
        return await ScopedCompanies()
            .OrderBy(c => c.CompanyName)
            .Select(c => new CompanyDto(c.CompanyID, c.CompanyName, c.Address, c.Phone))
            .ToListAsync();
    }

    public async Task<CompanyDto> GetCompanyAsync(int companyId)
    {
        var company = await _db.Companies.FirstOrDefaultAsync(c => c.CompanyID == companyId)
            ?? throw AppException.NotFound("ไม่พบบริษัทนี้");

        // advisor ต้องดูแล internship ของบริษัทนั้น
        if (_currentUser.Role == Constants.Roles.Advisor &&
            !await _db.Internships.AnyAsync(i => i.CompanyID == companyId && i.AdvisorID == _currentUser.UserId))
            throw AppException.Forbidden("คุณไม่มีสิทธิ์เข้าถึงข้อมูลบริษัทนี้");

        return new CompanyDto(company.CompanyID, company.CompanyName, company.Address, company.Phone);
    }

    /// <summary>บริษัทที่ผู้ใช้ปัจจุบันมีสิทธิ์เห็น</summary>
    private IQueryable<Company> ScopedCompanies()
    {
        if (_currentUser.Role != Constants.Roles.Advisor)
            return _db.Companies;

        var userId = _currentUser.UserId;
        return _db.Companies.Where(c => _db.Internships.Any(i => i.CompanyID == c.CompanyID && i.AdvisorID == userId));
    }
}
