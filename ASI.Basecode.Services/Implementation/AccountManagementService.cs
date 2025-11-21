using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.DTOs;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ASI.Basecode.Services.Implementation
{
    public class AccountManagementService : IAccountManagementService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IClassManagementRepository _classRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountManagementService> _logger;

        public AccountManagementService(
            IUserRepository userRepository,
            UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            IClassManagementRepository classRepository,
            IEmailService emailService,
            ILogger<AccountManagementService> logger)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _classRepository = classRepository;
            _emailService = emailService;
            _logger = logger;
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
                    IdNumber = user.IdNumber, // added mapping
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
                IdNumber = user.IdNumber,
                UserName = user.UserName!,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = primaryRole,
                IsActive = user.IsApproved,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<UserManagementDto?> GetUserByIdNumberAsync(int idNumber)
        {
            var user = await _userManager.Users
                .Where(u => !u.IsDeleted && u.IdNumber == idNumber)
                .FirstOrDefaultAsync();

            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault() ?? "Student";

            return new UserManagementDto
            {
                Id = user.Id,
                IdNumber = user.IdNumber,
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
                IdNumber = user.IdNumber, // added mapping
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

                // If user is a student, enforce "no same course" and "no same schedule" rules
                var roles = await _userRepository.GetRolesAsync(user);
                var isStudent = roles.Any(r => r.Equals("Student", System.StringComparison.OrdinalIgnoreCase));

                if (isStudent)
                {
                    var enrollments = await _classRepository.GetEnrollmentsByStudentIdAsync(userId);

                    // Prevent enrolling in the same course (if class has CourseId)
                    if (IsEnrolledInSameSubject(classEntity, enrollments))
                    {

                        return UserManagementResult.Failure("Student is already enrolled in another class for the same course.");

                    }

                    // Prevent enrolling in a class with the same schedule (exact match, case-insensitive)
                    var newSchedule = classEntity.Schedule?.Trim();
                    if (HasScheduleConflict(classEntity, enrollments))
                    {

                        return UserManagementResult.Failure("Student is already enrolled in a class with the same schedule.");

                    }
                }

                // Check if student is already enrolled in this exact class
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
                    ? $"{e.Class.Course.CourseName}"
                    : $"Class {e.ClassId}"
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
                    ? $"{c.Course.CourseName}"
                    : $"Class {c.Id}"
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
        private static bool IsEnrolledInSameSubject(Class targetClass, IEnumerable<Enrollment> enrollments)
        {
            var targetCourseId = targetClass.CourseId;
            var targetCourseCode = targetClass.Course?.CourseCode;

            return enrollments.Any(enrollment =>
            {
                var course = enrollment.Class?.Course;
                if (course == null)
                {
                    return false;
                }

                if (targetCourseId.HasValue && course.Id == targetCourseId.Value)
                {
                    return true;
                }

                if (!string.IsNullOrWhiteSpace(targetCourseCode) &&
                    !string.IsNullOrWhiteSpace(course.CourseCode) &&
                    string.Equals(course.CourseCode, targetCourseCode, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return false;
            });
        }

        private static bool HasScheduleConflict(Class targetClass, IEnumerable<Enrollment> enrollments)
        {
            var targetSchedule = ParseScheduleForComparison(targetClass?.Schedule);
            if (!IsScheduleUsable(targetSchedule))
            {
                return false;
            }

            foreach (var enrollment in enrollments)
            {
                var existingSchedule = ParseScheduleForComparison(enrollment.Class?.Schedule);
                if (!IsScheduleUsable(existingSchedule))
                {
                    continue;
                }

                if (SchedulesOverlap(targetSchedule, existingSchedule))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool SchedulesOverlap(ScheduleParts a, ScheduleParts b)
        {
            if (!a.Start.HasValue || !a.End.HasValue || !b.Start.HasValue || !b.End.HasValue)
            {
                return false;
            }

            var sharesDay = a.Days.Any(day => b.Days.Contains(day));
            if (!sharesDay)
            {
                return false;
            }

            return a.Start.Value < b.End.Value && b.Start.Value < a.End.Value;
        }

        private static bool IsScheduleUsable(ScheduleParts schedule)
        {
            return schedule.Days.Count > 0 && schedule.Start.HasValue && schedule.End.HasValue;
        }

        private static ScheduleParts ParseScheduleForComparison(string? schedule)
        {
            var result = new ScheduleParts();
            if (string.IsNullOrWhiteSpace(schedule))
            {
                return result;
            }

            var parts = schedule.Split(',', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                var dayPart = parts[0].Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();

                if (dayPart.Contains("TH", StringComparison.Ordinal))
                {
                    result.Days.Add("TH");
                    dayPart = dayPart.Replace("TH", string.Empty, StringComparison.Ordinal);
                }

                foreach (var character in dayPart)
                {
                    switch (character)
                    {
                        case 'M':
                            result.Days.Add("M");
                            break;
                        case 'T':
                            result.Days.Add("T");
                            break;
                        case 'W':
                            result.Days.Add("W");
                            break;
                        case 'F':
                            result.Days.Add("F");
                            break;
                        case 'S':
                            result.Days.Add("S");
                            break;
                    }
                }
            }

            if (parts.Length > 1)
            {
                var timeRange = parts[1];
                var separatorIndex = timeRange.IndexOf('–');
                if (separatorIndex < 0)
                {
                    separatorIndex = timeRange.IndexOf('-');
                }

                if (separatorIndex > 0)
                {
                    var start = timeRange[..separatorIndex].Trim();
                    var end = timeRange[(separatorIndex + 1)..].Trim();

                    if (DateTime.TryParse(start, CultureInfo.InvariantCulture, DateTimeStyles.None, out var startTime))
                    {
                        result.Start = startTime.TimeOfDay;
                    }

                    if (DateTime.TryParse(end, CultureInfo.InvariantCulture, DateTimeStyles.None, out var endTime))
                    {
                        result.End = endTime.TimeOfDay;
                    }
                }
            }

            return result;
        }

        private class ScheduleParts
        {
            public HashSet<string> Days { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            public TimeSpan? Start { get; set; }
            public TimeSpan? End { get; set; }
        }
    }
}
