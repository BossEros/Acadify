using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.DTOs;
using ASI.Basecode.Services.Results;
using ASI.Basecode.Resources.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Implementation
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthRepository _authRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            IUserRepository userRepository,
            IAuthRepository authRepository,
            IEmailService emailService,
            ILogger<AccountService> logger)
        {
            _userRepository = userRepository;
            _authRepository = authRepository;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<AuthResult> RegisterAsync(RegisterRequest request)
        {
            bool emailExists = await _userRepository.FindByEmailAsync(request.Email) != null;
            if (emailExists)
            {
                return AuthResult.Failure(new[] { "Email is already in use." });
            }

            var user = new User
            {
                UserName = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                IsApproved = request.Role == "Student" // Set IsApproved to true if the role is Student 
            };

            var (succeeded, errors) = await _userRepository.CreateUserAsync(user, request.Password);
            if (!succeeded)
            {
                return AuthResult.Failure(errors);
            }

            await _userRepository.AddToRoleAsync(user, request.Role);
            return AuthResult.Success();
        }

        public async Task<SignInAuthResult> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return SignInAuthResult.Failed(AccountMessages.InvalidLoginAttempt);
            }

            var (succeeded, isLockedOut) = await _authRepository.PasswordSignInAsync(
                user,
                request.Password,
                isPersistent: request.RememberMe,
                lockoutOnFailure: true);

            if (succeeded)
            {
                return SignInAuthResult.Success();
            }

            if (isLockedOut)
            {
                return SignInAuthResult.LockedOut();
            }

            return SignInAuthResult.Failed(AccountMessages.InvalidLoginAttempt);
        }

        public async Task SignOutAsync()
        {
            await _authRepository.SignOutAsync();
        }

        public async Task<IList<string>> GetUserRolesAsync(string email)
        {
            var user = await _userRepository.FindByEmailAsync(email);
            if (user == null)
            {
                return new List<string>();
            }

            return await _userRepository.GetRolesAsync(user);
        }

        public async Task<ForgotPasswordResult> SendPasswordResetTokenAsync(string email)
        {
            try
            {
                var user = await _userRepository.FindByEmailAsync(email);
                if (user != null)
                {
                    var token = await _userRepository.GeneratePasswordResetTokenAsync(user);
                    await _emailService.SendPasswordResetEmailAsync(user.Email!, user.UserName!, token);
                }

                return ForgotPasswordResult.Success();
            }
            catch (Exception)
            {
                return ForgotPasswordResult.EmailFailed(AccountMessages.PasswordResetFailed);
            }
        }

        public async Task<AuthResult> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _userRepository.FindByEmailAsync(email);
            if (user == null)
            {
                return AuthResult.Failure(new[] { AccountMessages.InvalidPasswordResetRequest });
            }

            var (succeeded, errors) = await _userRepository.ResetPasswordAsync(user, token, newPassword);
            if (succeeded)
            {
                return AuthResult.Success();
            }

            return AuthResult.Failure(errors);
        }

        public async Task<string> GetRedirectPathBasedOnRoleAsync(string email)
        {
            var user = await _userRepository.FindByEmailAsync(email);
            if (user == null)
            {
                return "/Home"; // Default redirect if user not found
            }

            var roles = await _userRepository.GetRolesAsync(user);

            return roles.FirstOrDefault() switch
            {
                "Admin" => "/Admin",
                "Teacher" => "/Teacher",
                "Student" => "/Home/Index",
                _ => "/Register"
            };
        }

        public async Task<User> GetCurrentUserAsync(string username)
        {
            try
            {
                var user = await _userRepository.FindByUserNameAsync(username);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user: {Username}", username);
                return null;
            }
        }

        public async Task<AuthResult> ChangePasswordAsync(string username, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _userRepository.FindByUserNameAsync(username);
                if (user == null)
                {
                    return AuthResult.Failure(new[] { "User not found." });
                }

                var result = await _userRepository.ChangePasswordAsync(user, currentPassword, newPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Password changed successfully for user: {Username}", username);
                    return AuthResult.Success();
                }

                return AuthResult.Failure(result.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user: {Username}", username);
                return AuthResult.Failure(new[] { "An error occurred while changing password." });
            }
        }

        public async Task<AuthResult> DeleteAccountAsync(string username)
        {
            try
            {
                var user = await _userRepository.FindByUserNameAsync(username);
                if (user == null)
                {
                    return AuthResult.Failure(new[] { "User not found." });
                }

                var result = await _userRepository.DeleteUserAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Account deleted successfully for user: {Username}", username);
                    return AuthResult.Success();
                }

                return AuthResult.Failure(result.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting account for user: {Username}", username);
                return AuthResult.Failure(new[] { "An error occurred while deleting account." });
            }
        }
    }
}