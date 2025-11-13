using Microsoft.AspNetCore.Mvc;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.WebApp.ViewModels;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly IAdminDashboardService _dashboardService;

        public AdminDashboardController(IAdminDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var totalUsers = await _dashboardService.GetTotalUsers();
            var studentCount = await _dashboardService.GetStudentCount();
            var teacherCount = await _dashboardService.GetTeacherCount();
            var adminCount = await _dashboardService.GetAdminCount();

            var totalClasses = await _dashboardService.GetTotalClasses();
            var activeClasses = await _dashboardService.GetActiveClassCount();
            var inactiveClasses = await _dashboardService.GetInactiveClassCount();

            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                StudentCount = studentCount,
                TeacherCount = teacherCount,
                AdminCount = adminCount,
                
                TotalClasses = totalClasses,
                ActiveClasses = activeClasses,
                InactiveClasses = inactiveClasses
            };

            return View(viewModel);
        }
    }
}