using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.DTOs;
using ASI.Basecode.Services.Results;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IAccountService
    {
        Task<AuthResult> RegisterAsync(RegisterRequest request);
        Task<SignInAuthResult> LoginAsync(LoginRequest request);
        Task SignOutAsync();
        Task<IList<string>> GetUserRolesAsync(string email);
        Task<ForgotPasswordResult> SendPasswordResetTokenAsync(string email);
        Task<AuthResult> ResetPasswordAsync(string email, string token, string newPassword);
        Task<string> GetRedirectPathBasedOnRoleAsync(string email);

        // New methods for Settings
        Task<User> GetCurrentUserAsync(string username);
        Task<AuthResult> ChangePasswordAsync(string username, string currentPassword, string newPassword);
        Task<AuthResult> DeleteAccountAsync(string username);
    }
}