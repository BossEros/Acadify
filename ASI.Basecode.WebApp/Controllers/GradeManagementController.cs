using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.WebApp.ViewModels;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize]
    public class GradeManagementController : Controller
    {
        private readonly IGradeManagementService _gradeManagementService;
        private readonly UserManager<User> _userManager;

        public GradeManagementController(IGradeManagementService gradeManagementService, UserManager<User> userManager)
        {
            _gradeManagementService = gradeManagementService;
            _userManager = userManager;
        }

        public async Task<IActionResult> GradeView()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var semesterDtos = await _gradeManagementService.GetSemesterGradesForStudentAsync(user.Id);

            var semesterViewModels = semesterDtos.Select(s => new SemesterGradesViewModel
            {
                YearLevel = s.YearLevel,
                Semester = s.Semester,
                Grades = s.Grades.Select(g => new GradeViewModel
                {
                    CourseCode = g.CourseCode,
                    CourseName = g.CourseName,
                    Units = g.Units,
                    MidtermGrade = g.MidtermGrade,
                    FinalGrade = g.FinalGrade,
                    Remark = g.Remark,
                    IsPassed = g.IsPassed
                }).OrderBy(x => x.CourseCode).ToList()
            }).ToList();

            return View(semesterViewModels);
        }
    }
}