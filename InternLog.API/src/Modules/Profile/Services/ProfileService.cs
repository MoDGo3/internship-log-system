using InternLog.API.Infrastructure.Data;
using InternLog.API.Modules.Profile.DTOs;
using Microsoft.EntityFrameworkCore;

namespace InternLog.API.Modules.Profile.Services;

public class ProfileService : IProfileService {
    private readonly AppDbContext _context;

    public ProfileService(AppDbContext context) {
        _context = context;
    }

    public async Task<bool> UpdateProfileAsync(int userId, UpdateProfileRequest request) {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        if (request.FirstName != null) user.FirstName = request.FirstName;
        if (request.LastName != null) user.LastName = request.LastName;
        if (request.Phone != null) user.Phone = request.Phone;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateCompanyContactAsync(int studentId, UpdateCompanyContactRequest request) {
        var internship = await _context.Internships
            .FirstOrDefaultAsync(i => i.StudentID == studentId && i.Status == "active");
            
        if (internship == null) return false;

        internship.CompanyAddress = request.CompanyAddress;
        internship.CompanyPhone = request.CompanyPhone;
        
        await _context.SaveChangesAsync();
        return true;
    }
}
