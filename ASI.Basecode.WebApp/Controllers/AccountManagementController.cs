using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.DTOs;
using ASI.Basecode.WebApp.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ASI.Basecode.WebApp.Controllers
{
    [AllowAnonymous]
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

        // READ: View all users from database
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _accountManagementService.GetAllUsersAsync();
                
                var viewModel = new UserListViewModel
                {
                    Users = users,
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
                    Message = "Error loading users: " + ex.Message,
                    MessageType = "error"
                };
                return View(viewModel);
            }
        }

        // Remove old MVC view-based CRUD actions; new UI uses AJAX endpoints below

        // ==== AJAX ENDPOINTS FOR NEW UI ====

        [HttpGet]
        public async Task<IActionResult> FindUser(int userId)
        {
            try
            {
                var user = await _accountManagementService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return JsonResponse(false, "User not found.");
                }

                return JsonResponse(true, "User found", new
                {
                    id = user.Id,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    email = user.Email,
                    role = user.Role
                });
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
                // This assumes service understands creating a student-class relation by EDP code.
                var result = await _accountManagementService.EnrollStudentAsync(request.UserId, request.EdpCode);
                return JsonResponse(result.Succeeded, result.Message ?? (result.Succeeded ? "Student enrolled." : "Failed to enroll student."));
            }
            catch (Exception ex)
            {
                return JsonResponse(false, "Error enrolling student: " + ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTeacher([FromBody] SimpleUserCourseRequest request)
        {
            try
            {
                var result = await _accountManagementService.AssignTeacherAsync(request.UserId, request.EdpCode);
                return JsonResponse(result.Succeeded, result.Message ?? (result.Succeeded ? "Teacher assigned." : "Failed to assign teacher."));
            }
            catch (Exception ex)
            {
                return JsonResponse(false, "Error assigning teacher: " + ex.Message);
            }
        }

        public class EditPasswordRequest
        {
            public int UserId { get; set; }
            public string NewPassword { get; set; } = string.Empty;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser([FromBody] EditPasswordRequest request)
        {
            try
            {
                var result = await _accountManagementService.ChangePasswordAsync(new ChangePasswordRequest
                {
                    UserId = request.UserId,
                    NewPassword = request.NewPassword
                });
                return JsonResponse(result.Succeeded, result.Message ?? (result.Succeeded ? "Password updated." : "Failed to update password."));
            }
            catch (Exception ex)
            {
                return JsonResponse(false, "Error updating user: " + ex.Message);
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
                return JsonResponse(result.Succeeded, result.Message ?? (result.Succeeded ? "User deleted." : "Failed to delete user."));
            }
            catch (Exception ex)
            {
                return JsonResponse(false, "Error deleting user: " + ex.Message);
            }
        }
    }
}