using ASI.Basecode.Services.DTOs;
using ASI.Basecode.Services.Results;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IAccountManagementService
    {
        Task<IEnumerable<UserManagementDto>> GetAllUsersAsync();
        Task<UserManagementDto?> GetUserByIdAsync(int id);
        Task<UserManagementDto?> GetUserByEmailAsync(string email);
        Task<UserManagementResult> CreateUserAsync(RegisterRequest request);
        Task<UserManagementResult> UpdateUserAsync(UpdateUserRequest request);
        Task<UserManagementResult> UpdateUserEmailAndStatusAsync(UpdateUserEmailAndStatusRequest request);
        Task<UserManagementResult> ChangePasswordAsync(ChangePasswordRequest request);
        Task<UserManagementResult> DeleteUserAsync(int id);
        Task<IEnumerable<string>> GetAvailableRolesAsync();
        Task<UserManagementResult> EnrollStudentAsync(int userId, string edpCode);
        Task<IEnumerable<EnrolledClassDto>> GetEnrolledClassesAsync(int userId);
        Task<UserManagementResult> UnenrollStudentAsync(int userId, string edpCode);
        Task<UserManagementResult> AssignTeacherAsync(int userId, string edpCode);
    }
}
