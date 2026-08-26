using InternLog.API.Core.Common;
using InternLog.API.Modules.Evaluations.DTOs;
using InternLog.API.Modules.Evaluations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InternLog.API.Modules.Evaluations.Controllers;

[Authorize]
public class EvaluationsController : AppControllerBase {
    private readonly IEvaluationService _evaluationService;

    public EvaluationsController(IEvaluationService evaluationService) {
        _evaluationService = evaluationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetEvaluations() {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "";
        var list = await _evaluationService.GetEvaluationsAsync(GetUserId(), role);
        return Ok(new ApiResponse<List<EvaluationDto>>(true, "ดึงข้อมูลสำเร็จ", list));
    }

    [Authorize(Roles = "student,advisor")]
    [HttpPost]
    public async Task<IActionResult> CreateEvaluation([FromBody] CreateEvaluationRequest request) {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "";
        var result = await _evaluationService.CreateEvaluationAsync(GetUserId(), role, request);
        
        if (result == null) return BadRequest(new ApiResponse<object>(false, "ไม่สามารถประเมินได้ หรือประเมินไปแล้ว", null));
        
        return Ok(new ApiResponse<EvaluationDto>(true, "บันทึกการประเมินสำเร็จ", result));
    }
}
