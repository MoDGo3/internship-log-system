using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InternLog.API.Modules.AdvisorVisits.DTOs;
using InternLog.API.Core.Common;

namespace InternLog.API.Modules.AdvisorVisits;

[ApiController]
[Route("api/[controller]")]
public class AdvisorVisitsController : ControllerBase
{
    private readonly IAdvisorVisitService _visitService;

    public AdvisorVisitsController(IAdvisorVisitService visitService)
    {
        _visitService = visitService;
    }

    [HttpGet]
    [Authorize(Roles = "student,advisor")]
    public async Task<IActionResult> GetVisits([FromQuery] int internshipId)
    {
        var visits = await _visitService.GetVisitsAsync(internshipId);
        return Ok(ApiResponse<IEnumerable<AdvisorVisitResponseDto>>.Ok(visits));
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "student,advisor")]
    public async Task<IActionResult> GetVisitDetail(int id)
    {
        var visit = await _visitService.GetVisitDetailAsync(id);
        return Ok(ApiResponse<AdvisorVisitResponseDto>.Ok(visit));
    }

    [HttpPost]
    [Authorize(Roles = "advisor")]
    public async Task<IActionResult> CreateVisit([FromBody] AdvisorVisitRequestDto request)
    {
        var result = await _visitService.CreateVisitAsync(request);
        return CreatedAtAction(nameof(GetVisitDetail), new { id = result.VisitID }, ApiResponse<AdvisorVisitResponseDto>.Ok(result, "บันทึกการนิเทศเรียบร้อย"));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "advisor")]
    public async Task<IActionResult> UpdateVisit(int id, [FromBody] AdvisorVisitRequestDto request)
    {
        var result = await _visitService.UpdateVisitAsync(id, request);
        return Ok(ApiResponse<AdvisorVisitResponseDto>.Ok(result, "แก้ไขข้อมูลการนิเทศเรียบร้อย"));
    }
}
