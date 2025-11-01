using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> GetCurrentUserAsync(string username);
        Task<(bool Succeeded, IEnumerable<string> Errors)> ChangePasswordAsync(string username, string currentPassword, string newPassword);
        Task<(bool Succeeded, IEnumerable<string> Errors)> DeleteAccountAsync(string username);
    }
}