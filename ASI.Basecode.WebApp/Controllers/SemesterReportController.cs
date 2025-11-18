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

        public async Task<IActionResult> Index(int schoolYearStart = 2025, short semester = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var rawEnrollments = await _performanceService.GetStudentGradesForSemester(user.Id, schoolYearStart, semester);
            var gpa = await _performanceService.GetGpaForSemester(user.Id, schoolYearStart, semester);
            var totalUnits = await _performanceService.GetTotalEnrolledUnitsForSemester(user.Id, schoolYearStart, semester);

            int passedCount = 0;
            int failedCount = 0;
            int incompleteCount = 0;

            foreach (var enrollment in rawEnrollments)
            {
                var grade = enrollment.Grade;

                if (grade == null || grade.FinalGrade == null)
                {
                    incompleteCount++;
                }
                else
                {
                    var remark = (grade.Remarks ?? "").Trim();
                    
                    if (remark == "Passed") passedCount++;
                    else if (remark == "Failed") failedCount++;
                    else incompleteCount++; 
                }
            }

            var gradeViewModels = rawEnrollments.Select(e => new GradeViewModel
            {
                CourseCode = e.Class.Course?.CourseCode ?? "N/A",
                CourseName = e.Class.Course?.CourseName ?? "N/A",
                Units = e.Class.Course?.Units ?? 0,
                
                MidtermGrade = e.Grade?.MidtermGrade,
                FinalGrade = e.Grade?.FinalGrade,
                
                Remark = e.Grade?.Remarks ?? "Incomplete", 
                
                IsPassed = e.Grade?.Remarks == "Passed"
            }).OrderBy(vm => vm.CourseCode).ToList();

            var schoolYearRange = $"S.Y. {schoolYearStart}-{schoolYearStart + 1}";
            var semesterDescription = semester == 1 ? "1st Semester" : "2nd Semester";

            var viewModel = new SemesterReportViewModel
            {
                StudentName = $"{user.FirstName} {user.LastName}",

                StudentIdNumber = user.IdNumber.ToString(), 
                
                SchoolYearStart = schoolYearStart, 
                Semester = semester,

                SchoolYearRange = schoolYearRange,         
                SemesterDescription = semesterDescription, 
                
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