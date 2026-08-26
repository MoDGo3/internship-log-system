using InternLog.API.Infrastructure.Data;
using InternLog.API.Modules.Evaluations.DTOs;
using InternLog.API.Modules.Evaluations.Models;
using Microsoft.EntityFrameworkCore;

namespace InternLog.API.Modules.Evaluations.Services;

public class EvaluationService : IEvaluationService {
    private readonly AppDbContext _context;

    public EvaluationService(AppDbContext context) {
        _context = context;
    }

    public async Task<List<EvaluationDto>> GetEvaluationsAsync(int userId, string role) {
        var query = _context.Evaluations.AsQueryable();

        if (role == "student") {
            query = query.Where(e => e.Internship.StudentID == userId);
        } else if (role == "advisor") {
            query = query.Where(e => e.Internship.AdvisorID == userId);
        }

        var results = await query.ToListAsync();
        return results.Select(e => new EvaluationDto {
            EvaluationID = e.EvaluationID,
            InternshipID = e.InternshipID,
            EvaluatorID = e.EvaluatorID,
            EvalType = e.EvalType,
            TotalScore = e.TotalScore,
            Comment = e.Comment,
            EvaluatedAt = e.EvaluatedAt
        }).ToList();
    }

    public async Task<EvaluationDto?> CreateEvaluationAsync(int userId, string role, CreateEvaluationRequest request) {
        var internship = await _context.Internships.FindAsync(request.InternshipID);
        if (internship == null) return null;

        string evalType = "";
        
        if (role == "student") {
            if (internship.StudentID != userId) return null;
            evalType = "student_eval";
        } else if (role == "advisor") {
            if (internship.AdvisorID != userId) return null;
            evalType = "advisor_eval";
        } else {
            return null;
        }

        // Check if already evaluated
        var existing = await _context.Evaluations.FirstOrDefaultAsync(e => e.InternshipID == request.InternshipID && e.EvalType == evalType);
        if (existing != null) return null; // Already evaluated

        var evaluation = new Evaluation {
            InternshipID = request.InternshipID,
            EvaluatorID = userId,
            EvalType = evalType,
            TotalScore = request.TotalScore,
            Comment = request.Comment,
            EvaluatedAt = DateTime.UtcNow
        };

        _context.Evaluations.Add(evaluation);
        await _context.SaveChangesAsync();

        return new EvaluationDto {
            EvaluationID = evaluation.EvaluationID,
            InternshipID = evaluation.InternshipID,
            EvaluatorID = evaluation.EvaluatorID,
            EvalType = evaluation.EvalType,
            TotalScore = evaluation.TotalScore,
            Comment = evaluation.Comment,
            EvaluatedAt = evaluation.EvaluatedAt
        };
    }
}
