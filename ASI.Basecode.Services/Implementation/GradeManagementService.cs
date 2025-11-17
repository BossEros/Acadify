using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.DTOs;

namespace ASI.Basecode.Services.Implementation
{
    public class GradeManagementService : IGradeManagementService
    {
        private readonly IClassManagementRepository _repo;

        public GradeManagementService(IClassManagementRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<SemesterGradesDto>> GetSemesterGradesForStudentAsync(int studentId)
        {
            var enrollments = await _repo.GetEnrollmentsByStudentIdAsync(studentId);

            var semesterGrades = enrollments
                .GroupBy(e =>
                {
                    var month = e.EnrolledAt.Month;
                    var semester = (short)((month >= 6 && month <= 12) ? 1 : 2);
                    var schoolYearStart = (month >= 6 && month <= 12) ? e.EnrolledAt.Year : e.EnrolledAt.Year - 1;
                    return new { SchoolYearStart = schoolYearStart, Semester = semester };
                })
                .OrderByDescending(g => g.Key.SchoolYearStart)
                .ThenBy(g => g.Key.Semester)
                .Select(g => new SemesterGradesDto
                {
                    SchoolYearStart = g.Key.SchoolYearStart,
                    Semester = g.Key.Semester,
                    Grades = g.Select(e =>
                    {
                        var isPassed = e.Grade?.FinalGrade.HasValue == true || e.Grade?.MidtermGrade.HasValue == true;
                        var remarkText = !string.IsNullOrWhiteSpace(e.Grade?.Remarks)
                            ? e.Grade!.Remarks!
                            : (isPassed ? "Passed" : "Incomplete");

                        return new GradeDto
                        {
                            CourseCode = e.Class.Course!.CourseCode,
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

            return semesterGrades;
        }
    }
}