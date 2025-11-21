using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.WebApp.ViewModels;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherGradeController : Controller
    {
        private readonly IClassManagementRepository _repo;
        private readonly ITeacherGradeService _teacherGradeService;
        private readonly UserManager<User> _userManager;

        public TeacherGradeController(IClassManagementRepository repo, ITeacherGradeService teacherGradeService, UserManager<User> userManager)
        {
            _repo = repo;
            _teacherGradeService = teacherGradeService;
            _userManager = userManager;
        }

        // GET: TeacherGrade/ClassGrades/{id}
        public async Task<IActionResult> ClassGrades(int id)
        {
            if (id <= 0) return BadRequest("Invalid class id.");

            var classWithDetails = await _repo.GetByIdIncludeInactiveAsync(id);

            if (classWithDetails == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Only the teacher assigned to this class can view it
            if (classWithDetails.TeacherId != user.Id)
                return Forbid();

            var viewModel = new TeacherGradeViewModel
            {
                ClassId = classWithDetails.Id,
                CourseCode = classWithDetails.Course!.CourseCode,
                CourseName = classWithDetails.Course.CourseName,
                Schedule = classWithDetails.Schedule ?? string.Empty,
                Units = (int)classWithDetails.Course.Units,
                StudentGrades = classWithDetails.Enrollments!.Select(e =>
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
                        StudentId = e.Student.IdNumber,
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

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var enrollment = await _repo.GetEnrollmentByIdAsync(enrollmentId);
            if (enrollment == null) return NotFound("Enrollment not found.");

            // Only the teacher assigned to the class can update grades
            if (enrollment.Class?.TeacherId != user.Id)
                return Forbid();

            // Detect presence of properties so we don't overwrite unchanged fields.
            var midtermProvided = payload.TryGetProperty("midtermGrade", out var midElem);
            decimal? midterm = null;
            if (midtermProvided)
            {
                if (midElem.ValueKind == JsonValueKind.Null)
                    midterm = null;
                else if (midElem.ValueKind == JsonValueKind.Number)
                    midterm = midElem.GetDecimal();
                else if (midElem.ValueKind == JsonValueKind.String && decimal.TryParse(midElem.GetString(), out var mv))
                    midterm = mv;
            }

            var finalProvided = payload.TryGetProperty("finalGrade", out var finElem);
            decimal? final = null;
            if (finalProvided)
            {
                if (finElem.ValueKind == JsonValueKind.Null)
                    final = null;
                else if (finElem.ValueKind == JsonValueKind.Number)
                    final = finElem.GetDecimal();
                else if (finElem.ValueKind == JsonValueKind.String && decimal.TryParse(finElem.GetString(), out var fv))
                    final = fv;
            }

            // Server-side range validation
            if (midtermProvided && midterm.HasValue && (midterm < 1.00m || midterm > 5.00m))
                return BadRequest("Midterm grade must be between 1.00 and 5.00.");

            if (finalProvided && final.HasValue && (final < 1.00m || final > 5.00m))
                return BadRequest("Final grade must be between 1.00 and 5.00.");

            var result = await _teacherGradeService.UpdateGradeAsync(enrollmentId, midterm, final, midtermProvided, finalProvided);

            if (!result.Success)
            {
                return NotFound(result.ErrorMessage ?? "Failed to update grade.");
            }

            return Ok(new
            {
                success = true,
                remark = result.Remark,
                midterm = result.Midterm,
                final = result.Final
            });
        }
    }
}