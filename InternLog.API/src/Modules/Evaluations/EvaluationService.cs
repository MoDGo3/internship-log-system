using Microsoft.EntityFrameworkCore;
using InternLog.API.Modules.Evaluations.DTOs;
using InternLog.API.Core.Interfaces;
using InternLog.API.Core.Common;
using InternLog.API.Infrastructure.Data;
using InternLog.API.Infrastructure.Data.Models;

namespace InternLog.API.Modules.Evaluations;

public class EvaluationService : IEvaluationService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public EvaluationService(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<EvaluationResponseDto>> GetEvaluationsAsync(int internshipId)
    {
        var userId = _currentUser.UserId;
        var role = _currentUser.Role;

        // ถ้าไม่ส่ง internshipId (= 0) ให้ auto-resolve จาก active internship ของ user ปัจจุบัน
        if (internshipId == 0)
        {
            if (role == Constants.Roles.Student)
            {
                var activeInternship = await _context.Internships
                    .FirstOrDefaultAsync(i => i.StudentID == userId && i.Status == "active");
                if (activeInternship == null)
                    return Enumerable.Empty<EvaluationResponseDto>();
                internshipId = activeInternship.InternshipID;
            }
            else
            {
                return Enumerable.Empty<EvaluationResponseDto>();
            }
        }

        var internship = await _context.Internships
            .Include(i => i.Student)
            .FirstOrDefaultAsync(i => i.InternshipID == internshipId);

        if (internship == null)
            throw AppException.NotFound("ไม่พบข้อมูลการฝึกงานนี้");

        // เช็คว่าคุณมีสิทธิ์ดูข้อมูลนี้หรือไม่
        bool canAccess = role == Constants.Roles.Admin
            || (role == Constants.Roles.Student && internship.StudentID == userId)
            || (role == Constants.Roles.Advisor && internship.AdvisorID == userId);

        if (!canAccess)
            throw AppException.Forbidden("คุณไม่มีสิทธิ์เข้าถึงข้อมูลนี้");

        var evaluations = await _context.Evaluations
            .Include(e => e.Internship).ThenInclude(i => i.Student)
            .Where(e => e.InternshipID == internshipId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        return evaluations.Select(MapToDto);
    }

    public async Task<EvaluationResponseDto> GetEvaluationDetailAsync(int evaluationId)
    {
        var evaluation = await _context.Evaluations
            .Include(e => e.Internship).ThenInclude(i => i.Student)
            .FirstOrDefaultAsync(e => e.EvaluationID == evaluationId);

        if (evaluation == null)
            throw AppException.NotFound("ไม่พบข้อมูลการประเมินนี้");

        return MapToDto(evaluation);
    }

    public async Task<EvaluationResponseDto> CreateEvaluationAsync(EvaluationRequestDto request)
    {
        var userId = _currentUser.UserId;
        var role = _currentUser.Role;

        var internship = await _context.Internships
            .Include(i => i.Student)
            .FirstOrDefaultAsync(i => i.InternshipID == request.InternshipID);

        if (internship == null)
            throw AppException.NotFound("ไม่พบข้อมูลการฝึกงานนี้");

        // advisor_eval: ทำได้เฉพาะ advisor
        // student_eval: ทำได้เฉพาะนักศึกษาเจ้าของ
        if (request.EvaluatorRole == "advisor_eval")
        {
            if (role != Constants.Roles.Advisor)
                throw AppException.Forbidden("เฉพาะ Advisor เท่านั้นที่ประเมินได้");

            if (internship.AdvisorID != userId)
                throw AppException.Forbidden("คุณไม่ใช่ Advisor ของการฝึกงานนี้");
        }
        else if (request.EvaluatorRole == "student_eval")
        {
            if (role != Constants.Roles.Student || internship.StudentID != userId)
                throw AppException.Forbidden("เฉพาะนักศึกษาเจ้าของการฝึกงานเท่านั้นที่ประเมินได้");
        }
        else
        {
            throw AppException.BadRequest("EvaluatorRole ไม่ถูกต้อง (ต้องเป็น advisor_eval หรือ student_eval)");
        }

        // เช็คว่าเคยประเมินประเภทนี้ไปแล้วหรือยัง (1 internship = 1 eval per type)
        bool exists = await _context.Evaluations
            .AnyAsync(e => e.InternshipID == request.InternshipID && e.EvaluatorRole == request.EvaluatorRole);
        if (exists)
            throw AppException.BadRequest($"มีการประเมิน {request.EvaluatorRole} สำหรับการฝึกงานนี้ไปแล้ว");

        var evaluation = new Evaluation
        {
            InternshipID = request.InternshipID,
            EvaluatorRole = request.EvaluatorRole,
            CriteriaJson = request.CriteriaJson,
            TotalScore = request.TotalScore,
            Grade = request.Grade,
            Comment = request.Comment,
            CreatedAt = AppClock.Now
        };

        _context.Evaluations.Add(evaluation);
        await _context.SaveChangesAsync();

        evaluation.Internship = internship;
        return MapToDto(evaluation);
    }

    private static EvaluationResponseDto MapToDto(Evaluation e) => new(
        e.EvaluationID,
        e.InternshipID,
        e.Internship?.StudentID ?? 0,
        e.Internship?.Student != null ? $"{e.Internship.Student.FirstName} {e.Internship.Student.LastName}" : string.Empty,
        e.EvaluatorRole,
        e.CriteriaJson,
        e.TotalScore,
        e.Grade,
        e.Comment,
        e.CreatedAt
    );
}
