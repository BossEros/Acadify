using ASI.Basecode.Services.DTOs;
using ASI.Basecode.Services.Results;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IUserManagementService
    {
        Task<IEnumerable<UserManagementDto>> GetAllUsersAsync();
        Task<UserManagementDto?> GetUserByIdAsync(int id);
        Task<UserManagementResult> CreateUserAsync(CreateUserRequest request);
        Task<UserManagementResult> UpdateUserAsync(UpdateUserRequest request);
        Task<UserManagementResult> ChangePasswordAsync(ChangePasswordRequest request);
        Task<UserManagementResult> DeleteUserAsync(int id);
        Task<IEnumerable<string>> GetAvailableRolesAsync();
    }
}
