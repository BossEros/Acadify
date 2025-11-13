using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Data;
using ASI.Basecode.WebApp.ViewModels;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherGradeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public TeacherGradeController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: TeacherGrade/ClassGrades/{id}
        public async Task<IActionResult> ClassGrades(int id)
        {
            if (id <= 0) return BadRequest("Invalid class id.");

            var classWithDetails = await _context.Classes
                .Include(c => c.Course)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Grade)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (classWithDetails == null)
                return NotFound();

            var viewModel = new TeacherGradeViewModel
            {
                ClassId = classWithDetails.Id,
                CourseCode = classWithDetails.Course.CourseCode,
                CourseName = classWithDetails.Course.CourseName,
                Schedule = classWithDetails.Schedule,
                Units = (int)classWithDetails.Course.Units,
                StudentGrades = classWithDetails.Enrollments.Select(e =>
                {
                    var final = e.Grade?.FinalGrade;
                    string remark;
                    bool isPassed;

                    if (!final.HasValue)
                    {
                        remark = "Incomplete";
                        isPassed = false;
                    }
                    else if (final.Value > 3.0m)
                    {
                        remark = "Failed";
                        isPassed = false;
                    }
                    else
                    {
                        remark = "Passed";
                        isPassed = true;
                    }

                    return new StudentGradeViewModel
                    {
                        EnrollmentId = e.Id,
                        StudentId = e.StudentId,
                        StudentName = $"{e.Student.FirstName} {e.Student.LastName}",
                        MidtermGrade = e.Grade?.MidtermGrade,
                        FinalGrade = final,
                        IsPassed = isPassed,
                        Remark = remark
                    };
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: TeacherGrade/UpdateGrade
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGrade([FromBody] JsonElement payload)
        {
            if (!payload.TryGetProperty("enrollmentId", out var enrollmentProp) || enrollmentProp.ValueKind != JsonValueKind.Number)
                return BadRequest("enrollmentId is required and must be a number.");

            var enrollmentId = enrollmentProp.GetInt32();

            var enrollmentExists = await _context.Enrollments.AnyAsync(e => e.Id == enrollmentId);
            if (!enrollmentExists)
                return NotFound("Enrollment not found.");

            var grade = await _context.Grades.FirstOrDefaultAsync(g => g.EnrollmentId == enrollmentId);
            if (grade == null)
            {
                grade = new Grade
                {
                    EnrollmentId = enrollmentId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Grades.Add(grade);
            }

            if (payload.TryGetProperty("midtermGrade", out var midElem))
            {
                if (midElem.ValueKind == JsonValueKind.Null)
                    grade.MidtermGrade = null;
                else if (midElem.ValueKind == JsonValueKind.Number)
                    grade.MidtermGrade = midElem.GetDecimal();
                else if (midElem.ValueKind == JsonValueKind.String && decimal.TryParse(midElem.GetString(), out var midVal))
                    grade.MidtermGrade = midVal;
            }

            if (payload.TryGetProperty("finalGrade", out var finElem))
            {
                if (finElem.ValueKind == JsonValueKind.Null)
                    grade.FinalGrade = null;
                else if (finElem.ValueKind == JsonValueKind.Number)
                    grade.FinalGrade = finElem.GetDecimal();
                else if (finElem.ValueKind == JsonValueKind.String && decimal.TryParse(finElem.GetString(), out var finVal))
                    grade.FinalGrade = finVal;
            }

            // Compute remark using requested rule:
            grade.Remarks = ComputeRemark(grade.FinalGrade);
            grade.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                remark = grade.Remarks,
                midterm = grade.MidtermGrade,
                final = grade.FinalGrade
            });
        }

        private static string ComputeRemark(decimal? final)
        {
            if (!final.HasValue) return "Incomplete";
            if (final.Value > 3.0m) return "Failed";
            return "Passed";
        }
    }
}