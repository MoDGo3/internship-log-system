using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InternLog.API.Modules.Admin.DTOs;
using InternLog.API.Core.Common;

namespace InternLog.API.Modules.Admin;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    // ─── Users ────────────────────────────────────────────────────────────────

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] string? role, [FromQuery] string? search)
    {
        var users = await _adminService.GetUsersAsync(role, search);
        return Ok(ApiResponse<IEnumerable<AdminUserResponseDto>>.Ok(users));
    }

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _adminService.GetUserDetailAsync(id);
        return Ok(ApiResponse<AdminUserResponseDto>.Ok(user));
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] AdminCreateUserDto request)
    {
        var user = await _adminService.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetUser), new { id = user.UserID },
            ApiResponse<AdminUserResponseDto>.Ok(user, "สร้างผู้ใช้งานสำเร็จ"));
    }

    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] AdminUpdateUserDto request)
    {
        var user = await _adminService.UpdateUserAsync(id, request);
        return Ok(ApiResponse<AdminUserResponseDto>.Ok(user, "แก้ไขข้อมูลผู้ใช้งานสำเร็จ"));
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _adminService.DeleteUserAsync(id);
        return Ok(ApiResponse<string>.Ok("ok", "ปิดการใช้งานผู้ใช้เรียบร้อย"));
    }

    // ─── Companies ────────────────────────────────────────────────────────────

    [HttpGet("companies")]
    public async Task<IActionResult> GetCompanies()
    {
        var companies = await _adminService.GetCompaniesAsync();
        return Ok(ApiResponse<IEnumerable<AdminCompanyResponseDto>>.Ok(companies));
    }

    [HttpPost("companies")]
    public async Task<IActionResult> CreateCompany([FromBody] AdminCompanyDto request)
    {
        var company = await _adminService.CreateCompanyAsync(request);
        return Ok(ApiResponse<AdminCompanyResponseDto>.Ok(company, "เพิ่มบริษัทสำเร็จ"));
    }

    [HttpPut("companies/{id}")]
    public async Task<IActionResult> UpdateCompany(int id, [FromBody] AdminCompanyDto request)
    {
        var company = await _adminService.UpdateCompanyAsync(id, request);
        return Ok(ApiResponse<AdminCompanyResponseDto>.Ok(company, "แก้ไขข้อมูลบริษัทสำเร็จ"));
    }

    [HttpDelete("companies/{id}")]
    public async Task<IActionResult> DeleteCompany(int id)
    {
        await _adminService.DeleteCompanyAsync(id);
        return Ok(ApiResponse<string>.Ok("ok", "ลบบริษัทเรียบร้อย"));
    }

    // ─── Internships ──────────────────────────────────────────────────────────

    [HttpGet("internships")]
    public async Task<IActionResult> GetInternships()
    {
        var internships = await _adminService.GetInternshipsAsync();
        return Ok(ApiResponse<object>.Ok(internships));
    }

    [HttpPost("internships")]
    public async Task<IActionResult> CreateInternship([FromBody] AdminCreateInternshipDto request)
    {
        var result = await _adminService.CreateInternshipAsync(request);
        return Ok(ApiResponse<object>.Ok(result, "สร้างการฝึกงานสำเร็จ"));
    }
}
