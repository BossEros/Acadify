namespace ASI.Basecode.WebApp.Controllers;

using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.WebApp.ViewModels.ClassManagement;
using Microsoft.AspNetCore.Mvc;

public class TeacherClassController : Controller
{
    private readonly IClassManagementService _classManagementService;
    private readonly IAccountManagementService _userManagementService;

    public TeacherClassController(IClassManagementService classManagementService, IAccountManagementService userManagementService)
    {
        _classManagementService = classManagementService;
        _userManagementService = userManagementService;
    }

    [HttpGet]
    public async Task<IActionResult> ViewClass(int id)
    {
        var classEntity = await _classManagementService.GetClassByIdAsync(id);

        if (classEntity == null)
            return NotFound();

        var viewModel = new TeacherClassViewModel
        {
            CourseCode = classEntity.Course?.CourseCode ?? "N/A",
            CourseName = classEntity.Course?.CourseName ?? "N/A",
            Schedule = classEntity.Schedule ?? "N/A",
            Units = classEntity.Course?.Units ?? 0,

            Students = classEntity.Enrollments?.Select(e => new TeacherClassViewModel.StudentGradeItem
            {
                StudentName = $"{e.Student?.FirstName} {e.Student?.LastName}",
                MidtermGrade = (double)(e.Grade?.MidtermGrade ?? 0),
                FinalGrade = (double)(e.Grade?.FinalGrade ?? 0),
                Remarks = e.Grade?.Remarks ?? "N/A"
            }).ToList() ?? new List<TeacherClassViewModel.StudentGradeItem>()
        };

        return View("~/Views/ClassManagement/ViewClass.cshtml", viewModel);
    }

    public async Task<IActionResult> TeacherDashboard()
    {
        // Get the currently logged-in user's ID from claims
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized(); // not logged in
        }

        int teacherId = int.Parse(userIdClaim);

        var allUsers = await _userManagementService.GetAllUsersAsync();
        var teacher = allUsers.FirstOrDefault(u => u.Id == teacherId);

        string teacherName = teacher != null
            ? $"{teacher.FirstName} {teacher.LastName}"
            : "Unknown Teacher";

        // Get classes taught by this teacher
        var allClasses = await _classManagementService.GetAllClassesAsync();
        var teacherClasses = allClasses
            .Where(c => c.TeacherId == teacherId)
            .Select(c => new TeacherDashboardViewModel.ClassCard
            {
                Id = c.Id,
                CourseCode = c.Course?.CourseCode ?? "N/A",
                CourseName = c.Course?.CourseName ?? "N/A",
                Schedule = c.Schedule ?? "N/A",
                Semester = c.Semester,
                YearLevel = c.YearLevel,
                IsActive = c.IsActive
            })
            .ToList();

        var viewModel = new TeacherDashboardViewModel
        {
            TeacherName = teacherName,
            Classes = teacherClasses
        };

        return View("~/Views/ClassManagement/TeacherDashboard.cshtml", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActivateClass(ActivateClassViewModel viewModel)
    {
        // Get the currently logged-in user's ID from claims
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Json(new { success = false, message = "User not authenticated." });
        }

        int teacherId = int.Parse(userIdClaim);

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, errors = errors });
        }

        // Parse EDP code as Class ID
        if (!int.TryParse(viewModel.EdpCode, out int classId))
        {
            return Json(new { success = false, message = "Invalid EDP code format. Please enter a valid class ID." });
        }

        try
        {
            // Check if class already exists and is assigned to this teacher
            var existingClass = await _classManagementService.GetClassByIdIncludeInactiveAsync(classId);
            if (existingClass != null && existingClass.IsActive && existingClass.TeacherId == teacherId)
            {
                return Json(new { success = false, message = "You are already assigned to this active class." });
            }

            // Activate the class (this will also assign the teacher)
            await _classManagementService.ActivateClassAsync(classId, teacherId);

            return Json(new { success = true, message = "Class activated successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}