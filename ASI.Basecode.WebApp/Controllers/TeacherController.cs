using Microsoft.AspNetCore.Mvc;

namespace Student_Performance_Tracker.Controllers
{
    public class TeacherController : Controller
    {
        
            public IActionResult Dashboard()
            {
                return View();
            }

            public IActionResult Settings()
            {
                return View();
            }

            public IActionResult MyClasses()
            {
                return View();
            }

            public IActionResult Schedule()
            {
                return View();
            }

            public IActionResult Students()
            {
                return View();
            }

            public IActionResult Grades()
            {
                return View();
            }
        }
    }

