using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using InternLog.API.Modules.Admin.DTOs;
using InternLog.API.Core.Common;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Infrastructure.Data.Models;

namespace InternLog.API.Modules.Admin;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;

    public AdminService(AppDbContext context)
    {
        _context = context;
    }

    // ─── Users ────────────────────────────────────────────────────────────────
    public async Task<IEnumerable<AdminUserResponseDto>> GetUsersAsync(string? role, string? search)
    {
        var query = _context.Users
            .Include(u => u.Role)
            .AsQueryable();

        if (!string.IsNullOrEmpty(role))
            query = query.Where(u => u.Role.RoleName == role);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(u =>
                u.FirstName.Contains(search) ||
                u.LastName.Contains(search) ||
                u.Email.Contains(search) ||
                (u.StudentID != null && u.StudentID.Contains(search)));

        var users = await query.OrderBy(u => u.LastName).ToListAsync();
        return users.Select(MapUserToDto);
    }

    public async Task<AdminUserResponseDto> GetUserDetailAsync(int userId)
    {
        var user = await _context.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserID == userId);

        if (user == null)
            throw AppException.NotFound("ไม่พบผู้ใช้งานนี้");

        return MapUserToDto(user);
    }

    public async Task<AdminUserResponseDto> CreateUserAsync(AdminCreateUserDto request)
    {
        bool emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (emailExists)
            throw AppException.BadRequest("Email นี้ถูกใช้งานแล้ว");

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleID == request.RoleID);
        if (role == null)
            throw AppException.NotFound("ไม่พบ Role นี้ในระบบ");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            RoleID = request.RoleID,
            Phone = request.Phone,
            StudentID = request.StudentCode,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        user.Role = role;

        return MapUserToDto(user);
    }

    public async Task<AdminUserResponseDto> UpdateUserAsync(int userId, AdminUpdateUserDto request)
    {
        var user = await _context.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserID == userId);

        if (user == null)
            throw AppException.NotFound("ไม่พบผู้ใช้งานนี้");

        if (request.FirstName != null) user.FirstName = request.FirstName;
        if (request.LastName != null) user.LastName = request.LastName;
        if (request.Phone != null) user.Phone = request.Phone;
        if (request.StudentCode != null) user.StudentID = request.StudentCode;
        if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;

        if (request.RoleID.HasValue)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleID == request.RoleID.Value);
            if (role == null) throw AppException.NotFound("ไม่พบ Role นี้ในระบบ");
            user.RoleID = request.RoleID.Value;
            user.Role = role;
        }

        await _context.SaveChangesAsync();
        return MapUserToDto(user);
    }

    public async Task DeleteUserAsync(int userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
        if (user == null)
            throw AppException.NotFound("ไม่พบผู้ใช้งานนี้");

        // Soft delete: ปิดการใช้งานแทนการลบจริง
        user.IsActive = false;
        await _context.SaveChangesAsync();
    }

    // ─── Companies ────────────────────────────────────────────────────────────
    public async Task<IEnumerable<AdminCompanyResponseDto>> GetCompaniesAsync()
    {
        var companies = await _context.Companies.OrderBy(c => c.CompanyName).ToListAsync();
        return companies.Select(MapCompanyToDto);
    }

    public async Task<AdminCompanyResponseDto> CreateCompanyAsync(AdminCompanyDto request)
    {
        var company = new Company
        {
            CompanyName = request.CompanyName,
            Address = request.Address,
            Phone = request.Phone
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();
        return MapCompanyToDto(company);
    }

    public async Task<AdminCompanyResponseDto> UpdateCompanyAsync(int companyId, AdminCompanyDto request)
    {
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyID == companyId);
        if (company == null)
            throw AppException.NotFound("ไม่พบบริษัทนี้");

        company.CompanyName = request.CompanyName;
        company.Address = request.Address;
        company.Phone = request.Phone;

        await _context.SaveChangesAsync();
        return MapCompanyToDto(company);
    }

    public async Task DeleteCompanyAsync(int companyId)
    {
        var company = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyID == companyId);
        if (company == null)
            throw AppException.NotFound("ไม่พบบริษัทนี้");

        _context.Companies.Remove(company);
        await _context.SaveChangesAsync();
    }

    // ─── Internships ──────────────────────────────────────────────────────────
    public async Task<object> GetInternshipsAsync()
    {
        var internships = await _context.Internships
            .Include(i => i.Student)
            .Include(i => i.Company)
            .Include(i => i.Advisor)
            .OrderByDescending(i => i.StartDate)
            .Select(i => new
            {
                i.InternshipID,
                StudentID = i.StudentID,
                StudentName = $"{i.Student.FirstName} {i.Student.LastName}",
                CompanyName = i.Company.CompanyName,
                AdvisorName = $"{i.Advisor.FirstName} {i.Advisor.LastName}",
                i.StartDate,
                i.EndDate,
                i.RequiredDays
            })
            .ToListAsync();

        return internships;
    }

    public async Task<object> CreateInternshipAsync(AdminCreateInternshipDto request)
    {
        // ตรวจสอบว่า Student ยังไม่มีการฝึกงานที่ Active
        bool active = await _context.Internships.AnyAsync(i =>
            i.StudentID == request.StudentID &&
            i.EndDate >= DateOnly.FromDateTime(DateTime.Today));
        if (active)
            throw AppException.BadRequest("นักศึกษานี้มีการฝึกงานที่ยังไม่สิ้นสุดอยู่แล้ว");

        var internship = new Internship
        {
            StudentID = request.StudentID,
            CompanyID = request.CompanyID,
            AdvisorID = request.AdvisorID,
            SupervisorID = request.SupervisorID,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            RequiredDays = request.RequiredDays
        };

        _context.Internships.Add(internship);
        await _context.SaveChangesAsync();

        return new { internship.InternshipID, Message = "สร้างการฝึกงานเรียบร้อย" };
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────
    private static AdminUserResponseDto MapUserToDto(User u) => new(
        u.UserID,
        u.Email,
        u.FirstName,
        u.LastName,
        u.Role?.RoleName ?? string.Empty,
        u.Phone,
        u.StudentID,
        u.IsActive,
        u.CreatedAt
    );

    private static AdminCompanyResponseDto MapCompanyToDto(Company c) => new(
        c.CompanyID,
        c.CompanyName,
        c.Address,
        null,
        c.Phone
    );
}
