using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.WebApp.ViewModels;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(Roles = "Student")]
    public class SemesterReportController : Controller
    {
        private readonly IStudentPerformanceService _performanceService;
        private readonly UserManager<User> _userManager;

        public SemesterReportController(IStudentPerformanceService performanceService, 
                                        UserManager<User> userManager)
        {
            _performanceService = performanceService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(short semester = 1, short yearLevel = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // 1. Fetch Data
            var rawGrades = await _performanceService.GetStudentGradesForSemester(user.Id, semester, yearLevel);
            var gpa = await _performanceService.GetGpaForSemester(user.Id, semester, yearLevel);
            var totalUnits = await _performanceService.GetTotalEnrolledUnitsForSemester(user.Id, semester, yearLevel);

            // 2. Calculate Counts locally (More efficient than calling DB 3 times)
            int passedCount = 0;
            int failedCount = 0;
            int incompleteCount = 0;

            foreach (var grade in rawGrades)
            {
                // Normalize string to handle potential spaces or case sensitivity
                var remark = (grade.Remarks ?? "").Trim(); 
                
                if (remark == "Passed") passedCount++;
                else if (remark == "Failed") failedCount++;
                else incompleteCount++; // Handles "Incomplete", null, or empty
            }

            // 3. Map to GradeViewModel
            var gradeViewModels = rawGrades.Select(g => new GradeViewModel
            {
                CourseCode = g.Enrollment.Class.Course.CourseCode,
                CourseName = g.Enrollment.Class.Course.CourseName,
                Units = g.Enrollment.Class.Course.Units,
                MidtermGrade = g.MidtermGrade,
                FinalGrade = g.FinalGrade,
                Remark = g.Remarks ?? "Incomplete",
                IsPassed = g.Remarks == "Passed"
            }).ToList();

            // 4. Create Report ViewModel
            var viewModel = new SemesterReportViewModel
            {
                StudentName = $"{user.FirstName} {user.LastName}",
                StudentIdNumber = user.Id.ToString(),
                Semester = semester,
                YearLevel = yearLevel,
                
                GPA = gpa,
                TotalUnits = totalUnits,
                SubjectsPassed = passedCount,
                SubjectsFailed = failedCount,
                SubjectsIncomplete = incompleteCount,
                
                Grades = gradeViewModels
            };

            return View(viewModel);
        }
    }
}