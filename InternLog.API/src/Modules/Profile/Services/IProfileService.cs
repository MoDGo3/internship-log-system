using InternLog.API.Modules.Profile.DTOs;

namespace InternLog.API.Modules.Profile.Services;

public interface IProfileService {
    Task<bool> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    Task<bool> UpdateCompanyContactAsync(int studentId, UpdateCompanyContactRequest request);
}
