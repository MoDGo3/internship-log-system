using InternLog.API.Core.Common;
using InternLog.API.Modules.Profile.DTOs;
using InternLog.API.Modules.Profile.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternLog.API.Modules.Profile.Controllers;

[Authorize]
public class ProfileController : AppControllerBase {
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService) {
        _profileService = profileService;
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request) {
        var success = await _profileService.UpdateProfileAsync(GetUserId(), request);
        if (!success) return NotFound(new ApiResponse<object>(false, "ไม่พบผู้ใช้", null));
        return Ok(new ApiResponse<object>(true, "อัปเดตโปรไฟล์สำเร็จ", null));
    }

    [Authorize(Roles = "student")]
    [HttpPut("company-contact")]
    public async Task<IActionResult> UpdateCompanyContact([FromBody] UpdateCompanyContactRequest request) {
        var success = await _profileService.UpdateCompanyContactAsync(GetUserId(), request);
        if (!success) return NotFound(new ApiResponse<object>(false, "ไม่พบข้อมูลการฝึกงานที่กำลังดำเนินการ", null));
        return Ok(new ApiResponse<object>(true, "อัปเดตข้อมูลบริษัทสำเร็จ", null));
    }
}
