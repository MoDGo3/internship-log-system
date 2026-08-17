using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InternLog.API.Modules.Evaluations.DTOs;
using InternLog.API.Core.Common;

namespace InternLog.API.Modules.Evaluations;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EvaluationsController : ControllerBase
{
    private readonly IEvaluationService _evaluationService;

    public EvaluationsController(IEvaluationService evaluationService)
    {
        _evaluationService = evaluationService;
    }

    /// <summary>ดึงผลการประเมินทั้งหมดของ Internship นั้น</summary>
    [HttpGet]
    public async Task<IActionResult> GetEvaluations([FromQuery] int internshipId)
    {
        var evaluations = await _evaluationService.GetEvaluationsAsync(internshipId);
        return Ok(ApiResponse<IEnumerable<EvaluationResponseDto>>.Ok(evaluations));
    }

    /// <summary>ดูรายละเอียดการประเมิน 1 รายการ</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(int id)
    {
        var evaluation = await _evaluationService.GetEvaluationDetailAsync(id);
        return Ok(ApiResponse<EvaluationResponseDto>.Ok(evaluation));
    }

    /// <summary>สร้างการประเมิน (supervisor_eval หรือ student_eval)</summary>
    [HttpPost]
    [Authorize(Roles = "supervisor,advisor,student")]
    public async Task<IActionResult> CreateEvaluation([FromBody] EvaluationRequestDto request)
    {
        var result = await _evaluationService.CreateEvaluationAsync(request);
        return CreatedAtAction(nameof(GetDetail), new { id = result.EvaluationID },
            ApiResponse<EvaluationResponseDto>.Ok(result, "บันทึกการประเมินเรียบร้อย"));
    }
}
