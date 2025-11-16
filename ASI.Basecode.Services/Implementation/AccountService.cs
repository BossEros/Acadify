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

            // Generate IdNumber before creating user: Students start at 2320001, Teachers start at 1047001
            var roleCount = await _userRepository.GetUserCountByRoleAsync(request.Role);
            var startingNumber = request.Role == "Student" ? 2320001 : 1047001;
            var idNumber = startingNumber + roleCount;

            var user = new User
            {
                UserName = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                IdNumber = idNumber,
                IsApproved = request.Role == "Student" // Set IsApproved to true if the role is Student
            };

            var (succeeded, errors) = await _userRepository.CreateUserAsync(user, request.Password);
            if (!succeeded)
            {
                return AuthResult.Failure(errors);
            }

            await _userRepository.AddToRoleAsync(user, request.Role);

            // Send email verification (non-blocking - continue even if it fails)
            await SendEmailVerificationAsync(user.Email!);

            return AuthResult.Success();
        }

        public async Task<SignInAuthResult> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return SignInAuthResult.Failed(AccountMessages.InvalidLoginAttempt);
            }

            // Check if account is deleted
            if (user.IsDeleted)
            {
                _logger.LogWarning("Login attempt for deleted account: {Email}", request.Email);
                return SignInAuthResult.Failed("Invalid login attempt. Please try again.");
            }

            // Check if account is approved
            if (!user.IsApproved)
            {
                _logger.LogWarning("Login attempt for unapproved account: {Email}", request.Email);
                return SignInAuthResult.Failed("Your account is pending approval. Please contact an administrator.");
            }

            // Check if email is confirmed
            if (!user.EmailConfirmed)
            {
                _logger.LogWarning("Login attempt for unverified email: {Email}", request.Email);
                return SignInAuthResult.Failed("Please verify your email address before logging in. Check your inbox for the verification link.");
            }

            var (succeeded, isLockedOut) = await _authRepository.PasswordSignInAsync(
                user,
                request.Password,
                isPersistent: false,
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

        public async Task<AuthResult> ConfirmEmailAsync(string email, string token)
        {
            var user = await _userRepository.FindByEmailAsync(email);
            if (user == null)
            {
                return AuthResult.Failure(new[] { "Invalid email confirmation request." });
            }

            var (succeeded, errors) = await _userRepository.ConfirmEmailAsync(user, token);
            if (succeeded)
            {
                _logger.LogInformation("Email confirmed successfully for user: {Email}", email);
                return AuthResult.Success();
            }

            return AuthResult.Failure(errors);
        }

        public async Task<AuthResult> SendEmailVerificationAsync(string email)
        {
            try
            {
                var user = await _userRepository.FindByEmailAsync(email);
                if (user == null)
                {
                    // For security, don't reveal if email exists or not
                    return AuthResult.Success();
                }

                // Check if email is already confirmed
                if (user.EmailConfirmed)
                {
                    return AuthResult.Failure(new[] { "Email is already verified." });
                }

                // Generate and send email verification token
                var token = await _userRepository.GenerateEmailConfirmationTokenAsync(user);
                await _emailService.SendEmailVerificationAsync(user.Email!, $"{user.FirstName} {user.LastName}", token);
                _logger.LogInformation("Email verification sent to: {Email}", user.Email);

                return AuthResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email verification to: {Email}", email);
                return AuthResult.Failure(new[] { "Failed to send verification email. Please try again later." });
            }
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
                "Admin" => "/AdminDashboard/Index",
                "Teacher" => "/ClassManagement/TeacherDashboard",
                "Student" => "/Home/StudentDashboard",
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
                _logger.LogInformation("ChangePasswordAsync called for user: {Username}", username);

                var user = await _userRepository.FindByUserNameAsync(username);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {Username}", username);
                    return AuthResult.Failure(new[] { "User not found." });
                }

                _logger.LogInformation("Attempting to change password for user: {UserId} - {Email}", user.Id, user.Email);

                var result = await _userRepository.ChangePasswordAsync(user, currentPassword, newPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Password changed successfully for user: {Username}", username);
                    return AuthResult.Success();
                }

                // Log each error individually
                foreach (var error in result.Errors)
                {
                    _logger.LogWarning("Password change error for {Username}: {Error}", username, error);
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

                // Soft delete: Set IsDeleted to true instead of physically deleting
                user.IsDeleted = true;
                var result = await _userRepository.UpdateUserAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Account marked as deleted for user: {Username}", username);
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