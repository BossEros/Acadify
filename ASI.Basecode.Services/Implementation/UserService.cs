using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<User> GetCurrentUserAsync(string username)
        {
            return await _userRepository.FindByUserNameAsync(username);
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> ChangePasswordAsync(string username, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _userRepository.FindByUserNameAsync(username);
                if (user == null)
                {
                    return (false, new List<string> { "User not found." });
                }

                var result = await _userRepository.ChangePasswordAsync(user, currentPassword, newPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Password changed successfully for user: {Username}", username);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user: {Username}", username);
                return (false, new List<string> { "An error occurred while changing password." });
            }
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> DeleteAccountAsync(string username)
        {
            try
            {
                var user = await _userRepository.FindByUserNameAsync(username);
                if (user == null)
                {
                    return (false, new List<string> { "User not found." });
                }

                var result = await _userRepository.DeleteUserAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Account deleted successfully for user: {Username}", username);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting account for user: {Username}", username);
                return (false, new List<string> { "An error occurred while deleting account." });
            }
        }
    }
}