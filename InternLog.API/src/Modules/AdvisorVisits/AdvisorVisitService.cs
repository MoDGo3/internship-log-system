using Microsoft.EntityFrameworkCore;
using InternLog.API.Modules.AdvisorVisits.DTOs;
using InternLog.API.Core.Interfaces;
using InternLog.API.Core.Common;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Infrastructure.Data.Models;

namespace InternLog.API.Modules.AdvisorVisits;

public class AdvisorVisitService : IAdvisorVisitService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AdvisorVisitService(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<AdvisorVisitResponseDto>> GetVisitsAsync(int internshipId)
    {
        var userId = _currentUser.UserId;
        var role = _currentUser.Role;

        var internship = await _context.Internships.FirstOrDefaultAsync(i => i.InternshipID == internshipId);
        if (internship == null)
            throw AppException.NotFound("ไม่พบข้อมูลการฝึกงานนี้");

        if (role == Constants.Roles.Student && internship.StudentID != userId)
            throw AppException.Forbidden("คุณไม่มีสิทธิ์เข้าถึงข้อมูลนี้");
            
        if (role == Constants.Roles.Advisor && internship.AdvisorID != userId)
            throw AppException.Forbidden("คุณไม่มีสิทธิ์เข้าถึงข้อมูลนี้");

        var visits = await _context.AdvisorVisits
            .Include(v => v.Internship).ThenInclude(i => i.Student)
            .Include(v => v.Advisor)
            .Where(v => v.InternshipID == internshipId)
            .OrderByDescending(v => v.VisitDate)
            .ToListAsync();

        return visits.Select(MapToDto);
    }

    public async Task<AdvisorVisitResponseDto> GetVisitDetailAsync(int visitId)
    {
        var visit = await _context.AdvisorVisits
            .Include(v => v.Internship).ThenInclude(i => i.Student)
            .Include(v => v.Advisor)
            .FirstOrDefaultAsync(v => v.VisitID == visitId);

        if (visit == null)
            throw AppException.NotFound("ไม่พบข้อมูลการนิเทศนี้");

        ValidateAccess(visit.Internship);

        return MapToDto(visit);
    }

    public async Task<AdvisorVisitResponseDto> CreateVisitAsync(AdvisorVisitRequestDto request)
    {
        var advisorId = _currentUser.UserId;
        
        var internship = await _context.Internships
            .Include(i => i.Student)
            .FirstOrDefaultAsync(i => i.InternshipID == request.InternshipID);

        if (internship == null)
            throw AppException.NotFound("ไม่พบข้อมูลการฝึกงานนี้");

        if (internship.AdvisorID != advisorId)
            throw AppException.Forbidden("คุณไม่ใช่ที่ปรึกษาของการฝึกงานนี้");

        // Check for UNIQUE(InternshipID, VisitNo)
        bool exists = await _context.AdvisorVisits.AnyAsync(v => v.InternshipID == request.InternshipID && v.VisitNo == request.VisitNo);
        if (exists)
            throw AppException.BadRequest($"มีการบันทึกการนิเทศครั้งที่ {request.VisitNo} สำหรับการฝึกงานนี้ไปแล้ว");

        var visit = new AdvisorVisit
        {
            InternshipID = request.InternshipID,
            PersonNo = request.PersonNo,
            VisitNo = request.VisitNo,
            VisitDate = request.VisitDate,
            AdvisorID = advisorId,
            VisitType = request.VisitType,
            Summary = request.Summary,
            Score = request.Score,
            CreatedAt = AppClock.Now
        };

        _context.AdvisorVisits.Add(visit);
        await _context.SaveChangesAsync();

        visit.Internship = internship;
        visit.Advisor = await _context.Users.FirstAsync(u => u.UserID == advisorId);

        return MapToDto(visit);
    }

    public async Task<AdvisorVisitResponseDto> UpdateVisitAsync(int visitId, AdvisorVisitRequestDto request)
    {
        var advisorId = _currentUser.UserId;

        var visit = await _context.AdvisorVisits
            .Include(v => v.Internship).ThenInclude(i => i.Student)
            .Include(v => v.Advisor)
            .FirstOrDefaultAsync(v => v.VisitID == visitId);

        if (visit == null)
            throw AppException.NotFound("ไม่พบข้อมูลการนิเทศนี้");

        if (visit.AdvisorID != advisorId)
            throw AppException.Forbidden("แก้ไขได้เฉพาะการนิเทศของตนเองเท่านั้น");

        // If VisitNo changed, check uniqueness
        if (visit.VisitNo != request.VisitNo)
        {
            bool exists = await _context.AdvisorVisits.AnyAsync(v => v.InternshipID == request.InternshipID && v.VisitNo == request.VisitNo);
            if (exists)
                throw AppException.BadRequest($"มีการบันทึกการนิเทศครั้งที่ {request.VisitNo} สำหรับการฝึกงานนี้ไปแล้ว");
        }

        visit.PersonNo = request.PersonNo;
        visit.VisitNo = request.VisitNo;
        visit.VisitDate = request.VisitDate;
        visit.VisitType = request.VisitType;
        visit.Summary = request.Summary;
        visit.Score = request.Score;

        await _context.SaveChangesAsync();

        return MapToDto(visit);
    }

    private void ValidateAccess(Internship internship)
    {
        var userId = _currentUser.UserId;
        var role = _currentUser.Role;

        if (role == Constants.Roles.Student && internship.StudentID != userId)
            throw AppException.Forbidden("คุณไม่มีสิทธิ์เข้าถึงข้อมูลนี้");

        if (role == Constants.Roles.Advisor && internship.AdvisorID != userId)
            throw AppException.Forbidden("คุณไม่มีสิทธิ์เข้าถึงข้อมูลนี้");
    }

    private static AdvisorVisitResponseDto MapToDto(AdvisorVisit visit)
    {
        return new AdvisorVisitResponseDto(
            visit.VisitID,
            visit.InternshipID,
            visit.Internship?.StudentID ?? 0,
            visit.Internship?.Student != null ? $"{visit.Internship.Student.FirstName} {visit.Internship.Student.LastName}" : string.Empty,
            visit.PersonNo,
            visit.VisitNo,
            visit.VisitDate,
            visit.AdvisorID,
            visit.Advisor != null ? $"{visit.Advisor.FirstName} {visit.Advisor.LastName}" : string.Empty,
            visit.VisitType,
            visit.Summary,
            visit.Score,
            visit.CreatedAt
        );
    }
}
