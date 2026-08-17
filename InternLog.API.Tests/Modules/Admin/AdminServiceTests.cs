using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using InternLog.API.Core.Common;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Infrastructure.Data.Models;
using InternLog.API.Modules.Admin;
using InternLog.API.Modules.Admin.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InternLog.API.Tests.Modules.Admin;

public class AdminServiceTests
{
    private readonly AppDbContext _db;
    private readonly AdminService _adminService;

    public AdminServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _adminService = new AdminService(_db);

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        _db.Roles.Add(new Role { RoleID = 1, RoleName = "admin" });
        _db.Roles.Add(new Role { RoleID = 2, RoleName = "student" });

        _db.Users.Add(new User { UserID = 1, RoleID = 2, Email = "student@test.com", FirstName = "Test", LastName = "Student", IsActive = true });
        _db.Users.Add(new User { UserID = 2, RoleID = 1, Email = "admin@test.com", FirstName = "Test", LastName = "Admin", IsActive = true });

        _db.Companies.Add(new Company { CompanyID = 1, CompanyName = "Tech Corp" });

        _db.SaveChanges();
    }

    [Fact]
    public async Task CreateUserAsync_ValidUser_ReturnsUser()
    {
        var req = new AdminCreateUserDto("new@test.com", "password", "New", "User", 2, null, null);
        var result = await _adminService.CreateUserAsync(req);

        Assert.NotNull(result);
        Assert.Equal("new@test.com", result.Email);
        Assert.Equal("New", result.FirstName);
    }

    [Fact]
    public async Task UpdateUserAsync_ValidUser_UpdatesData()
    {
        var req = new AdminUpdateUserDto("Updated", null, null, null, null, null);
        var result = await _adminService.UpdateUserAsync(1, req);

        Assert.Equal("Updated", result.FirstName);
        Assert.Equal("Student", result.LastName); // unchanged
    }

    [Fact]
    public async Task DeactivateUserAsync_ValidUser_Deactivates()
    {
        await _adminService.DeleteUserAsync(1);

        var user = await _db.Users.FindAsync(1);
        Assert.False(user.IsActive);
    }

    [Fact]
    public async Task CreateCompanyAsync_ValidCompany_ReturnsCompany()
    {
        var req = new AdminCompanyDto("New Corp", null, null, null);
        var result = await _adminService.CreateCompanyAsync(req);

        Assert.NotNull(result);
        Assert.Equal("New Corp", result.CompanyName);
    }

    [Fact]
    public async Task CreateInternshipAsync_ValidInternship_ReturnsInternship()
    {
        var req = new AdminCreateInternshipDto(1, 1, 2, null, DateOnly.FromDateTime(DateTime.Now), DateOnly.FromDateTime(DateTime.Now.AddMonths(3)), 60);
        var result = await _adminService.CreateInternshipAsync(req);

        Assert.NotNull(result);
    }

    [Fact]
    public void AdminController_HasAuthorizeAttributeWithAdminRole()
    {
        // ADM-06: Student calls Admin API -> 403 Forbidden
        // Test that the AdminController has [Authorize(Roles = "admin")]
        var type = typeof(AdminController);
        var authorizeAttribute = type.GetCustomAttributes(typeof(AuthorizeAttribute), true).FirstOrDefault() as AuthorizeAttribute;

        Assert.NotNull(authorizeAttribute);
        Assert.Equal("admin", authorizeAttribute.Roles);
    }
}
