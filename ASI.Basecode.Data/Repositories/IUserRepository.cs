namespace ASI.Basecode.Data.Repositories;

using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IUserRepository
{
    Task<(bool Succeeded, IEnumerable<string> Errors)> CreateUserAsync(User user, string password);
    Task AddToRoleAsync(User user, string role);
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByIdAsync(int id);
    Task<User?> FindByUserNameAsync(string userName);
    Task<IList<string>> GetRolesAsync(User user);
    Task<string> GeneratePasswordResetTokenAsync(User user);
    Task<(bool Succeeded, IEnumerable<string> Errors)> ResetPasswordAsync(User user, string token, string newPassword);
    Task<string> GenerateEmailConfirmationTokenAsync(User user);
    Task<(bool Succeeded, IEnumerable<string> Errors)> ConfirmEmailAsync(User user, string token);
    Task<(bool Succeeded, IEnumerable<string> Errors)> UpdateUserAsync(User user);
    Task<(bool Succeeded, IEnumerable<string> Errors)> DeleteUserAsync(User user);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<(bool Succeeded, IEnumerable<string> Errors)> ChangePasswordAsync(User user, string currentPassword, string newPassword);
    Task<int> GetUserCountByRoleAsync(string roleName);
    Task<bool> CheckPasswordAsync(User user, string password);
   


}




