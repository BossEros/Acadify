using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace Student_Performance_Tracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly IClassManagementService _classService;
        private readonly IAccountManagementService _accountManagementService;

        public HomeController(IClassManagementService classService, IAccountManagementService accountManagementService)
        {
            _classService = classService;
            _accountManagementService = accountManagementService;
        }

        public async Task<IActionResult> StudentDashboard()
        {
            // Get the currently logged-in user's ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(); // not logged in
            }

            int studentId = int.Parse(userIdClaim);

            // Get user info for name
            var allUsers = await _accountManagementService.GetAllUsersAsync();
            var student = allUsers.FirstOrDefault(u => u.Id == studentId);

            string studentName = student != null
                ? $"{student.FirstName} {student.LastName}"
                : "Unknown Student";

            // Get enrolled classes
            var allClasses = await _classService.GetAllClassesAsync();
            var enrolledClasses = allClasses
                .Where(c => c.Status && c.Enrollments != null && c.Enrollments.Any(e => e.StudentId == studentId))
                .Select(c => new StudentDashboardViewModel.ClassCard
                {
                    Id = c.EDPCode,
                    CourseCode = c.CourseCode ?? "N/A",
                    CourseName = c.Description ?? "N/A",
                    Units = c.Units,
                    TeacherName = c.TeacherName != null ? $"Prof. {c.TeacherName}" : "N/A",
                    Schedule = c.Schedule ?? "N/A"
                })
                .ToList();

            var viewModel = new StudentDashboardViewModel
            {
                StudentName = studentName,
                Classes = enrolledClasses
            };

            return View("StudentDashboard", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollClass(StudentDashboardViewModel viewModel)
        {
            // Get the currently logged-in user's ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Json(new { success = false, message = "User not authenticated." });
            }

            int studentId = int.Parse(userIdClaim);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, errors = errors });
            }

            try
            {
                // Enroll the student using the existing service method
                var result = await _accountManagementService.EnrollStudentAsync(studentId, viewModel.EdpCode);

                if (result.Succeeded)
                {
                    return Json(new { success = true, message = result.Message ?? "Enrolled successfully." });
                }
                else
                {
                    return Json(new { success = false, message = result.Message ?? "Failed to enroll." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
