using Microsoft.AspNetCore.Mvc;
using ASI.Basecode.Services.Interfaces;
using System.Linq;
using System.Security.Claims;

namespace Student_Performance_Tracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly IClassManagementService _classService;

        public HomeController(IClassManagementService classService)
        {
            _classService = classService;
        }

        public async Task<IActionResult> StudentDashboard()
        {
            int studentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var allClasses = await _classService.GetAllClassesAsync();

            var enrolled = allClasses
                .Where(c => c.Enrollments.Any(e => e.StudentId == studentId))
                .ToList();

            return View("StudentDashboard", enrolled);
        }
    }
}
