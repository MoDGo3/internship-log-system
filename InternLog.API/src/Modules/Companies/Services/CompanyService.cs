using InternLog.API.Infrastructure.Data;
using InternLog.API.Modules.Admin.Models;
using InternLog.API.Modules.Companies.DTOs;
using Microsoft.EntityFrameworkCore;

namespace InternLog.API.Modules.Companies.Services;

public class CompanyService : ICompanyService {
    private readonly AppDbContext _context;

    public CompanyService(AppDbContext context) {
        _context = context;
    }

    public async Task<List<CompanyDto>> GetCompaniesAsync(int userId, string role) {
        var query = _context.Companies.AsQueryable();
        
        if (role == "student") {
            var myCompanyIds = await _context.Internships
                .Where(i => i.StudentID == userId && i.Status == "active")
                .Select(i => i.CompanyID)
                .ToListAsync();
            query = query.Where(c => myCompanyIds.Contains(c.CompanyID));
        }
        else if (role == "advisor") {
            var advCompanyIds = await _context.Internships
                .Where(i => i.AdvisorID == userId && i.Status == "active")
                .Select(i => i.CompanyID)
                .ToListAsync();
            query = query.Where(c => c.AdvisorID == userId || advCompanyIds.Contains(c.CompanyID));
        }

        var companies = await query.ToListAsync();
        return companies.Select(c => new CompanyDto {
            CompanyID = c.CompanyID,
            Name = c.Name,
            Address = c.Address,
            Phone = c.Phone,
            WorkStartTime = c.WorkStartTime,
            WorkEndTime = c.WorkEndTime,
            AdvisorID = c.AdvisorID
        }).ToList();
    }

    public async Task<CompanyDto?> CreateCompanyAsync(int userId, string role, CreateCompanyRequest request) {
        if (role == "student") return null;

        var company = new Company {
            Name = request.Name,
            Address = request.Address,
            Phone = request.Phone,
            WorkStartTime = request.WorkStartTime ?? new TimeSpan(9, 0, 0),
            WorkEndTime = request.WorkEndTime ?? new TimeSpan(18, 0, 0),
            AdvisorID = role == "advisor" ? userId : null
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

        return new CompanyDto {
            CompanyID = company.CompanyID,
            Name = company.Name,
            Address = company.Address,
            Phone = company.Phone,
            WorkStartTime = company.WorkStartTime,
            WorkEndTime = company.WorkEndTime,
            AdvisorID = company.AdvisorID
        };
    }

    public async Task<CompanyDto?> UpdateCompanyAsync(int companyId, int userId, string role, UpdateCompanyRequest request) {
        var company = await _context.Companies.FindAsync(companyId);
        if (company == null) return null;

        if (role == "advisor" && company.AdvisorID != userId) return null;

        company.Name = request.Name;
        company.Address = request.Address;
        company.Phone = request.Phone;
        if (request.WorkStartTime.HasValue) company.WorkStartTime = request.WorkStartTime.Value;
        if (request.WorkEndTime.HasValue) company.WorkEndTime = request.WorkEndTime.Value;

        await _context.SaveChangesAsync();

        return new CompanyDto {
            CompanyID = company.CompanyID,
            Name = company.Name,
            Address = company.Address,
            Phone = company.Phone,
            WorkStartTime = company.WorkStartTime,
            WorkEndTime = company.WorkEndTime,
            AdvisorID = company.AdvisorID
        };
    }
}
