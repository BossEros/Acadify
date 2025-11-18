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
            // Check if email is already taken
            bool emailExists = await _userRepository.FindByEmailAsync(request.Email) != null;
            if (emailExists)
            {
                return AuthResult.Failure(new[] { "Email is already in use." });
            }

            // Generate unique ID number based on role
            var idNumber = await GenerateUserIdNumberAsync(request.Role);

            // Create user entity
            var user = new User
            {
                UserName = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                IdNumber = idNumber,
                IsApproved = IsUserAutoApproved(request.Role)
            };

            // Save user to database
            var (succeeded, errors) = await _userRepository.CreateUserAsync(user, request.Password);
            if (!succeeded)
            {
                return AuthResult.Failure(errors);
            }

            // Assign role to user
            await _userRepository.AddToRoleAsync(user, request.Role);

            // Send email verification (non-blocking - continue even if it fails)
            await SendEmailVerificationAsync(user.Email!);

            return AuthResult.Success();
        }

        public async Task<SignInAuthResult> LoginAsync(LoginRequest request)
        {
            // Find user by email
            var user = await _userRepository.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Return unified error message to prevent user enumeration
                return SignInAuthResult.Failed(AccountMessages.IncorrectCredentials);
            }

            // Attempt sign-in - this will track failed attempts for lockout
            // We check password correctness after to determine the right error message
            var (signInSucceeded, isLockedOut) = await _authRepository.PasswordSignInAsync(
                user,
                request.Password,
                isPersistent: false,
                lockoutOnFailure: true);

            if (isLockedOut)
            {
                return SignInAuthResult.LockedOut();
            }

            if (!signInSucceeded)
            {
                // Sign-in failed - check if it's due to wrong password
                // Use unified error message for security (prevents user enumeration)
                var isPasswordCorrect = await _userRepository.CheckPasswordAsync(user, request.Password);
                
                if (!isPasswordCorrect)
                {
                    // Password is incorrect - PasswordSignInAsync already tracked the failed attempt
                    // Return unified error message instead of specific "incorrect password" message
                    return SignInAuthResult.Failed(AccountMessages.IncorrectCredentials);
                }
            }

            // Sign-in succeeded - verify account status is still valid before allowing access
            var accountStatusResult = ValidateUserAccountStatus(user);
            if (accountStatusResult != null)
            {
                // Sign out since account status is invalid
                await _authRepository.SignOutAsync();
                return accountStatusResult;
            }

            // All validations passed - sign-in was successful
            return SignInAuthResult.Success();
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

                // Always return success, even if user not found
                // Security: Don't reveal whether an email exists in the system
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
                _logger.LogInformation("Password reset successful for user: {Email}", email);

                // Send password changed confirmation email
                try
                {
                    var userName = $"{user.FirstName} {user.LastName}";
                    await _emailService.SendPasswordChangedEmailAsync(user.Email!, userName);
                    _logger.LogInformation("Password changed email sent successfully to {Email}", user.Email);
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Failed to send password changed email to {Email}, but password was reset successfully", user.Email);
                    // Don't fail the password reset operation due to email failure
                }

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
                    // Security: Don't reveal if email exists or not
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

                    // Send password changed confirmation email
                    try
                    {
                        var userName = $"{user.FirstName} {user.LastName}";
                        await _emailService.SendPasswordChangedEmailAsync(user.Email!, userName);
                        _logger.LogInformation("Password changed email sent successfully to {Email}", user.Email);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogWarning(emailEx, "Failed to send password changed email to {Email}, but password was changed successfully", user.Email);
                        // Don't fail the password change operation due to email failure
                    }

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

        #region Private Helper Methods

        /// <summary>
        /// Generates a unique ID number for a user based on their role.
        /// Students start at 2320001, Teachers start at 1047001.
        /// </summary>
        private async Task<int> GenerateUserIdNumberAsync(string role)
        {
            var roleCount = await _userRepository.GetUserCountByRoleAsync(role);
            var startingNumber = role == "Student" ? 2320001 : 1047001;
            return startingNumber + roleCount;
        }

        /// <summary>
        /// Determines if a user should be auto-approved based on their role.
        /// Students are auto-approved, Teachers and Admins require manual approval.
        /// </summary>
        private bool IsUserAutoApproved(string role)
        {
            return role == "Student";
        }

        /// <summary>
        /// Validates user account status for login (checks deleted, approved, email confirmed).
        /// Returns an error result if validation fails, null if validation passes.
        /// </summary>
        private SignInAuthResult? ValidateUserAccountStatus(User user)
        {
            // Check if account is deleted
            if (user.IsDeleted)
            {
                _logger.LogWarning("Login attempt for deleted account: {Email}", user.Email);
                return SignInAuthResult.Failed(AccountMessages.AccountDeactivated);
            }

            // Check if account is approved
            if (!user.IsApproved)
            {
                _logger.LogWarning("Login attempt for unapproved account: {Email}", user.Email);
                return SignInAuthResult.Failed(AccountMessages.AccountPendingApproval);
            }

            // Check if email is confirmed
            if (!user.EmailConfirmed)
            {
                _logger.LogWarning("Login attempt for unverified email: {Email}", user.Email);
                return SignInAuthResult.Failed(AccountMessages.EmailNotVerified);
            }

            return null; // All validations passed
        }

        #endregion
    }
}