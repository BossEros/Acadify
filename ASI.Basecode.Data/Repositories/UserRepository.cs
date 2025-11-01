namespace ASI.Basecode.Data.Repositories;

using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class UserRepository : IUserRepository
{
    private readonly UserManager<User> _userManager;

    public UserRepository(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<(bool Succeeded, IEnumerable<string> Errors)> CreateUserAsync(User user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }

    public async Task AddToRoleAsync(User user, string role)
    {
        await _userManager.AddToRoleAsync(user, role);
    }

    public Task<User?> FindByEmailAsync(string email) => _userManager.FindByEmailAsync(email);

    public Task<User?> FindByIdAsync(int id) => _userManager.FindByIdAsync(id.ToString());

    public Task<User?> FindByUserNameAsync(string userName) => _userManager.FindByNameAsync(userName);

    public Task<IList<string>> GetRolesAsync(User user) => _userManager.GetRolesAsync(user);

    public Task<string> GeneratePasswordResetTokenAsync(User user) => _userManager.GeneratePasswordResetTokenAsync(user);

    public async Task<(bool Succeeded, IEnumerable<string> Errors)> ResetPasswordAsync(User user, string token, string newPassword)
    {
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }

    public async Task<(bool Succeeded, IEnumerable<string> Errors)> UpdateUserAsync(User user)
    {
        var result = await _userManager.UpdateAsync(user);
        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }

    public async Task<(bool Succeeded, IEnumerable<string> Errors)> DeleteUserAsync(User user)
    {
        var result = await _userManager.DeleteAsync(user);
        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _userManager.Users.ToListAsync();
    }

    public async Task<(bool Succeeded, IEnumerable<string> Errors)> ChangePasswordAsync(User user, string currentPassword, string newPassword)
    {
        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }
}


