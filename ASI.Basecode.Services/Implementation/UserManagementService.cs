using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.DTOs;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ASI.Basecode.Services.Implementation
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public UserManagementService(
            IUserRepository userRepository,
            UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IEnumerable<UserManagementDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.Id)
                .ToListAsync();

            var userDtos = new List<UserManagementDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var primaryRole = roles.FirstOrDefault() ?? "Student";

                userDtos.Add(new UserManagementDto
                {
                    Id = user.Id,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = primaryRole,
                    IsActive = user.IsApproved,
                    CreatedAt = user.CreatedAt
                });
            }

            return userDtos;
        }

        public async Task<UserManagementDto?> GetUserByIdAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault() ?? "Student";

            return new UserManagementDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = primaryRole,
                IsActive = user.IsApproved,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<UserManagementResult> CreateUserAsync(CreateUserRequest request)
        {
            try
            {
                // Check if username or email already exists
                var existingUserByUsername = await _userRepository.FindByUserNameAsync(request.UserName);
                var existingUserByEmail = await _userRepository.FindByEmailAsync(request.Email);

                if (existingUserByUsername != null)
                {
                    return UserManagementResult.Failure("Username already exists.");
                }

                if (existingUserByEmail != null)
                {
                    return UserManagementResult.Failure("Email already exists.");
                }

                // Create new user
                var user = new User
                {
                    UserName = request.UserName,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    IsApproved = request.IsActive,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var (succeeded, errors) = await _userRepository.CreateUserAsync(user, request.Password);

                if (succeeded)
                {
                    await _userRepository.AddToRoleAsync(user, request.Role);
                    return UserManagementResult.Success($"User '{user.FirstName} {user.LastName}' created successfully!");
                }

                return UserManagementResult.Failure(errors);
            }
            catch (Exception ex)
            {
                return UserManagementResult.Failure($"Error creating user: {ex.Message}");
            }
        }

        public async Task<UserManagementResult> UpdateUserAsync(UpdateUserRequest request)
        {
            try
            {
                var user = await _userRepository.FindByIdAsync(request.Id);
                if (user == null)
                {
                    return UserManagementResult.Failure("User not found.");
                }

                // Check if new username conflicts with other users
                var existingUserByUsername = await _userRepository.FindByUserNameAsync(request.UserName);
                if (existingUserByUsername != null && existingUserByUsername.Id != user.Id)
                {
                    return UserManagementResult.Failure("Username already exists.");
                }

                // Update user properties
                user.UserName = request.UserName;
                user.IsApproved = request.IsActive;

                var (succeeded, errors) = await _userRepository.UpdateUserAsync(user);

                if (succeeded)
                {
                    return UserManagementResult.Success($"User '{user.FirstName} {user.LastName}' updated successfully!");
                }

                return UserManagementResult.Failure(errors);
            }
            catch (Exception ex)
            {
                return UserManagementResult.Failure($"Error updating user: {ex.Message}");
            }
        }

        public async Task<UserManagementResult> ChangePasswordAsync(ChangePasswordRequest request)
        {
            try
            {
                var user = await _userRepository.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return UserManagementResult.Failure("User not found.");
                }

                // Remove current password and add new one
                var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                if (!removePasswordResult.Succeeded)
                {
                    return UserManagementResult.Failure(removePasswordResult.Errors.Select(e => e.Description));
                }

                var addPasswordResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
                if (addPasswordResult.Succeeded)
                {
                    return UserManagementResult.Success($"Password for user '{user.FirstName} {user.LastName}' changed successfully!");
                }

                return UserManagementResult.Failure(addPasswordResult.Errors.Select(e => e.Description));
            }
            catch (Exception ex)
            {
                return UserManagementResult.Failure($"Error changing password: {ex.Message}");
            }
        }

        public async Task<UserManagementResult> DeleteUserAsync(int id)
        {
            try
            {
                var user = await _userRepository.FindByIdAsync(id);
                if (user == null)
                {
                    return UserManagementResult.Failure("User not found.");
                }

                // Prevent deleting Admins
                var roles = await _userRepository.GetRolesAsync(user);
                if (roles.Contains("Admin"))
                {
                    return UserManagementResult.Failure("Admins cannot be deleted.");
                }

                var (succeeded, errors) = await _userRepository.DeleteUserAsync(user);
                if (succeeded)
                {
                    return UserManagementResult.Success($"User '{user.FirstName} {user.LastName}' deleted successfully!");
                }

                return UserManagementResult.Failure(errors);
            }
            catch (Exception ex)
            {
                return UserManagementResult.Failure($"Error deleting user: {ex.Message}");
            }
        }

        public async Task<IEnumerable<string>> GetAvailableRolesAsync()
        {
            var roles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
            return roles;
        }
    }
}
