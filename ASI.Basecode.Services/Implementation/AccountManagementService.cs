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
        private readonly IClassManagementRepository _classRepository;

        public AccountManagementService(
            IUserRepository userRepository,
            UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            IClassManagementRepository classRepository)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _classRepository = classRepository;
        }

        public async Task<IEnumerable<UserManagementDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users
                .Where(u => u.IsDeleted == false)
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
            if (user == null || user.IsDeleted) return null;

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
                if (!user.IsDeleted)
                {
                    user.IsDeleted = true;
                    var (ok, errs) = await _userRepository.UpdateUserAsync(user);
                    if (!ok) return UserManagementResult.Failure(errs);
                }
                return UserManagementResult.Success($"User '{user.FirstName} {user.LastName}' is Deleted.");
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

        // Enroll student in a class
        public async Task<UserManagementResult> EnrollStudentAsync(int userId, string edpCode)
        {
            try
            {
                var user = await _userRepository.FindByIdAsync(userId);
                if (user == null)
                {
                    return UserManagementResult.Failure("User not found.");
                }

                if (string.IsNullOrWhiteSpace(edpCode))
                {
                    return UserManagementResult.Failure("EDP code is required.");
                }

                // Parse EDP code as Class ID
                if (!int.TryParse(edpCode, out int classId))
                {
                    return UserManagementResult.Failure("Invalid EDP code format. Please enter a valid class ID.");
                }

                // Find the class by ID
                var classEntity = await _classRepository.GetByIdAsync(classId);
                if (classEntity == null)
                {
                    return UserManagementResult.Failure("Class not found with the provided EDP code.");
                }

                // Check if student is already enrolled
                var isEnrolled = await _classRepository.IsStudentEnrolledAsync(userId, classEntity.Id);
                if (isEnrolled)
                {
                    return UserManagementResult.Failure("Student is already enrolled in this class.");
                }

                // Enroll the student
                await _classRepository.EnrollStudentAsync(userId, classEntity.Id);

                return UserManagementResult.Success("Student enrolled successfully.");
            }
            catch (Exception ex)
            {
                return UserManagementResult.Failure($"Error enrolling student: {ex.Message}");
            }
        }

        public async Task<IEnumerable<EnrolledClassDto>> GetEnrolledClassesAsync(int userId)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null)
            {
                return Enumerable.Empty<EnrolledClassDto>();
            }

            var enrollments = await _classRepository.GetEnrollmentsByStudentIdAsync(userId);

            var enrolledClasses = enrollments.Select(e => new EnrolledClassDto
            {
                EdpCode = e.Class.Id.ToString(),
                ClassName = e.Class.Course != null
                    ? $"{e.Class.Course.CourseName} (Sem {e.Class.Semester}, Year {e.Class.YearLevel})"
                    : $"Class {e.ClassId} (Sem {e.Class.Semester}, Year {e.Class.YearLevel})"
            }).ToList();

            return enrolledClasses;
        }

        public async Task<UserManagementResult> UnenrollStudentAsync(int userId, string edpCode)
        {
            try
            {
                var user = await _userRepository.FindByIdAsync(userId);
                if (user == null)
                {
                    return UserManagementResult.Failure("User not found.");
                }

                if (string.IsNullOrWhiteSpace(edpCode))
                {
                    return UserManagementResult.Failure("EDP code is required.");
                }

                // Parse EDP code as Class ID
                if (!int.TryParse(edpCode, out int classId))
                {
                    return UserManagementResult.Failure("Invalid EDP code format. Please enter a valid class ID.");
                }

                // Find the class by ID
                var classEntity = await _classRepository.GetByIdAsync(classId);
                if (classEntity == null)
                {
                    return UserManagementResult.Failure("Class not found with the provided EDP code.");
                }

                // Check if student is enrolled
                var isEnrolled = await _classRepository.IsStudentEnrolledAsync(userId, classEntity.Id);
                if (!isEnrolled)
                {
                    return UserManagementResult.Failure("Student is not enrolled in this class.");
                }

                // Unenroll the student (Grade will be cascade deleted if exists)
                await _classRepository.UnenrollStudentAsync(userId, classEntity.Id);

                return UserManagementResult.Success("Student unenrolled from class successfully.");
            }
            catch (Exception ex)
            {
                return UserManagementResult.Failure($"Error unenrolling student: {ex.Message}");
            }
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

        public async Task<IEnumerable<AssignedClassDto>> GetAssignedClassesAsync(int userId)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null)
            {
                return Enumerable.Empty<AssignedClassDto>();
            }

            var classes = await _classRepository.GetClassesByTeacherIdAsync(userId);

            var assignedClasses = classes.Select(c => new AssignedClassDto
            {
                EdpCode = c.Id.ToString(),
                ClassName = c.Course != null
                    ? $"{c.Course.CourseName} (Sem {c.Semester}, Year {c.YearLevel})"
                    : $"Class {c.Id} (Sem {c.Semester}, Year {c.YearLevel})"
            }).ToList();

            return assignedClasses;
        }

        public async Task<UserManagementResult> UnassignTeacherAsync(int userId, string edpCode)
        {
            try
            {
                var user = await _userRepository.FindByIdAsync(userId);
                if (user == null)
                {
                    return UserManagementResult.Failure("User not found.");
                }

                if (string.IsNullOrWhiteSpace(edpCode))
                {
                    return UserManagementResult.Failure("EDP code is required.");
                }

                // Parse EDP code as Class ID
                if (!int.TryParse(edpCode, out int classId))
                {
                    return UserManagementResult.Failure("Invalid EDP code format. Please enter a valid class ID.");
                }

                // Find the class by ID
                var classEntity = await _classRepository.GetByIdAsync(classId);
                if (classEntity == null)
                {
                    return UserManagementResult.Failure("Class not found with the provided EDP code.");
                }

                // Verify that the user is the teacher of this class
                if (classEntity.TeacherId != userId)
                {
                    return UserManagementResult.Failure("This teacher is not assigned to the specified class.");
                }

                // Unassign the teacher by deleting the class
                await _classRepository.UnassignTeacherFromClassAsync(classId);

                return UserManagementResult.Success("Teacher unassigned from class successfully.");
            }
            catch (Exception ex)
            {
                return UserManagementResult.Failure($"Error unassigning teacher: {ex.Message}");
            }
        }
    }
}
