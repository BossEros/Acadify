using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Data;
using ASI.Basecode.WebApp.ViewModels;
using ASI.Basecode.Services.Interfaces;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class ClassReportController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IClassReportService _reportService;

        public ClassReportController(AppDbContext context, IClassReportService reportService)
        {
            _context = context;
            _reportService = reportService;
        }

        public async Task<IActionResult> Index(int id)
        {
            var classWithDetails = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments!)
                    .ThenInclude(e => e.Student)
                .Include(c => c.Enrollments!)
                    .ThenInclude(e => e.Grade)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classWithDetails == null)
            {
                return NotFound();
            }

            double passingRate = await _reportService.GetPassingRate(id);
            double failingRate = await _reportService.GetFailingRate(id);
            double classAverage = await _reportService.GetClassAverage(id);
            double incompleteRate = await _reportService.GetIncompleteRate(id);

            if (classWithDetails.Course == null)
            {
                 return NotFound();
            }

            var course = classWithDetails.Course;
            var enrollments = classWithDetails.Enrollments ?? new List<Data.Models.Enrollment>();

            var viewModel = new TeacherGradeViewModel
            {
                ClassId = classWithDetails.Id,
                CourseCode = course.CourseCode,
                CourseName = classWithDetails.Course.CourseName,
                Schedule = classWithDetails.Schedule ?? string.Empty,
                Units = (int)classWithDetails.Course.Units,
                
                TeacherName = classWithDetails.Teacher != null 
                    ? $"{classWithDetails.Teacher.FirstName} {classWithDetails.Teacher.LastName}" 
                    : "Unknown",

                StudentGrades = enrollments.Select(e => new StudentGradeViewModel
                {
                    EnrollmentId = e.Id,
                    StudentId = e.StudentId,
                    StudentName = $"{e.Student.FirstName} {e.Student.LastName}",
                    MidtermGrade = e.Grade?.MidtermGrade,
                    FinalGrade = e.Grade?.FinalGrade,
                    Remark = e.Grade?.Remarks ?? "Incomplete",
                    IsPassed = e.Grade?.Remarks == "Passed"
                }).ToList(),

                // Statistics
                PassingRate = passingRate,
                FailingRate = failingRate,
                ClassAverage = classAverage,
                IncompleteRate = incompleteRate
            };

            return View(viewModel); 
        }
    }
}