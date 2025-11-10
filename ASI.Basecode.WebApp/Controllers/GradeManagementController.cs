using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Data;
using ASI.Basecode.WebApp.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize]
    public class GradeManagementController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public GradeManagementController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> GradeView()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var enrollments = await _context.Enrollments
                .Include(e => e.Class)
                    .ThenInclude(c => c.Course)
                .Include(e => e.Grade)
                .Where(e => e.StudentId == user.Id)
                .ToListAsync();

            var semesterGrades = enrollments
                .GroupBy(e => new { e.Class.YearLevel, e.Class.Semester })
                .OrderBy(g => g.Key.YearLevel)
                .ThenBy(g => g.Key.Semester)
                .Select(g => new SemesterGradesViewModel
                {
                    YearLevel = g.Key.YearLevel,
                    Semester = g.Key.Semester,
                    Grades = g.Select(e =>
                    {
                        var isPassed = e.Grade?.FinalGrade.HasValue == true || e.Grade?.MidtermGrade.HasValue == true;
                        // Basic rule: if student has any grade recorded treat as "Passed" in this app's simplified logic.
                        // You can replace this with your real passing rules (e.g. threshold checks) later.
                        var remarkText = isPassed ? "Passed" : "Incomplete";

                        return new GradeViewModel
                        {
                            CourseCode = e.Class.Course.CourseCode,
                            CourseName = e.Class.Course.CourseName,
                            Units = (int)e.Class.Course.Units,
                            MidtermGrade = e.Grade?.MidtermGrade,
                            FinalGrade = e.Grade?.FinalGrade,
                            Remark = remarkText,
                            IsPassed = isPassed
                        };
                    })
                    .OrderBy(x => x.CourseCode)
                    .ToList()
                })
                .ToList();

            return View(semesterGrades);
        }
    }
}
