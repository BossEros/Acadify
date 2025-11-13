using ASI.Basecode.Data.Data;
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
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public AccountManagementService(
            IUserRepository userRepository,
            UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            AppDbContext context)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
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

        public async Task<UserManagementResult> EnrollStudentAsync(int userId, string edpCode)
        {
            if (string.IsNullOrWhiteSpace(edpCode))
            {
                return UserManagementResult.Failure("EDP code is required.");
            }

            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null)
            {
                return UserManagementResult.Failure("User not found.");
            }

            var normalizedEdp = edpCode.Trim().ToUpperInvariant();

            var classEntity = await _context.Classes
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c =>
                    (c.JoinCode != null && c.JoinCode.ToUpper() == normalizedEdp) ||
                    (c.Course != null && c.Course.CourseCode.ToUpper() == normalizedEdp));

            if (classEntity == null)
            {
                var availableCodes = await _context.Classes
                    .Select(c => c.JoinCode ?? c.Course.CourseCode)
                    .Where(code => code != null)
                    .Distinct()
                    .ToListAsync();
                var hint = availableCodes.Any()
                    ? $" Available codes: {string.Join(", ", availableCodes)}."
                    : " No classes are currently available. Please create a class first.";
                return UserManagementResult.Failure("Class not found for the supplied EDP code." + hint);
            }

            var roles = await _userRepository.GetRolesAsync(user);
            if (!roles.Contains("Student"))
            {
                await _userRepository.AddToRoleAsync(user, "Student");
            }

            if (!user.IsApproved)
            {
                user.IsApproved = true;
                await _userRepository.UpdateUserAsync(user);
            }

            var alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.ClassId == classEntity.Id && e.StudentId == user.Id);
            if (alreadyEnrolled)
            {
                return UserManagementResult.Failure("Student is already enrolled in this class.");
            }

            _context.Enrollments.Add(new Enrollment
            {
                ClassId = classEntity.Id,
                StudentId = user.Id,
                EnrolledAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            var courseLabel = classEntity.Course?.CourseCode ?? classEntity.JoinCode ?? "class";
            return UserManagementResult.Success($"Student enrolled in {courseLabel} successfully.");
        }

        public async Task<UserManagementResult> AssignTeacherAsync(int userId, string edpCode)
        {
            if (string.IsNullOrWhiteSpace(edpCode))
            {
                return UserManagementResult.Failure("EDP code is required.");
            }

            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null)
            {
                return UserManagementResult.Failure("User not found.");
            }

            var normalizedEdp = edpCode.Trim().ToUpperInvariant();

            var classEntity = await _context.Classes
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c =>
                    (c.JoinCode != null && c.JoinCode.ToUpper() == normalizedEdp) ||
                    (c.Course != null && c.Course.CourseCode.ToUpper() == normalizedEdp));

            if (classEntity == null)
            {
                var availableCodes = await _context.Classes
                    .Select(c => c.JoinCode ?? c.Course.CourseCode)
                    .Where(code => code != null)
                    .Distinct()
                    .ToListAsync();
                var hint = availableCodes.Any()
                    ? $" Available codes: {string.Join(", ", availableCodes)}."
                    : " No classes are currently available. Please create a class first.";
                return UserManagementResult.Failure("Class not found for the supplied EDP code." + hint);
            }

            var roles = await _userRepository.GetRolesAsync(user);
            if (!roles.Contains("Teacher"))
            {
                await _userRepository.AddToRoleAsync(user, "Teacher");
            }

            if (!user.IsApproved)
            {
                user.IsApproved = true;
                await _userRepository.UpdateUserAsync(user);
            }

            classEntity.TeacherId = user.Id;
            classEntity.IsActive = true;
            await _context.SaveChangesAsync();

            var courseLabel = classEntity.Course?.CourseCode ?? classEntity.JoinCode ?? "class";
            return UserManagementResult.Success($"Teacher assigned to {courseLabel} successfully.");
        }
    }
}
