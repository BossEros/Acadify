using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.DTOs;
using ASI.Basecode.WebApp.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ASI.Basecode.WebApp.Controllers
{
    
    [Authorize(Roles = "Admin")]
    public class AccountManagementController : Controller
    {
        private readonly IAccountManagementService _accountManagementService;

        public AccountManagementController(IAccountManagementService accountManagementService)
        {
            _accountManagementService = accountManagementService;
        }

        private JsonResult JsonResponse(bool success, string message, object? data = null)
        {
            return Json(new { success, message, data });
        }

        // Main account management page
        public IActionResult Index()
        {
            return View();
        }

        // Student Management View
        public async Task<IActionResult> Students()
        {
            try
            {
                var users = await _accountManagementService.GetAllUsersAsync();
                var students = users.Where(u => u.Role?.ToLower() == "student");

                var viewModel = new UserListViewModel
                {
                    Users = students,
                    Message = TempData["Message"]?.ToString(),
                    MessageType = TempData["MessageType"]?.ToString()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                var viewModel = new UserListViewModel
                {
                    Users = new List<UserManagementDto>(),
                    Message = "Error loading students: " + ex.Message,
                    MessageType = "error"
                };
                return View(viewModel);
            }
        }

        // Teacher Management View
        public async Task<IActionResult> Teachers()
        {
            try
            {
                var users = await _accountManagementService.GetAllUsersAsync();
                var teachers = users.Where(u => u.Role?.ToLower() == "teacher");

                var viewModel = new UserListViewModel
                {
                    Users = teachers,
                    Message = TempData["Message"]?.ToString(),
                    MessageType = TempData["MessageType"]?.ToString()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                var viewModel = new UserListViewModel
                {
                    Users = new List<UserManagementDto>(),
                    Message = "Error loading teachers: " + ex.Message,
                    MessageType = "error"
                };
                return View(viewModel);
            }
        }

        // ==== AJAX ENDPOINTS ====

        [HttpGet]
        public async Task<IActionResult> FindUser(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return JsonResponse(false, "Please enter a user ID or email to search.");
                }

                // Try to parse as ID first
                if (int.TryParse(searchTerm, out int userId))
                {
                    var user = await _accountManagementService.GetUserByIdAsync(userId);
                    if (user != null)
                    {
                        return JsonResponse(true, "User found", new
                        {
                            id = user.Id,
                            firstName = user.FirstName,
                            lastName = user.LastName,
                            email = user.Email,
                            role = user.Role,
                            isActive = user.IsActive
                        });
                    }
                }

                // Try as email
                var userByEmail = await _accountManagementService.GetUserByEmailAsync(searchTerm);
                if (userByEmail != null)
                {
                    return JsonResponse(true, "User found", new
                    {
                        id = userByEmail.Id,
                        firstName = userByEmail.FirstName,
                        lastName = userByEmail.LastName,
                        email = userByEmail.Email,
                        role = userByEmail.Role,
                        isActive = userByEmail.IsActive
                    });
                }

                return JsonResponse(false, "User not found.");
            }
            catch (Exception ex)
            {
                return JsonResponse(false, "Error searching user: " + ex.Message);
            }
        }

        public class SimpleUserCourseRequest
        {
            public int UserId { get; set; }
            public string EdpCode { get; set; } = string.Empty;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollStudent([FromBody] SimpleUserCourseRequest request)
        {
            try
            {
                var result = await _accountManagementService.EnrollStudentAsync(request.UserId, request.EdpCode);
                return JsonResponse(result.Succeeded, result.Message ?? (result.Succeeded ? "Student enrolled successfully." : "Failed to enroll student."));
            }
            catch (Exception ex)
            {
                return JsonResponse(false, "Error enrolling student: " + ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEnrolledClasses(int userId)
        {
            try
            {
                var classes = await _accountManagementService.GetEnrolledClassesAsync(userId);
                return JsonResponse(true, "Enrolled classes retrieved successfully", classes ?? new List<EnrolledClassDto>());
            }
            catch (Exception ex)
            {
                return JsonResponse(false, "Error retrieving enrolled classes: " + ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnenrollStudent([FromBody] SimpleUserCourseRequest request)
        {
            try
            {
                var result = await _accountManagementService.UnenrollStudentAsync(request.UserId, request.EdpCode);
                return JsonResponse(result.Succeeded, result.Message ?? (result.Succeeded ? "Student unenrolled successfully." : "Failed to unenroll student."));
            }
            catch (Exception ex)
            {
                return JsonResponse(false, "Error unenrolling student: " + ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTeacher([FromBody] SimpleUserCourseRequest request)
        {
            try
            {
                var result = await _accountManagementService.AssignTeacherAsync(request.UserId, request.EdpCode);
                return JsonResponse(result.Succeeded, result.Message ?? (result.Succeeded ? "Teacher assigned successfully." : "Failed to assign teacher."));
            }
            catch (Exception ex)
            {
                return JsonResponse(false, "Error assigning teacher: " + ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAssignedClasses(int userId)
        {
            try
            {
                var classes = await _accountManagementService.GetAssignedClassesAsync(userId);
                return JsonResponse(true, "Assigned classes retrieved successfully", classes ?? new List<AssignedClassDto>());
            }
            catch (Exception ex)
            {
                return JsonResponse(false, "Error retrieving assigned classes: " + ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnassignTeacher([FromBody] SimpleUserCourseRequest request)
        {
            try
            {
                var result = await _accountManagementService.UnassignTeacherAsync(request.UserId, request.EdpCode);
                return JsonResponse(result.Succeeded, result.Message ?? (result.Succeeded ? "Teacher unassigned successfully." : "Failed to unassign teacher."));
            }
            catch (Exception ex)
            {
                return JsonResponse(false, "Error unassigning teacher: " + ex.Message);
            }
        }

        public class EditUserRequest
        {
            public int UserId { get; set; }
            public string Email { get; set; } = string.Empty;
            public bool IsActive { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser([FromBody] EditUserRequest request)
        {
            try
            {
                if (request == null || request.UserId <= 0)
                {
                    return JsonResponse(false, "Invalid request data.");
                }

                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return JsonResponse(false, "Email address is required.");
                }

                var user = await _accountManagementService.GetUserByIdAsync(request.UserId);
                if (user == null)
                {
                    return JsonResponse(false, "User not found.");
                }

                // Check if email is being changed and if new email already exists
                if (!request.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var existingUser = await _accountManagementService.GetUserByEmailAsync(request.Email);
                    if (existingUser != null && existingUser.Id != request.UserId)
                    {
                        return JsonResponse(false, "Email address already exists.");
                    }
                }

                var updateResult = await _accountManagementService.UpdateUserEmailAndStatusAsync(new UpdateUserEmailAndStatusRequest
                {
                    UserId = request.UserId,
                    Email = request.Email,
                    IsActive = request.IsActive
                });

                if (!updateResult.Succeeded)
                {
                    return JsonResponse(false, updateResult.Message ?? "Failed to update user.");
                }

                return JsonResponse(true, "User updated successfully.");
            }
            catch (Exception ex)
            {
                return JsonResponse(false, $"Error updating user: {ex.Message}");
            }
        }

        public class DeleteUserAjaxRequest { public int UserId { get; set; } }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser([FromBody] DeleteUserAjaxRequest request)
        {
            try
            {
                var result = await _accountManagementService.DeleteUserAsync(request.UserId);
                return JsonResponse(result.Succeeded, result.Message ?? (result.Succeeded ? "User deleted successfully." : "Failed to delete user."));
            }
            catch (Exception ex)
            {
                return JsonResponse(false, "Error deleting user: " + ex.Message);
            }
        }
    }
}