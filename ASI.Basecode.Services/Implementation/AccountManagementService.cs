using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.DTOs;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ASI.Basecode.Services.Implementation
{
    public class AccountManagementService : IAccountManagementService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public AccountManagementService(
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

        public async Task<UserManagementDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
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

        public async Task<UserManagementResult> CreateUserAsync(RegisterRequest request)
        {
            try
            {
                // Check if username or email already exists
                var existingUserByUsername = await _userRepository.FindByUserNameAsync(request.Email); // Using email as username
                var existingUserByEmail = await _userRepository.FindByEmailAsync(request.Email);

                if (existingUserByUsername != null)
                {
                    return UserManagementResult.Failure("Email already exists.");
                }

                if (existingUserByEmail != null)
                {
                    return UserManagementResult.Failure("Email already exists.");
                }

                // Create new user
                var user = new User
                {
                    UserName = request.Email, // Using email as username like in AccountService
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    IsApproved = request.Role == "Student", // Set IsApproved based on role like in AccountService
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

        public async Task<UserManagementResult> UpdateUserEmailAndStatusAsync(UpdateUserEmailAndStatusRequest request)
        {
            try
            {
                if (request == null)
                {
                    return UserManagementResult.Failure("Invalid request.");
                }

                var user = await _userRepository.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return UserManagementResult.Failure("User not found.");
                }

                // Check if new email conflicts with other users
                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    var currentEmail = user.Email ?? string.Empty;

                    if (!request.Email.Equals(currentEmail, StringComparison.OrdinalIgnoreCase))
                    {
                        var existingUserByEmail = await _userRepository.FindByEmailAsync(request.Email);
                        if (existingUserByEmail != null && existingUserByEmail.Id != user.Id)
                        {
                            return UserManagementResult.Failure("Email address already exists.");
                        }

                        // Update email
                        user.Email = request.Email;
                        user.UserName = request.Email; // Keep username in sync with email
                        user.NormalizedEmail = request.Email.ToUpperInvariant();
                        user.NormalizedUserName = request.Email.ToUpperInvariant();
                    }
                }

                // Update status
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

                // Soft delete: mark inactive instead of removing
                if (user.IsApproved)
                {
                    user.IsApproved = false;
                    var (ok, errs) = await _userRepository.UpdateUserAsync(user);
                    if (!ok) return UserManagementResult.Failure(errs);
                }
                return UserManagementResult.Success($"User '{user.FirstName} {user.LastName}' set to Inactive.");
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

        // Placeholder business actions for EDP enroll/assign. Replace with real domain logic when available.
        public async Task<UserManagementResult> EnrollStudentAsync(int userId, string edpCode)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null)
            {
                return UserManagementResult.Failure("User not found.");
            }
            // No-op example: mark approved if not yet
            if (!user.IsApproved)
            {
                user.IsApproved = true;
                var (ok, errors) = await _userRepository.UpdateUserAsync(user);
                if (!ok) return UserManagementResult.Failure(errors);
            }
            return UserManagementResult.Success("Student enrolled to EDP successfully.");
        }

        public async Task<IEnumerable<EnrolledClassDto>> GetEnrolledClassesAsync(int userId)
        {
            // TODO: Replace with actual implementation when enrollment data structure is available
            // For now, return mock data or query from your actual enrollment/student-class relationship table

            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null)
            {
                return Enumerable.Empty<EnrolledClassDto>();
            }

            // Placeholder: Return empty list for now
            // In a real implementation, you would query the StudentClass or Enrollment table
            // Example:
            // var enrollments = await _context.StudentClasses
            //     .Where(sc => sc.UserId == userId)
            //     .Select(sc => new EnrolledClassDto
            //     {
            //         EdpCode = sc.EdpCode,
            //         ClassName = sc.Class.Name
            //     })
            //     .ToListAsync();

            return Enumerable.Empty<EnrolledClassDto>();
        }

        public async Task<UserManagementResult> UnenrollStudentAsync(int userId, string edpCode)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null)
            {
                return UserManagementResult.Failure("User not found.");
            }

            // TODO: Replace with actual implementation when enrollment data structure is available
            // In a real implementation, you would delete the enrollment record
            // Example:
            // var enrollment = await _context.StudentClasses
            //     .FirstOrDefaultAsync(sc => sc.UserId == userId && sc.EdpCode == edpCode);
            // if (enrollment != null)
            // {
            //     _context.StudentClasses.Remove(enrollment);
            //     await _context.SaveChangesAsync();
            // }

            return UserManagementResult.Success("Student unenrolled from class successfully.");
        }

        public async Task<UserManagementResult> AssignTeacherAsync(int userId, string edpCode)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null)
            {
                return UserManagementResult.Failure("User not found.");
            }
            // Ensure role Teacher
            var roles = await _userRepository.GetRolesAsync(user);
            if (!roles.Contains("Teacher"))
            {
                await _userRepository.AddToRoleAsync(user, "Teacher");
            }
            return UserManagementResult.Success("Teacher assigned to EDP successfully.");
        }
    }
}
